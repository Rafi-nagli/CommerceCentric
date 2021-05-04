using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Bargains
{
    public class BargainSearchResultViewModel
    {
        public int TotalResults { get; set; }

        public IList<BargainViewModel> Bargains { get; set; }
    }
}