using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Notifications.NotificationParams;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Trackings.Rules
{
    public class GetStuckTrackingRule : ITrackingRule
    {
        private INotificationService _notificationService;
        private ILogService _log;
        private ITime _time;

        public NotificationType Type
        {
            get { return NotificationType.LabelGotStuck; }
        }

        public GetStuckTrackingRule(ILogService log,
            INotificationService notificationService,
            ITime time)
        {
            _time = time;
            _log = log;
            _notificationService = notificationService;
        }

        public bool IsAccept(OrderToTrackDTO orderToTrackInfo, 
            string status, 
            DateTime? statusDate,
            IList<TrackingRecord> records)
        {
            var today = _time.GetAppNowTime().Date;

            if (orderToTrackInfo.LabelCanceled == true
                || orderToTrackInfo.CancelLabelRequested == true)
                return false;

            //почта после 60 дней за них не отвечает
            //старше 55 дней
            if (statusDate.HasValue
                && statusDate.Value.AddDays(55) < today)
                return false;

            if (orderToTrackInfo.LabelCanceled)
                return false;

            if (orderToTrackInfo.ActualDeliveryDate.HasValue)
                return false;

            if (status.IndexOf("Pre-Shipment Info Sent to USPS", StringComparison.InvariantCultureIgnoreCase) > 0)
                return false;
            
            if (statusDate.HasValue && orderToTrackInfo.ShippingMethodId.HasValue)
            {
                var isInternational = ShippingUtils.IsInternationalShippingType(orderToTrackInfo.ShippingMethodId.Value);
                if (!isInternational)
                {
                    var shippingType = ShippingUtils.GetShippingType(orderToTrackInfo.ShippingMethodId.Value);

                    var periodDays = 4;
                    switch (shippingType)
                    {
                        case ShippingTypeCode.Standard:
                            periodDays = 4;
                            break;
                        case ShippingTypeCode.Priority:
                            periodDays = 2;
                            break;
                        case ShippingTypeCode.PriorityExpress:
                            periodDays = 1;
                            break;
                        case ShippingTypeCode.SameDay:
                            periodDays = 1; //TODO: 0.5
                            break;
                    }

                    var dayCount = _time.GetBizDaysCount(statusDate.Value, today);
                    if (dayCount > periodDays)
                        return true;
                }
            }

            return false;
        }

        public void Process(Core.IUnitOfWork db, 
            OrderToTrackDTO shipping, 
            string status, 
            DateTime? statusDate,
            IList<TrackingRecord> records,
            DateTime when)
        {
            if (IsAccept(shipping, status, statusDate, records))
            {
                var recordId = shipping.ShipmentInfoId.HasValue
                    ? shipping.ShipmentInfoId.Value
                    : (shipping.MailInfoId ?? 0);

                var additionalParams = new LabelGetStuckParams()
                {
                    Status = status,
                    StatusDate = statusDate,
                    ShippingName = shipping.ShippingName,
                    Carrier = shipping.Carrier,
                    OrderNumber = shipping.OrderNumber,
                    LabelFromTypeId =
                        shipping.ShipmentInfoId.HasValue ? (int) LabelFromType.Batch : (int) LabelFromType.Mail,
                    ShippingInfoId = recordId,
                    ReasonId = shipping.ReasonId
                };

                var result = _notificationService.Add(shipping.TrackingNumber,
                    EntityType.Tracking,
                    String.Empty,
                    additionalParams,
                    shipping.OrderNumber,
                    Type);

                if (result.HasValue)
                {
                    _log.Info("Added notification, type=" + Type + ", id=" + shipping.TrackingNumber);
                }
            }
            else
            {
                _notificationService.RemoveExist(shipping.TrackingNumber, Type);
            }
        }
    }
}
