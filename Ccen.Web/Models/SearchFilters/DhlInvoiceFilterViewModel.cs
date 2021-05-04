using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;

namespace Amazon.Web.Models.SearchFilters
{
    public class DhlInvoiceFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public string OrderNumber { get; set; }
        public int? Status { get; set; }

        public SelectList StatusList
        {
            get
            {
                return new SelectList(NotificationTypes, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> NotificationTypes = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(DhlInvoiceHelper.ToString(DhlInvoiceStatusEnum.Matched), (int)DhlInvoiceStatusEnum.Matched),
            new KeyValuePair<string, int>(DhlInvoiceHelper.ToString(DhlInvoiceStatusEnum.Incorrect), (int)DhlInvoiceStatusEnum.Incorrect),
            new KeyValuePair<string, int>(DhlInvoiceHelper.ToString(DhlInvoiceStatusEnum.OrderNotFound), (int)DhlInvoiceStatusEnum.OrderNotFound),
            new KeyValuePair<string, int>(DhlInvoiceHelper.ToString(DhlInvoiceStatusEnum.DhlNotified), (int)DhlInvoiceStatusEnum.DhlNotified),
            new KeyValuePair<string, int>(DhlInvoiceHelper.ToString(DhlInvoiceStatusEnum.RefundApproved), (int)DhlInvoiceStatusEnum.RefundApproved),
            new KeyValuePair<string, int>(DhlInvoiceHelper.ToString(DhlInvoiceStatusEnum.Rejected), (int)DhlInvoiceStatusEnum.Rejected),
        };

        public static DhlInvoiceFilterViewModel Empty
        {
            get
            {
                return new DhlInvoiceFilterViewModel()
                {

                };
            }
        }
    }
}