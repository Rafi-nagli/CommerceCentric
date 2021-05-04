using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Grid
{
    public class GridRequest
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public string SortField { get; set; }
        public string SortMode { get; set; }

        public bool ClearCache { get; set; }

        public long TimeStamp { get; set; }
    }
}