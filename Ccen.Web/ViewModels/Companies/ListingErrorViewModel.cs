using Amazon.Model.Implementation.Markets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Companies
{
    public class ListingErrorViewModel
    {
        public int Count { get;set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string ShortName
        {
            get
            {
                return MarketHelper.GetShortName(Market, MarketplaceId);
            }
        }
    }
}