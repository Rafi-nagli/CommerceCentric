using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO.Listings;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Products;
using Amazon.Web.ViewModels.Products.Edits;
using Amazon.Web.ViewModels.Results;
using WebGrease.Css.Extensions;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class ItemExportController : BaseController
    {
        public override string TAG
        {
            get { return "ItemExportController."; }
        }

        private const string PopupContentView = "ItemExportViewModel";

        public virtual ActionResult OnExport(int id)
        {
            LogI("OnEdit, id=" + id);

            var parent = Db.ParentItems.GetAsDTO(id);
            IList<MessageString> messages = null;

            var model = ItemExportViewModel.FromParentASIN(Db,
                parent.ASIN,
                parent.Market,
                parent.MarketplaceId,
                out messages);

            ViewBag.PartialViewName = PopupContentView;

            return View("EditEmpty", model);
        }
        
        [HttpPost]
        public virtual ActionResult CreateStyleVariations(AddVariationViewModel model)
        {
            LogI("GetStyleVariations, styleString=" + model.StyleString);
            
            var items = ItemExportViewModel.CreateStyleVariations(Db,
                model.StyleString,
                model.ExistSizes,
                (MarketType)model.Market,
                model.MarketplaceId);
            return JsonGet(CallResult<IList<ItemVariationExportViewModel>>.Success(items));
        }

        public virtual ActionResult Submit(ItemExportViewModel model)
        {
            LogI("Submit, model=" + model);

            model.PrepareData();

            //Save
            if (ModelState.IsValid)
            {
                IList<MessageString> messages;

                if (model.IsValid(Db, out messages))
                {
                    var excelUrl = model.Export(Db,
                        Time,
                        LogService,
                        BarcodeService,
                        AmazonCategoryService,
                        MarketplaceService,
                        Time.GetAppNowTime(),
                        AccessManager.UserId);

                    return Json(new UpdateRowViewModel(model,
                        null,
                        null,
                        false,
                        excelUrl));
                }
                else
                {
                    messages.ForEach(m => ModelState.AddModelError("model", m.Message));
                }
            }

            return PartialView(PopupContentView, model);
        }
        
        public virtual ActionResult GetUnusedBarcodeForStyleItem(long styleItemId,
            int market,
            string marketplaceId)
        {
            LogI("GetUnusedBarcodeForStyleItemId");

            var barcode = ItemEditViewModel.GetUnusedBarcodeForStyleItem(Db, styleItemId, (MarketType)market, marketplaceId);

            return JsonGet(ValueResult<string>.Success("", barcode));
        }
    }
}
