using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.SystemActions;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Pages;
using Amazon.Web.ViewModels.Results;
using Ccen.Web;
using DocumentFormat.OpenXml.Wordprocessing;


namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class ReturnOrderController : BaseController
    {
        public override string TAG
        {
            get { return "ReturnOrderController."; }
        }

        public virtual ActionResult Index(string orderId)
        {
            LogI("Index, orderId=" + orderId);

            var model = new ReturnOrderPageViewModel();
            model.ReasonCode = (int)MailLabelReasonCodes.ReturnLabelReasonCode;
            model.OrderId = orderId;

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult ValidateRefund(ReturnOrderViewModel model)
        {
            LogI("ValidateRefund");

            var result = model.ValidateRefundRequest(Db, LogService);
            return Json(new ValueResult<string>(!result.Any(), String.Join("<br/>", result.Select(r => r.Message)), null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult AcceptReturn(long id)
        {
            LogI("AcceptRefund, id=" + id);

            var result = ReturnOrderViewModel.AcceptReturn(Db,                 
                LogService,
                QuantityManager,
                ActionService, 
                id, 
                Time.GetAppNowTime(), 
                AccessManager.UserId);
            return JsonGet(result);
        }

        [HttpPost]
        public virtual ActionResult MarkRefundAsProcessed(long id)
        {
            LogI("MarkRefundAsProcessed, id=" + id);

            var result = ReturnOrderViewModel.MarkRefundAsProcessed(Db,
                LogService,
                Time,
                ActionService,
                id,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return JsonGet(result);
        }

        [HttpPost]
        public virtual ActionResult Generate(ReturnOrderViewModel model)
        {
            LogI("Index, model=" + model);

            if (ModelState.IsValid)
            {
                model.OrderID = model.OrderID.RemoveWhitespaces();

                var shipmentProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.ReserveDirectory,
                    AppSettings.TemplateDirectory);

                var labelService = new LabelService(shipmentProviders, LogService, Time, DbFactory, EmailService, PdfMaker, AddressService);
                var quntityManager = new QuantityManager(LogService, Time);
                var actionService = new SystemActionService(LogService, Time);

                if (model.ReasonCode == (int)MailLabelReasonCodes.ReturnLabelReasonCode)
                {
                    var returnResults = model.ReturnAction(Db, 
                        quntityManager,
                        actionService,
                        LogService,
                        Time,
                        Time.GetAppNowTime(),
                        AccessManager.UserId);

                    model.Messages.AddRange(returnResults);
                }

                if (model.ReasonCode == (int)MailLabelReasonCodes.ExchangeCode)
                {
                    var exchangeResults = model.ExchangeAction(Db,
                        LogService,
                        Time,
                        quntityManager,
                        labelService,
                        WeightService,
                        ShippingService,
                        AccessManager.Company,
                        Time.GetAppNowTime(),
                        AccessManager.UserId);

                    model.Messages.AddRange(exchangeResults);
                }

                if (model.ReasonCode == (int) MailLabelReasonCodes.RefundCode)
                {
                    var refundResults = model.RefundAction(Db,
                        quntityManager,
                        actionService,
                        LogService,
                        Time,
                        Time.GetAppNowTime(),
                        AccessManager.UserId);

                    model.Messages.AddRange(refundResults);
                }

                var affectedStyleIdList = model.Items.Where(i => i.ExchangeStyleId.HasValue).Select(i => (long)i.ExchangeStyleId.Value).Distinct().ToList();
                affectedStyleIdList.AddRange(model.Items.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).Distinct().ToList());
                Cache.RequestStyleIdUpdates(Db,
                    affectedStyleIdList,
                    UpdateCacheMode.IncludeChild,
                    AccessManager.UserId);
            }
            else
            {
                model.Messages.AddRange(ModelState.GetErrors().Select(m => MessageString.Error(m)));
            }

            LogI("Index, Generate results=");
            model.Messages.ForEach(m => LogI(m.Status + ": " + m.Message));

            return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetEmailsByOrderId(string orderId)
        {
            LogI("GetModelByOrderId, orderId=" + orderId);

            IList<EmailViewModel> emails = new List<EmailViewModel>();
            if (!String.IsNullOrEmpty(orderId))
                emails = ReturnOrderViewModel.GetEmailsByOrderId(Db, AddressService, orderId);

            return new JsonResult { Data = emails, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetModelByOrderId(string searchString)
        {
            LogI("GetModelByOrderId, searchString=" + searchString);

            var model = new ReturnOrderViewModel();
            if (!String.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.RemoveWhitespaces();
                model = ReturnOrderViewModel.GetByOrderId(Db, LogService, WeightService, searchString);
            }
            return new JsonResult { Data = model, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult GetShippingOptions(string countryTo, string countryFrom, int? weightLb, decimal? weightOz)
        {
            LogI("GetShippingOptions, countryTo=" + countryTo + ", countryFrom=" + countryFrom + ", weightLb" + weightLb + ", weightOz=" + weightOz);

            return Json(MailViewModel.GetShippingOptions(Db, 
                countryTo, 
                countryFrom, 
                weightLb ?? 0, 
                weightOz ?? 0,
                ShipmentProviderType.Stamps), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetStyleItemById(long styleItemId)
        {
            var styleItem = Db.StyleItems.GetAllAsDto().FirstOrDefault(si => si.StyleItemId == styleItemId);
            return Json(styleItem, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetStyleSizeInfo(long styleItemId)
        {
            var styleSizeInfo = StyleSizeInfoViewModel.GetByStyleItemId(Db, styleItemId);
            return Json(styleSizeInfo, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetExchangeInfo(string orderNumber)
        {
            var model = ReturnOrderViewModel.GetExchangeInfo(Db, WeightService, orderNumber);
            return JsonGet(ValueResult<IList<ReturnQuantityItemViewModel>>.Success("", model));
        }
    }
}
