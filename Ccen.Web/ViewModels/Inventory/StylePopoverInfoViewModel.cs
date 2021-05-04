using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Helpers;
using Amazon.DTO;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StylePopoverInfoViewModel
    {
        public string StyleString { get; set; }
        public IList<StyleItemViewModel> StyleItems { get; set; }

        public IList<LocationViewModel> Locations { get; set; }

        public static StylePopoverInfoViewModel GetForStyle(IUnitOfWork db, 
            string styleString,
            long? listingId)
        {
            var result = new StylePopoverInfoViewModel();
            result.StyleString = styleString;

            //Size qty
            var styleItemQuery = from si in db.StyleItemCaches.GetAllAsDto()
                join s in db.Styles.GetAllActive() on si.StyleId equals s.Id
                where s.StyleID == styleString
                select si;

            result.StyleItems = styleItemQuery.ToList()
                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                .Select(si => new StyleItemViewModel()
                {
                    Id = si.Id,
                    Quantity = Math.Max(0, si.RemainingQuantity),
                    Size = si.Size,
                }).ToList();

            //Listing Price
            if (listingId.HasValue)
            {
                var listing = db.Items.GetAllViewAsDto().FirstOrDefault(i => i.ListingEntityId == listingId.Value);
                if (listing != null)
                {
                    IList<ItemDTO> childItems = new List<ItemDTO>();
                    if (!String.IsNullOrEmpty(listing.ParentASIN))
                    {
                        childItems = db.Items.GetAllViewAsDto()
                            .Where(l => l.ParentASIN == listing.ParentASIN
                                && l.Market == listing.Market
                                && (l.MarketplaceId == listing.MarketplaceId || String.IsNullOrEmpty(listing.MarketplaceId))
                                && !l.IsFBA
                                && l.StyleString == styleString)
                            .ToList();
                    }
                    else
                    {
                        childItems = new List<ItemDTO>() { listing };
                    }

                    foreach (var childItem in childItems)
                    {
                        if (childItem.StyleItemId.HasValue)
                        {
                            var styleItem = result.StyleItems.FirstOrDefault(s => s.Id == childItem.StyleItemId);
                            if (styleItem != null)
                            {
                                styleItem.Price = childItem.SalePrice ?? childItem.CurrentPrice;
                            }
                        }
                    }
                }
            }


            //Locations
            var styleLocationQuery = from si in db.StyleLocations.GetAllAsDTO()
                        join s in db.Styles.GetAllActive() on si.StyleId equals s.Id
                        where s.StyleID == styleString
                        select si;

            result.Locations = styleLocationQuery.ToList()
                .OrderByDescending(l => l.IsDefault)
                .Select(l => new LocationViewModel()
                {
                    Isle = l.Isle,
                    Section = l.Section,
                    Shelf = l.Shelf,
                    IsDefault = l.IsDefault
                }).ToList();
            

            return result;
        }
    }
}