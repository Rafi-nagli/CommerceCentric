using Amazon.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.Models.SearchFilters
{
    public class QuantityOperationFilterViewModel
    {
        public string OrderNumber { get; set; }
        public string StyleString { get; set; }
        public long? StyleItemId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? UserId { get; set; }
        public QuantityOperationType? Type { get; set; }

        public int StartIndex { get; set; }
        public int LimitCount { get; set; }

        public static QuantityOperationFilterViewModel Empty
        {
            get { return new QuantityOperationFilterViewModel(); }
        }
    }
}