using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels
{
    public class PrintOrderParamModel
    {
        public string OrderIds { get; set; }
        public long? BatchId { get; set; }
        public int SortMode { get; set; }
    }
}