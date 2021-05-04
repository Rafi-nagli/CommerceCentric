using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.Model.Models;
using Amazon.Model.Models.EmailInfos;

namespace Amazon.Model.Implementation.Trackings.Rules
{
    public class UndeliverableAsAddressedTrackingRule : ITrackingRule
    {
        private ISystemActionService _actionService;
        private IAddressService _addressService;
        private IEmailService _emailService;
        private ILogService _log;
        private ITime _time;

        public UndeliverableAsAddressedTrackingRule(ILogService log,
            ISystemActionService actionService,
            IAddressService addressService,
            IEmailService emailService,
            ITime time)
        {
            _actionService = actionService;
            _addressService = addressService;
            _emailService = emailService;
            _log = log;
            _time = time;
        }

        public bool IsAccept(OrderToTrackDTO orderToTrackInfo,
            string status,
            DateTime? statusDate,
            IList<TrackingRecord> records)
        {
            /*TASK: When tracking history contains “Undeliverable as Addressed” like order 114-8804539-8077829
            When it’s being scanned back in Hallandale (after that),and if there is no Notes in account after the day it was scanned as undeliverable, send client an email:
            “Dear %Name%,
            Your order of %List Of pajamas% being returned to us by USPS because the address you have provided for this order is undeliverable.
            Please review full tracking history of this order at %link to USPS with tracking number%.
            Please let us know how would you like us to proceed with your order once we get it back.

            Best Regards,
            Customer Service”

            Please also add a note to the account: Order being returned, emailed customer.
            */

            var today = _time.GetAppNowTime().Date;

            //NOTE: processing only fresh records
            if (statusDate.HasValue
                && statusDate.Value < today.AddDays(-10))
            {
                _log.Info("Skip old status, pass more than 10 days");
                return false;
            }

            if (orderToTrackInfo.Carrier == ShippingServiceUtils.USPSCarrier)
            {
                //TASK: When tracking history contains “Undeliverable as Addressed”
                var undeliverableAsAddressStatus = records.FirstOrDefault(s => 
                    String.Compare(s.Message, "Undeliverable as Addressed", StringComparison.OrdinalIgnoreCase) == 0
                    || (s.Message ?? "").IndexOf("Addressee not available", StringComparison.OrdinalIgnoreCase) >= 0);
                if (undeliverableAsAddressStatus != null)
                {
                    //TASK: sent only to US and CA, for other country we always do refund
                    if (ShippingUtils.IsInternational(orderToTrackInfo.ShippingAddress.FinalCountry)
                        && orderToTrackInfo.ShippingAddress.FinalCountry != "CA")
                        return false;

                    _log.Info("Found \"Undeliverable As Addressed\"");
                    var scanInHallandale = records.FirstOrDefault(r => r.Date > undeliverableAsAddressStatus.Date
                                                                       && _addressService.IsMine(r.AsAddressDto()));
                    if (scanInHallandale != null)
                    {
                        _log.Info("Being scanned back");
                        //When it’s being scanned back in Hallandale (after that),and if there is no Notes in account after the day it was scanned as undeliverable, send client an email

                        //NOTE: disable check and send message at same day when scanned back
                        //if (scanInHallandale.Date.HasValue && _time.GetBizDaysCount(scanInHallandale.Date.Value, today) > 1)
                        //{
                        //    if (!records.Any(r => r.Date >= scanInHallandale.Date.Value
                        //                          && !_addressService.IsMine(r.AsAddressDto())))
                        //    {
                        //        _log.Info("no Notes in account after the day it was scanned. Send email.");

                        //        return true;
                        //    }
                        //    else
                        //    {
                        //        _log.Info("Found extra notes after package was scanned back");
                        //    }
                        //}

                        return true;
                    }
                }
            }

            return false;
        }

        public void Process(Core.IUnitOfWork db, 
            DTO.OrderToTrackDTO shipping, 
            string status, 
            DateTime? statusDate,
            IList<TrackingRecord> records,
            DateTime when)
        {
            if (IsAccept(shipping, status, statusDate, records))
            {
                var order = db.ItemOrderMappings.GetOrderWithItems(null, shipping.OrderNumber, unmaskReferenceStyle:false, includeSourceItems: false);
               
                var isExist = db.OrderEmailNotifies.IsExist(order.OrderId,
                    OrderEmailNotifyType.OutputUndeliveredAsAddressedEmail);

                if (!isExist)
                {
                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = order.Id,
                        Message = "Order being returned. Undelivered As Addressed email sent to customer",
                        Type = (int)CommentType.ReturnExchange,
                        CreateDate = _time.GetAppNowTime(),
                        UpdateDate = _time.GetAppNowTime()
                    });

                    var emailInfo = new UndeliverableAsAddressedRequestEmailInfo(_emailService.AddressService,
                        null,
                        order.OrderId,
                        (MarketType)order.Market,
                        order.Items,
                        shipping.Carrier,
                        shipping.TrackingNumber,
                        order.BuyerName,
                        order.BuyerEmail);

                    _emailService.SendEmail(emailInfo, CallSource.Service);

                    db.OrderEmailNotifies.Add(new OrderEmailNotify()
                    {
                        Type = (int)OrderEmailNotifyType.OutputUndeliveredAsAddressedEmail,
                        OrderNumber = shipping.OrderNumber,
                        CreateDate = when,
                    });

                    db.Commit();
                }
            }
        }
    }
}
