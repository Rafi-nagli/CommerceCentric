using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models.EmailInfos
{
    public class DhlPickupScheduleErrorEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.DhlPickupScheduleErrorToSeller; }
        }
        
        public string ErrorMessage { get; set; }
        public DateTime PickupDate { get; set; }
        public TimeSpan ReadyByTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        
        public string Subject
        {
            get { return String.Format("[System notification] Dhl Pickup scheduling error (Batch: {0})", Tag); }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dhl Pickup scheduling error for batch: {0}, pickup date: {1}, ready by time: {2}, close time: {3}<br/>
                                Details: {4}</p>
                                <br/>
                                <p>System Notification</p></div>", 
                                                                 Tag,
                                                                 PickupDate.ToString("MM/dd/yyyy"),
                                                                 ReadyByTime.ToString("hh\\:mm"),
                                                                 CloseTime.ToString("hh\\:mm"),
                                                                 ErrorMessage);
            }
        }

        public DhlPickupScheduleErrorEmailInfo(IAddressService addressService, 
            string batchId,
            string errorMessage,
            DateTime pickupDate,
            TimeSpan readyByTime,
            TimeSpan closeTime,
            string sellerName,
            string sellerEmail) : base(addressService)
        {
            Tag = batchId;
            ErrorMessage = errorMessage;
            PickupDate = pickupDate;
            ReadyByTime = readyByTime;
            CloseTime = closeTime;

            ToEmail = sellerEmail;
            ToName = sellerName;
        }
    }
}
