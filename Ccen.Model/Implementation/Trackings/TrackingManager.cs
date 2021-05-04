using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Trackings.Rules;


namespace Amazon.Model.Implementation
{
    public class TrackingManager
    {
        private ITime _time;
        private ILogService _log;
        private IAddressService _addressService;
        private IEmailService _emailService;
        private ISystemActionService _actionService;

        private IList<ITrackingRule> _ruleList;

        private string[] _deliveryEventList = new[]
        {
            "Delivered, In/At Mailbox",
            "Delivered, Front Door/Porch",
            "Delivered, Front Desk/Reception",
            "Tendered to Final Delivery Agent", //?
            //"Attempted Delivery - Item being held, addressee being notified", //?
            "Delivered, Left with Individual",
            "Delivered, PO Box",
            "Delivered, To Agent",
            "Delivered, Parcel Locker",
            "Delivered, Individual Picked Up at Post Office",
            "Delivered, Garage or Other Location at Address",
            "Delivered, Individual Picked Up at Postal Facility",
            "Delivered, To Mail Room",
            "Delivered, Neighbor as Requested",
            "DELIVERED",
        };



        public TrackingManager(ILogService log,
            ISystemActionService actionService,
            IAddressService addressService,
            IEmailService emailService,
            ITime time,
            List<ITrackingRule> ruleList)
        {
            _log = log;
            _time = time;
            _actionService = actionService;
            _emailService = emailService;
            _addressService = addressService;
            _ruleList = ruleList;
        }

