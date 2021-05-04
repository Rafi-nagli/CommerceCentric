using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Listings;

namespace Amazon.Web.ViewModels.Products.Edits
{
    public class AddVariationViewModel
    {
        public string StyleString { get; set; }
        public IList<ItemSizeMapping> ExistSizes { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string WalmartUrl { get; set; }

        public override string ToString()
        {
            return "styleItemIds=" +
                   (ExistSizes != null ? String.Join(",", ExistSizes.Select(s => s.StyleItemId.ToString())) : "")
                   + ", market=" + Market + ", marketplaceId=" + MarketplaceId;
        }
    }
}