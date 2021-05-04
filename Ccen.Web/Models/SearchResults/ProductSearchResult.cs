using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.Models.SearchResults
{
    public class ProductSearchResult
    {
        public IList<long> StyleIdList { get; set; }
        public IList<int> ChildItemIdList { get; set; }
    }
}