        public IList<TrackingState> UpdateOrderTracking(
            IUnitOfWork db,
            CompanyDTO company,
            IList<OrderToTrackDTO> shippings,
            ITrackingProvider trackingProvider)
        {
            _log.Info("UpdateOrderTracking, shippings=" + shippings.Count);
            try
            {
                var ruleList = _ruleList;

                var trackingList = shippings
                    .Where(o => !String.IsNullOrEmpty(o.TrackingNumber))
                    .Select(o => new TrackingNumberToCheckDto()
                    {
                        TrackingNumber = o.TrackingNumber,
                        ToCountry = o.ShippingAddress != null ? o.ShippingAddress.FinalCountry : null,                        
                    }).ToList();

                var stateList = trackingProvider.TrackShipments(trackingList);
                var today = _time.GetAppNowTime();

                _log.Info("UpdateOrderTracking, track count get back=" + stateList.Count);
                var processedShipmentIds = new List<long>();
                var processedMailInfoIds = new List<long>();
                foreach (var state in stateList)
                {
                    //NOTE: Fedex has delivery records not as last record (ex.: 393418292108)
                    var deliveredRecord = state.Records != null ? state.Records.FirstOrDefault(r => _deliveryEventList.Contains(r.Message ?? "") || (r.Message ?? "").ToLower().StartsWith("delivered")) : null;

                    var lastRecord = deliveredRecord != null ? deliveredRecord : (state.Records != null ? state.Records.FirstOrDefault() : null);
                    var isBack = lastRecord != null ? _addressService.IsMine(lastRecord.AsAddressDto()) : false;

                    var shippingDto = shippings.FirstOrDefault(sh => sh.TrackingNumber == state.TrackingNumber);
                    if (shippingDto != null)
                    {
                        var hasStateChanges = false;
                        if (lastRecord == null)
                        {
                            hasStateChanges = false;
                            shippingDto.TrackingRequestAttempts = shippingDto.TrackingRequestAttempts + 1;
                            shippingDto.LastTrackingRequestDate = _time.GetUtcTime();
                            _log.Warn("UpdateOrderTracking, empty Records!");
                        }
                        else
                        {
                            _log.Info("UpdateOrderTracking, o=" + shippingDto.OrderNumber + ", tr=" +
                                      shippingDto.TrackingNumber + ", at=" + shippingDto.OrderDate + ", new state=" +
                                      lastRecord.Message + " at=" + lastRecord.Date);

                            var newStateEvent = StringHelper.Substring(lastRecord.Message, 100);
                            hasStateChanges = shippingDto.TrackingStateDate != lastRecord.Date
                                                  || shippingDto.TrackingStateEvent != newStateEvent;

                            shippingDto.TrackingStateSource = (int) lastRecord.Source;
                            shippingDto.TrackingStateDate = lastRecord.Date;
                            shippingDto.TrackingStateEvent = newStateEvent;
                            shippingDto.TrackingLocation = lastRecord.Country + ", " + lastRecord.State + ", " +
                                                           lastRecord.City + ", " + lastRecord.Zip;

                            if (_deliveryEventList.Contains(lastRecord.Message)
                                || lastRecord.Message.ToLower().StartsWith("delivered")
                                || (lastRecord.Message.ToLower().Contains("shipment picked up")
                                    && (state.Records.Count > 5 && lastRecord.Date < today.AddDays(-2))))
                                //NOTE: temporary solution for "Shipment Picked Up", every shipment may have transit status with that name (not final)
                            {
                                shippingDto.DeliveredStatus = isBack
                                    ? (int) DeliveredStatusEnum.DeliveredToSender
                                    : (int) DeliveredStatusEnum.Delivered;

                                shippingDto.IsDelivered = true;
                                shippingDto.ActualDeliveryDate = lastRecord.Date;
                            }
                            else
                            {
                                //May happend like with refused tracking number https://tools.usps.com/go/TrackConfirmAction!input.action?tRef=qt&tLc=1&tLabels=9400116901495496872649
                                //Order #1577164597317
                                shippingDto.DeliveredStatus = (int) DeliveredStatusEnum.None;
                                shippingDto.IsDelivered = false;
                                shippingDto.ActualDeliveryDate = null;
                            }

                            //For DHL
                            if (lastRecord.Message.ToLower().StartsWith("Returned to shipper"))
                            {
                                shippingDto.DeliveredStatus = (int) DeliveredStatusEnum.DeliveredToSender;
                                shippingDto.IsDelivered = true;
                                shippingDto.ActualDeliveryDate = lastRecord.Date;
                            }

                            shippingDto.LastTrackingRequestDate = _time.GetUtcTime();

                            if (!hasStateChanges)
                                shippingDto.TrackingRequestAttempts = shippingDto.TrackingRequestAttempts + 1;
                            else
                                shippingDto.TrackingRequestAttempts = 1;

                            //NOTE: If state no changes for a long time => no requests
                            if (lastRecord.Date < _time.GetAppNowTime().AddDays(-50)
                                || (shippingDto.OrderDate < _time.GetAppNowTime().AddDays(-90)
                                    && lastRecord.Message.StartsWith(TrackingHelper.UndefinedPrefix)))
                            {
                                shippingDto.TrackingRequestAttempts = 10000;
                                if (lastRecord.Message.StartsWith(TrackingHelper.UndefinedPrefix))
                                {
                                    _log.Info("Info Unavailable, message=" + lastRecord.Message);
                                    shippingDto.DeliveredStatus = (int) DeliveredStatusEnum.InfoUnavailable;
                                }
                                _log.Info("Last attempt (= 10000)");
                            }
                        }

                        
                        if (shippingDto.ShipmentInfoId.HasValue)
                        {
                            if (processedShipmentIds.Any(sh => sh == shippingDto.ShipmentInfoId.Value))
                            {
                                _log.Info("Shipment with that Id already processed: " + shippingDto.ShipmentInfoId.Value);
                            }
                            else
                            {
                                processedShipmentIds.Add(shippingDto.ShipmentInfoId.Value);

                                var changeFields = new List<Expression<Func<OrderShippingInfo, object>>>()
                                {
                                    p => p.LastTrackingRequestDate,
                                    p => p.TrackingRequestAttempts
                                };
                                if (hasStateChanges)
                                {
                                    changeFields.Add(p => p.TrackingStateSource);
                                    changeFields.Add(p => p.TrackingStateDate);
                                    changeFields.Add(p => p.TrackingStateEvent);
                                    changeFields.Add(p => p.TrackingLocation);

                                    changeFields.Add(p => p.DeliveredStatus);
                                    changeFields.Add(p => p.IsDelivered);
                                    changeFields.Add(p => p.ActualDeliveryDate);
                                }
                                db.OrderShippingInfos.TrackItem(new OrderShippingInfo()
                                    {
                                        Id = shippingDto.ShipmentInfoId.Value,
                                        TrackingStateSource = shippingDto.TrackingStateSource,
                                        TrackingStateDate = shippingDto.TrackingStateDate,
                                        TrackingStateEvent = shippingDto.TrackingStateEvent,
                                        TrackingLocation = shippingDto.TrackingLocation,
                                        DeliveredStatus = shippingDto.DeliveredStatus,
                                        IsDelivered = shippingDto.IsDelivered,
                                        ActualDeliveryDate = shippingDto.ActualDeliveryDate,
                                        LastTrackingRequestDate = shippingDto.LastTrackingRequestDate,
                                        TrackingRequestAttempts = shippingDto.TrackingRequestAttempts ?? 1
                                    },
                                    changeFields);
                            }
                        }

                        if (shippingDto.MailInfoId.HasValue)
                        {
                            if (processedMailInfoIds.Any(sh => sh == shippingDto.MailInfoId.Value))
                            {
                                _log.Info("Shipment with that Id already processed: " + shippingDto.MailInfoId.Value);
                            }
                            else
                            {
                                processedMailInfoIds.Add(shippingDto.MailInfoId.Value);

                                var changeFields = new List<Expression<Func<MailLabelInfo, object>>>()
                                {
                                    p => p.LastTrackingRequestDate,
                                    p => p.TrackingRequestAttempts
                                };
                                if (hasStateChanges)
                                {
                                    changeFields.Add(p => p.TrackingStateSource);
                                    changeFields.Add(p => p.TrackingStateDate);
                                    changeFields.Add(p => p.TrackingStateEvent);
                                    changeFields.Add(p => p.TrackingLocation);

                                    changeFields.Add(p => p.DeliveredStatus);
                                    changeFields.Add(p => p.IsDelivered);
                                    changeFields.Add(p => p.ActualDeliveryDate);
                                }
                                db.MailLabelInfos.TrackItem(new MailLabelInfo()
                                {
                                    Id = shippingDto.MailInfoId.Value,
                                    TrackingStateSource = shippingDto.TrackingStateSource,
                                    TrackingStateDate = shippingDto.TrackingStateDate,
                                    TrackingStateEvent = shippingDto.TrackingStateEvent,
                                    TrackingLocation = shippingDto.TrackingLocation,
                                    DeliveredStatus = shippingDto.DeliveredStatus,
                                    IsDelivered = shippingDto.IsDelivered,
                                    ActualDeliveryDate = shippingDto.ActualDeliveryDate,
                                    LastTrackingRequestDate = shippingDto.LastTrackingRequestDate,
                                    TrackingRequestAttempts = shippingDto.TrackingRequestAttempts ?? 1
                                },
                                changeFields);
                            }
                        }

                        //NOTE: if do checks only when state changed, the rules based on calculate business day were never activated
                        //if (hasStateChanges) 
                        if (lastRecord != null)
                        {
                            CheckRules(db,
                                shippingDto,
                                lastRecord.Message,
                                lastRecord.Date,
                                state.Records,
                                ruleList);
                        }
                    }
                }
                db.Commit();

                return stateList;
            }
            catch (Exception ex)
            {
                _log.Error("UpdateOrderTracking", ex);
            }
            return null;
        }


        public void CheckRules(IUnitOfWork db,
            OrderToTrackDTO shipping, 
            string status, 
            DateTime? statusDate,
            IList<TrackingRecord> records,
            IList<ITrackingRule> ruleList)
        {
            foreach (var rule in ruleList)
            {
                try
                {
                    rule.Process(db, 
                        shipping, 
                        status, 
                        statusDate, 
                        records, 
                        _time.GetAppNowTime());
                }
                catch (Exception ex)
                {
                    _log.Error("Can't process tracking rule, for order #: " + shipping.OrderNumber, ex);
                }
            }
        }
    }
}
