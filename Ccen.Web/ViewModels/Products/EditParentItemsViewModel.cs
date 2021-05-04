using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;

namespace Amazon.Web.ViewModels.Products
{
    public class EditParentItemsViewModel
    {
        public int SelectedItemId { get; set; }
        public List<ParentItemViewModel> ParentItems { get; set; }

        public ParentItemViewModel SelectedItem
        {
            get
            {
                if (ParentItems != null)
                {
                    var listing = ParentItems.FirstOrDefault(l => l.Id == SelectedItemId);
                    if (listing == null)
                        return ParentItems.FirstOrDefault();
                    return listing;
                }
                return null;
            }
        }

        public static EditParentItemsViewModel GetForEdit(IUnitOfWork db, int id)
        {
            var model = new EditParentItemsViewModel();

            model.ParentItems = db.ParentItems
                    .GetSimilarByChildSkuAsDto(id)
                    .Select(l => new ParentItemViewModel(db, l))
                    .OrderBy(l => l.MarketIndex).ToList();
            
            model.SelectedItemId = id;

            return model;
        }
    }
}