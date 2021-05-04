using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Orders
{
    public class CreateBatchViewModel
    {
        public long? BatchId { get; set; }
        public string BatchName { get; set; }
        public string OrderIds { get; set; }
    }
}