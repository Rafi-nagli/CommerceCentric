using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Trackings.Rules
{
    public class NoticeLeftTrackingRule : ITrackingRule
    {
        private ISystemActionService _actionService;
        private IAddressService _addressService;
        private ILogService _log;
        private ITime _time;

        public NoticeLeftTrackingRule(ILogService log,
            ISystemActionService actionService,
            IAddressService addressService,
            ITime time)
        {
            _actionService = actionService;
            _addressService = addressService;
            _log = log;
            _time = time;
        }

        public bool IsAccept(OrderToTrackDTO orderToTrackInfo,
            string status,
            DateTime? statusDate,
            IList<TrackingRecord> records)
        {
            var now = _time.GetAppNowTime();
            var today = _time.GetAppNowTime().Date;

            var lastRecord = records.FirstOrDefault();
            var packageGoBack = (lastRecord != null ? _addressService.IsMine(lastRecord.AsAddressDto()) : false)
                || orderToTrackInfo.DeliveredStatus == (int)DeliveredStatusEnum.DeliveredToSender;

            //почта после 60 дней за них не отвечает
            //старше 55 дней
            if (statusDate.HasValue
                && statusDate.Value.AddDays(55) < today)
                return false;

            if (packageGoBack)
                return false; //No actions if package go back

            if (orderToTrackInfo.Carrier == ShippingServiceUtils.USPSCarrier)
            {
                if (statusDate.HasValue
                    && !_time.IsBusinessDay(statusDate.Value)
                    && _time.GetBizDaysCount(statusDate.Value, today) < 2)
                {
                    //TASK: Let’s not send Exception messages for First Class packages if they were not delivered on Weekends/holidays.
                    if (orderToTrackInfo.ShippingMethodId.HasValue
                        &&
                        ShippingUtils.GetShippingType(orderToTrackInfo.ShippingMethodId.Value) ==
                        ShippingTypeCode.Standard)
                    {
                        //Don’t send notice to first class orders which USPS tried to deliver on Saturday. 
                        //They will automatically will redeliver it on the next business day.
                        return false;
                    }


                    //TASK: if any package couldn’t be delivered to FF on weekends/holidays
                    if (AddressHelper.IsFFAddress(orderToTrackInfo.ShippingAddress))
                    {
                        return false;
                    }
                }

                //TASK: If First Class order wasn’t delivered like 102-1600419-5915438 check again in 24 hours, and only then send notifications.
                if (orderToTrackInfo.ShippingMethodId.HasValue
                    &&
                    ShippingUtils.GetShippingType(orderToTrackInfo.ShippingMethodId.Value) == ShippingTypeCode.Standard)
                {
                    var statusNextBusiessDay = _time.AddBusinessDays(statusDate, 1);
                    if (statusNextBusiessDay > now)
                        return false;
                }

                if (!String.IsNullOrEmpty(status)
                    && (status.IndexOf("Notice Left", StringComparison.InvariantCultureIgnoreCase) >= 0
                        || status.IndexOf("Receptacle Blocked", StringComparison.CurrentCultureIgnoreCase) >= 0
                        || status.IndexOf("Available for Pickup", StringComparison.InvariantCultureIgnoreCase) >= 0
                        || status.IndexOf("Business Closed", StringComparison.OrdinalIgnoreCase) >= 0
                        || status.IndexOf("Undeliverable as Addressed", StringComparison.OrdinalIgnoreCase) >= 0
                        || status.IndexOf("Addressee not available", StringComparison.OrdinalIgnoreCase) >= 0)
                    && statusDate.HasValue)
                {
                    return true;
                }
            }

            if (orderToTrackInfo.Carrier == ShippingServiceUtils.DHLCarrier
                || orderToTrackInfo.Carrier == ShippingServiceUtils.DHLMXCarrier)
            {
                if (!String.IsNullOrEmpty(status)
                    && status.IndexOf("Delivery attempted; recipient not home", StringComparison.InvariantCultureIgnoreCase) >= 0
                    && statusDate.HasValue)
                {
                    return true;
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
            /*
             * NO AUTHORIZED RECIPIENT AVAILABLE
                Notice Left
                Notice Left (No Authorized Recipient Available)
                Notice Left (No Secure Location Available)
                Notice Left (Receptacle Full/Item Oversized)
             */
            /*
             * Please send message similar to Notice Left to orders with status “Available for pickup”.
                Like 115-0119209-8758666
            */
            var labelType = shipping.MailInfoId.HasValue ? LabelFromType.Mail : LabelFromType.Batch;
            var shippingId = shipping.MailInfoId.HasValue ? shipping.MailInfoId.Value : shipping.ShipmentInfoId.Value;

            if (IsAccept(shipping, status, statusDate, records))
            {
                if (!db.OrderEmailNotifies.IsExist(shipping.OrderNumber, OrderEmailNotifyType.OutputNoticeLeft))
                {
                    _log.Info(String.Format("Order: {0}, has status: {1}",
                        shipping.OrderNumber,
                        status));

                    var sendEmailActionId = _actionService.AddAction(db,
                        SystemActionType.SendEmail,
                        shipping.OrderNumber,
                        new SendEmailInput()
                        {
                            EmailType = EmailTypes.NoticeLeft,
                            OrderId = shipping.OrderNumber,
                            Args = new Dictionary<string, string>()
                            {
                                {"LabelType", labelType.ToString()},
                                {"ShippingOrderId", shippingId.ToString()}
                            }
                        },
                        null,
                        null);

                    _actionService.AddAction(db,
                        SystemActionType.AddComment,
                        shipping.OrderNumber,
                        new AddCommentInput()
                        {
                            OrderId = shipping.OrderId,
                            Message = "[System] Notice Left email sent",
                            Type = (int) CommentType.OutputEmail,
                        },
                        sendEmailActionId,
                        null);


                    db.OrderEmailNotifies.Add(new OrderEmailNotify()
                    {
                        Type = (int) OrderEmailNotifyType.OutputNoticeLeft,
                        OrderNumber = shipping.OrderNumber,
                        CreateDate = when,
                    });
                    db.Commit();
                }
                else
                {
                    _log.Info("Notice left already sent");
                }
            }
        }
    }
}
