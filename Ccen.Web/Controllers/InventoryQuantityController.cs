using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Core.Models.Calls;
using Amazon.DTO.Listings;
using Amazon.Model.General.Services;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;


namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class InventoryQuantityController : BaseController
    {
        public override string TAG
        {
            get { return "InventoryQuantityController."; }
        }

        private const string PopupContentView = "StyleQuantityViewModel";

        [Authorize(Roles = AccessManager.AllBaseRole)]
        public virtual ActionResult EditStyleQuantity(long styleId)
        {
            LogI("EditStyleQuantity, id=" + styleId);

            var model = new StyleQuantityViewModel(Db, styleId, Time.GetAppNowTime());
            ViewBag.PartialViewName = PopupContentView;
            return View("EditEmpty", model);
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual JsonResult GenerateOpenBox(long styleId)
        {
            LogI("GenerateOpenBox, styleId=" + styleId);

            var messages = new List<MessageString>();
            StyleQuantityViewModel.ValidateGenerateOpenBox(Db, styleId, out messages);
            if (!messages.Any())
            {
                var quantityManager = new QuantityManager(LogService, Time);
                StyleQuantityViewModel.GenerateOpenBox(Db,
                    Cache,
                    quantityManager,
                    styleId,
                    Time.GetAppNowTime(),
                    AccessManager.UserId);

                return JsonGet(new MessagesResult(true));
            }
            else
            {
                return JsonGet(new MessagesResult(false)
                {
                    Messages = messages
                });
            }
        }

        [Authorize(Roles = AccessManager.AllWriteRole)]
        public virtual JsonResult Redistribute(long styleId)
        {
            LogI("Redistribute, styleId=" + styleId);
            try
            {
                var quantityManager = new QuantityManager(LogService, Time);
                var service = new QuantityDistributionService(DbFactory, 
                        quantityManager, 
                        LogService, 
                        Time,
                        QuantityDistributionHelper.GetDistributionMarkets(),
                        DistributeMode.None);
                var listings = service.RedistributeForStyle(Db, styleId);
                
                return new JsonResult()
                {
                    Data = ValueResult<IList<ListingQuantityDTO>>.Success("Quantity was redistributed", listings),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
                LogE("Redistribute, styleId=" + styleId, ex);
                return new JsonResult()
                {
                    Data = MessageResult.Error(ex.Message),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        public virtual ActionResult Validate(StyleQuantityViewModel model)
        {
            LogI("Validate, model=" + model);
            var messages = model.Validate(Db, LogService, Time.GetAppNowTime());
            foreach (var message in messages)
            {
                LogI("Validate, message=" + message.Message);
            }
            return JsonGet(new ValueResult<IList<MessageString>>(true, "", messages));
        }


        [Authorize(Roles = AccessManager.AllBaseRole)]
        [HttpPost]
        public virtual ActionResult Submit(StyleQuantityViewModel model)
        {
            LogI("Submit, model=" + model);

            //Save
            if (ModelState.IsValid)
            {
                var quantityManager = new QuantityManager(LogService, Time);

                model.Apply(Db,
                    Cache,
                    quantityManager, 
                    StyleHistoryService,
                    SystemActions,
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
    }
}
