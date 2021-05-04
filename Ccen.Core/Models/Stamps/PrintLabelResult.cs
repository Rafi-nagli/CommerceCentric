using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Amazon.Core.Models.Dhl;
using Amazon.DTO.Orders;

namespace Amazon.Core.Models
{
    public class PrintLabelResult
    {
        public bool IsPrintStarted { get; set; }
        public bool Success { get; set; }
        public string Url { get; set; }
        public IList<Message> Messages { get; set; }

        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }

        public decimal CostDiff { get; set; }
        public int DuplicateCount { get; set; }
        public IList<LabelPrintResult> FailedIds { get; set; }
        public IList<long> RemovedIds { get; set; }
        public long? PrintPackId { get; set; }

        public bool ResaveNumberInBatchRequested { get; set; }


        public string FailedSummary
        {
            get { return "-" + String.Join("<br/>-", FailedIds.Select(s => s.AmazonIdentifier + ": " + s.Message)); }
        }


        public IList<ScanFormInfo> ScanFormList { get; set; }

        public ScheduledPickupDTO PickupInfo { get; set; }

        public PrintLabelResult()
        {
            Success = false;
            Url = string.Empty;
            Messages = new List<Message>();
            CostDiff = 0;
            DuplicateCount = 0;
            FailedIds = new List<LabelPrintResult>();
        }

        public void AddFailedLabel(long shipmentId, long orderId, string amazonIdentifier, string message)
        {
            FailedIds.Add(new LabelPrintResult()
            {
                ShipmentId = shipmentId,
                OrderId = orderId,
                AmazonIdentifier = amazonIdentifier,
                Message = message
            });
        }

        public string GetConcatFailedOrdersString()
        {
            var message = String.Join("<br/>", FailedIds
                    .Select(f => string.Format("Unable to print order <a target='_blank' href='/Mailing/Index?orderId=" + HttpUtility.UrlEncode(f.AmazonIdentifier) + "'>{0}</a>. Reason: {1}",
                        f.AmazonIdentifier, 
                        f.Message))
                    .ToList());
            return message;
        }
    }
}
