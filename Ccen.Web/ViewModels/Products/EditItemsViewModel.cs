using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;

namespace Amazon.Web.ViewModels.Products
{
    public class EditItemsViewModel
    {
        public int SelectedItemId { get; set; }
        public List<ItemViewModel> Listings { get; set; }

        public ItemViewModel SelectedListing
        {
            get
            {
                if (Listings != null)
                {
                    var listing = Listings.FirstOrDefault(l => l.ItemId == SelectedItemId);
                    if (listing == null)
                        return Listings.FirstOrDefault();
                    return listing;
                }
                return null;
            }
        }

        public static EditItemsViewModel GetForEdit(IUnitOfWork db, int id, string sku)
        {
            var model = new EditItemsViewModel();

            if (!String.IsNullOrEmpty(sku))
                model.Listings = db.Items
                    .GetBySKUAsDto(sku)
                    .Select(l => new ItemViewModel(l))
                    .OrderBy(l => l.MarketIndex)
                    .ToList();
            else
                model.Listings = new List<ItemViewModel>()
                {
                    new ItemViewModel(db.Items.GetByIdAsDto(id))
                };
            
            model.SelectedItemId = id;

            return model;
        }
    }
}