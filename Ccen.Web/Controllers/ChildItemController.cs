using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Products;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class ChildItemController : BaseController
    {
        public virtual ActionResult GetChildren(GridRequest requst,
            int market, 
            string marketplaceId, 
            int listingsMode,
            string parentAsin)
        {
            try
            {
                LogI("GetChildren, parentAsin=" + parentAsin + ", market=" + market + ", marketplaceId=" + marketplaceId +
                     ", listingsMode=" + listingsMode);

                var items = ItemViewModel.GetAll(Db,
                    (MarketType) market,
                    marketplaceId,
                    parentAsin,
                    (ListingsModeType) listingsMode)
                    .ToList()
                    .OrderBy(i => i.StyleString)
                    .ThenBy(i => i.SizeIndex)
                    .ThenBy(i => i.Size)
                    .ThenBy(i => i.Color)
                    .ThenBy(i => i.SKU)
                    .ToList();

                var data = new GridResponse<ItemViewModel>(items, items.Count);
                return JsonGet(data);
            }
            catch (Exception ex)
            {
                LogE("GetChildren", ex);
                throw;
            }
        }

        public virtual ActionResult OnUpdateItem(int id, string sku)
        {
            LogI("OnUpdateParent, id=" + id + ", sku=" + sku);

            var model = EditItemsViewModel.GetForEdit(Db, id, sku);
            ViewBag.PartialViewName = "ItemViewModel";
            return View("EditNew", model);
        }

        public virtual ActionResult Submit(EditItemsViewModel model)
        {
            var logEntry = LogI("Update Items, count=" + model.Listings.Count);
            logEntry.Info("begin");

            if (model.Listings.Count > 0)
            {
                var selectedListing = model.SelectedListing;
                if (ModelState.IsValid)
                {
                    var priceManager = new PriceManager(LogService, Time, DbFactory, ActionService, Settings);

                    foreach (var listing in model.Listings)
                    {
                        logEntry.Info("Submit, listing=", listing.ToString());

                        listing.Update(Db, 
                            LogService, 
                            priceManager,
                            StyleHistoryService,
                            Time.GetAppNowTime(), 
                            AccessManager.UserId);
                    }
                }

                logEntry.Info("end");
                return Json(new UpdateRowViewModel(selectedListing, "Products_" + selectedListing.ParentASIN, null, true), JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }
    }
}
