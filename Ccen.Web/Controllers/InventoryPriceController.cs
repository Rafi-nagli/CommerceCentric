using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Core.Models.Calls;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Inventory.Prices;
using Amazon.Web.ViewModels.Messages;


namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryPriceController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryPriceController."; }
        }

        private const string PopupContentView = "StylePriceViewModel";

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult EditStylePrice(long styleId)
        {
            LogI("EditStylePrice, id=" + styleId);

            var model = new StylePriceViewModel(Db, styleId, Time.GetAppNowTime());
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }
        

        [Authorize(Roles = AccessManager.AllBaseRole)]
        [HttpPost]
        public virtual ActionResult Submit(StylePriceViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var styleItemHistory = new StyleItemHistoryService(LogService, Time, DbFactory);

                model.Apply(Db, 
                    DbFactory,
                    LogService,
                    Cache,
                    PriceManager,
                    styleItemHistory,
                    ActionService,
                    Time.GetAppNowTime(), 
                    AccessManager.UserId);

                var outputModel = StyleViewModel.GetAll(Db, new StyleSearchFilterViewModel()
                {
                    StyleId = model.StyleId
                }).Items?.FirstOrDefault();

                return Json(new UpdateRowViewModel(outputModel, 
                    "Styles",
                    new[]
                    {
                        "UsedBoxQuantity",
                        "HasManuallyQuantity",
                        "ManuallyQuantity",
                        "RemainingQuantity",
                        "TotalQuantity",
                        "BoxQuantity",
                        "InventoryQuantity",
                        "MarketsSoldQuantity",
                        "ScannedSoldQuantity",
                        "SentToFBAQuantity",
                        "SpecialCaseQuantity",
                        "TotalMarketsSoldQuantity",
                        "TotalScannedSoldQuantity",
                        "TotalSentToFBAQuantity",
                        "TotalSpecialCaseQuantity",
                        "QuantityModeName",
                        "StyleItemCaches",
                    },
                    false));
            }
            return View(PopupContentView, model);
        }

        public virtual ActionResult GetListingsByStyleSize(long styleItemId,
            decimal? initSalePrice,
            decimal? initSFPSalePrice)
        {
            LogI("GetListingsByStyleSize, styleItemId=" + styleItemId 
                + ", initSalePrice=" + initSalePrice
                + ", initSFPSalePrice=" + initSFPSalePrice);

            var results = MarketPriceEditViewModel.GetForStyleItemId(Db,
                DbFactory,
                styleItemId,
                initSalePrice,
                initSFPSalePrice);

            return JsonGet(ValueResult<IList<MarketPriceEditViewModel>>.Success("", results));
        }

        public virtual ActionResult SetListingsForStyleSize(long styleItemId,
            IList<MarketPriceEditViewModel> markets)
        {
            LogI("SetListingForStyleSize, styleItemId=" + styleItemId 
                + ", markets=" + (markets != null ? String.Join("\r\n", markets.Select(m => m.ToString())) : "-"));

            var results = MarketPriceEditViewModel.ApplySale(Db,
                LogService,
                styleItemId,
                markets,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(ValueResult<IList<MarketPriceViewViewModel>>.Success("", results));
        }
    }
}
