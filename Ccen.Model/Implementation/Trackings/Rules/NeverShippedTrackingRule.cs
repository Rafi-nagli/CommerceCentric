using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Notifications.NotificationParams;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Trackings.Rules
{
    public class NeverShippedTrackingRule : ITrackingRule
    {
        public static readonly string USPSPreShipmentInfoStatus = "Pre-Shipment Info Sent to USPS"; //OR Pre-Shipment Info Sent to USPS, USPS Awaiting Item
        public static readonly string USPSNoRecordStatus = "USPS Desc: No record of that item";
        public static readonly string USPSShipmentPickedUp = "Shipment Picked Up";
        public static readonly string USPSShipmentProcessing = "Item Accepted/Picked Up for Initial Processing";

        private INotificationService _notificationService;
        private ILogService _log;
        private ITime _time;

        public NotificationType Type
        {
            get { return NotificationType.LabelNeverShipped; }
        }

        public NeverShippedTrackingRule(ILogService logService,
            INotificationService notificationService,
            ITime time)
        {
            _log = logService;
            _time = time;
            _notificationService = notificationService;
        }

        public bool IsAccept(OrderToTrackDTO orderToTrackInfo, 
            string status, 
            DateTime? statusDate,
            IList<TrackingRecord> records)
        {
            var today = _time.GetAppNowTime().Date;
            //var now = _time.GetAppNowTime();

            //почта после 60 дней за них не отвечает
            //старше 55 дней
            if (statusDate.HasValue
                && statusDate.Value.AddDays(55) < today)
                return false;

            //5.	Don’t show “Label Never Shipped” for 36 hours after it was generated.
            //NOTE: round to 2 days
            //TASK: давай сделаем 10
            if (statusDate.HasValue
                && _time.GetBizDaysCount(statusDate.Value.Date, today) > 10) 
                return false;

            if (orderToTrackInfo.LabelCanceled)
                return false;

            if (orderToTrackInfo.ActualDeliveryDate.HasValue)
                return false;

            if (status.ToLower().Contains("delivered"))
                return false;

            if ((status.Contains(USPSPreShipmentInfoStatus)
                || status == USPSShipmentPickedUp
                || status == USPSNoRecordStatus
                || status == USPSShipmentProcessing)
                && statusDate.HasValue)
            {
                var dayCount = _time.GetBizDaysCount(statusDate.Value.Date, today);
                if (dayCount > 1)
                    return true;
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
            //USPS Desc: No record of that item
            //Pre-Shipment Info Sent to USPS

            if (IsAccept(shipping, status, statusDate, records))
            {
                var type = NotificationType.LabelNeverShipped;

                var recordId = shipping.ShipmentInfoId.HasValue
                    ? shipping.ShipmentInfoId.Value
                    : (shipping.MailInfoId ?? 0);

                var additionalParams = new LabelNeverShippedParams()
                {
                    BuyDate = shipping.BuyDate,
                    ShippingName = shipping.ShippingName,

                    Carrier = shipping.Carrier,
                    OrderNumber = shipping.OrderNumber,
                    LabelFromTypeId =
                        shipping.ShipmentInfoId.HasValue ? (int)LabelFromType.Batch : (int)LabelFromType.Mail,
                    ShippingInfoId = recordId,
                    ReasonId = shipping.ReasonId
                };

                var result = _notificationService.Add(shipping.TrackingNumber,
                    EntityType.Tracking,
                    String.Empty,

                    additionalParams,
                    
                    shipping.OrderNumber, 
                    type);

                if (result.HasValue)
                {
                    _log.Info("Added notification, type=" + type + ", id=" + shipping.TrackingNumber);
                }
            }
            else
            {
                _notificationService.RemoveExist(shipping.TrackingNumber, Type);
            }
        }
    }
}
