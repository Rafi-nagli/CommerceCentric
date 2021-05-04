using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation;
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
    public partial class ItemEditController : BaseController
    {
        public override string TAG
        {
            get { return "ItemEditController."; }
        }

        private const string PopupContentView = "ItemEditViewModel";

        public virtual ActionResult OnCreate(MarketType market, string marketplaceId)
        {
            LogI("OnCreate, market=" + market + ", marketplaceId=" + marketplaceId);

            var model = new ItemEditViewModel()
            {
                Market = (int)market,
                MarketplaceId = marketplaceId
            };

            ViewBag.PartialViewName = PopupContentView;

            return View("EditEmpty", model);
        }

        public virtual ActionResult OnEdit(int id)
        {
            LogI("OnEdit, id=" + id);

            var parent = Db.ParentItems.GetAsDTO(id);
            IList<MessageString> messages = null;

            var model = ItemEditViewModel.Edit(Db,
                parent.ASIN,
                parent.Market,
                parent.MarketplaceId);

            ViewBag.PartialViewName = PopupContentView;

            return View("EditEmpty", model);
        }

        public virtual ActionResult Validate(ItemEditViewModel model)
        {
            LogI("Validate, model=" + model);
            model.PrepareData();
            var messages = model.ValidateAsync(Db, LogService, Time.GetAppNowTime());
            foreach (var message in messages)
            {
                LogI("Validate, message=" + message.Message);
            }
            return JsonGet(new ValueResult<IList<MessageString>>(true, "", messages));
        }

        public virtual ActionResult Clone(int id)
        {
            LogI("Clone, id=" + id);

            IList<MessageString> messages = null;

            var model = ItemEditViewModel.Clone(Db, 
                Cache, 
                BarcodeService,
                ActionService,
                AutoCreateListingService,
                ItemHistoryService,
                id, 
                Time.GetAppNowTime(), 
                AccessManager.UserId,
                null,
                out messages);

            return JsonGet(CallResult<ItemEditViewModel>.Success(model));
        }

        public virtual ActionResult OnCopy(int id)
        {
            LogI("OnRequestBarcode");

            var model = new ItemCopyViewModel();
            model.Init(Db, id);

            return View("ItemCopyViewModel", model);
        }

        [HttpPost]
        public virtual ActionResult Copy(ItemCopyViewModel model)
        {
            LogI("Copy, id=" + model.Id);

            IList<MessageString> messages = null;

            ItemCopyViewModel.CopyToMarketplaces(Db,
                Cache,
                BarcodeService,
                ActionService,
                AutoCreateListingService,
                ItemHistoryService,
                model.Id,
                Time.GetAppNowTime(),
                AccessManager.UserId,
                model.Marketplaces,
                out messages);

            return JsonGet(CallResult<ItemEditViewModel>.Success(null));
        }

        public virtual ActionResult SendProductUpdate(int id)
        {
            LogI("SendProductUpdate, id=" + id);

            var when = Time.GetAppNowTime();
            ItemEditViewModel.SendProductUpdate(Db,
                ActionService,
                id,
                when,
                AccessManager.UserId);

            return JsonGet(CallResult<DateTime>.Success(when));
        }

        public virtual ActionResult Delete(int id)
        {
            LogI("Delete, id=" + id);

            IList<MessageString> messages = null;

            ItemEditViewModel.Delete(Db,
                id,
                Time.GetAppNowTime(),
                AccessManager.UserId,
                out messages);

            return JsonGet(CallMessagesResultVoid.Success());
        }

        public virtual ActionResult CreateStyleVariations(AddVariationViewModel model)
        {
            LogI("GetStyleVariations, styleString=" + model.StyleString);

            var items = ItemEditViewModel.CreateStyleVariations(Db,
                LogService,
                model.StyleString,
                model.ExistSizes,
                model.WalmartUrl,
                (MarketType)model.Market,
                model.MarketplaceId,
                null);
            return JsonGet(CallResult<StyleVariationListViewModel>.Success(items));
        }

        [HttpPost]
        public virtual ActionResult Submit(ItemEditViewModel model)
        {
            LogI("Submit, model=" + model);

            model.PrepareData();

            //Save
            if (ModelState.IsValid)
            {
                IList<MessageString> messages;

                if (model.IsValid(Db, out messages))
                {
                    model.Save(Db,
                        Cache,
                        BarcodeService,
                        ActionService,
                        ItemHistoryService,
                        Time.GetAppNowTime(),
                        AccessManager.UserId);

                    return Json(new UpdateRowViewModel(model,
                        "Products",
                        null,
                        false));
                }
                else
                {
                    messages.ForEach(m => ModelState.AddModelError("model", m.Message));
                }
            }

            return PartialView(PopupContentView, model);
        }

        public virtual ActionResult GetStyleDesc(string styleString)
        {
            LogI("GetStyleDesc, styleString=" + styleString);

            var model = StyleDescViewModel.GetStyleDesc(DbFactory, styleString);

            return JsonGet(new ValueResult<StyleDescViewModel>(true, null, model));
        }

        [HttpPost]
        public virtual ActionResult UpdateStyleDesc(StyleDescViewModel model)
        {
            LogI("UpdateStyleDesc, styleString=" + model.StyleString);

            model.UpdateStyleDesc(DbFactory);

            return JsonGet(CallResult<bool>.Success(true));
        }


        public virtual ActionResult GetModelFromStyle(string styleString,
            int market,
            string marketplaceId)
        {
            LogI("GetModelFromStyle, StyleString=" + styleString + ", market=" + market + ", marketplaceId=" + marketplaceId);

            IList<MessageString> messages;

            var model = ItemEditViewModel.CreateFromStyleString(Db,
                AutoCreateListingService,
                StringHelper.TrimWhitespace(styleString),
                (MarketType)market,
                marketplaceId,
                out messages);

            var isSuccess = model != null;

            return JsonGet(new ValueMessageResult<ItemEditViewModel>(isSuccess, messages, model));
        }

        public virtual ActionResult GetUnusedBarcodeForStyleItem(long styleItemId,
            int market,
            string marketplaceId)
        {
            LogI("GetUnusedBarcodeForStyleItemId");

            var barcode = ItemEditViewModel.GetUnusedBarcodeForStyleItem(Db, styleItemId, (MarketType)market, marketplaceId);

            return JsonGet(ValueResult<string>.Success("", barcode));
        }

        public virtual ActionResult GetMissingSizes(AddVariationViewModel model)
        {
            LogI("GetMissingSizes, model=" + model);

            var items = new List<ItemVariationEditViewModel>();
            if (model.ExistSizes != null && model.ExistSizes.Any())
            {
                items = ItemEditViewModel.GetMissingSizes(Db,
                    LogService,
                    model.ExistSizes,
                    (MarketType) model.Market,
                    model.MarketplaceId);
            }

            return JsonGet(CallResult<IList<ItemVariationEditViewModel>>.Success(items));
        }

        public virtual ActionResult GetModelFromParentASIN(string asin)
        {
            LogI("GetModelFromParentASIN, ASIN=" + asin);

            IList<MessageString> messages;

            var model = ItemEditViewModel.CreateFromParentASIN(Db,
                AutoCreateListingService,
                StringHelper.TrimWhitespace(asin),
                (int)MarketType.Amazon,
                MarketplaceKeeper.AmazonComMarketplaceId,
                false,
                out messages);

            var isSuccess = model != null;

            return Json(new ValueMessageResult<ItemEditViewModel>(isSuccess, messages, model), JsonRequestBehavior.AllowGet);
        }
    }
}
