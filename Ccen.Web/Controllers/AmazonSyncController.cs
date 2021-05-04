using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Common.Services;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Pdf;
using Amazon.Model.Implementation.Rates;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Messages;
using Ccen.Web;
using Newtonsoft.Json;


namespace Amazon.Web.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class AmazonSyncController : BaseController
    {
        public override string TAG
        {
            get { return "AmazonSyncController."; }
        }

        private static Thread _debugPrintLabelThread = null;
        public virtual ActionResult PrintLabelsForBatch(long batchId)
        {
            LogI("PrintLabelsForBatch, batchId=" + batchId);

            try
            {
                var companyId = AccessManager.CompanyId;
                if (!companyId.HasValue)
                    throw new ArgumentNullException("CompanyId");

                var when = Time.GetAppNowTime();
                var by = AccessManager.UserId;

                var actionId = ActionService.AddAction(Db,
                    SystemActionType.PrintBatch,
                    batchId.ToString(),
                    new PrintBatchInput()
                    {
                        BatchId = batchId,
                        CompanyId = companyId.Value,
                        UserId = by
                    },
                    null,
                    by);

                LogI("PrintLabelsForBatch, actionId=" + actionId);
                
                if (AppSettings.IsDebug)
                {
                    var labelBatchService = new LabelBatchService(DbFactory,
                       ActionService,
                       LogService,
                       Time,
                       WeightService,
                       ServiceFactory,
                       EmailService,
                       BatchManager,
                       new PdfMakerByIText(LogService),
                       new AddressService(null, null, null), 
                       OrderHistoryService,
                       AppSettings.DefaultCustomType,
                       AppSettings.LabelDirectory,
                       AppSettings.ReserveDirectory,
                       AppSettings.TemplateDirectory,
                       new LabelBatchService.Config(),
                       AppSettings.IsSampleLabels);


                    _debugPrintLabelThread = new Thread(() =>
                    {
                        labelBatchService.ProcessPrintBatchActions(null);
                    });
                    _debugPrintLabelThread.Start();
                }


                return JsonGet(MessageResult.Success("", actionId.ToString()));
            }
            catch (Exception ex)
            {
                LogE("PrintLabelsForBatch error", ex);
                return JsonGet(MessageResult.Error("Print Error: " + ex.Message));
            }
        }
        

        public virtual ActionResult GetLabel(long orderId)
        {
            var log = LogService.Info("GetLabel");

            log.Info("begin, orderId=" + orderId);

            var companyId = AccessManager.CompanyId;
            if (!companyId.HasValue)
                throw new ArgumentNullException("CompanyId");

            var printBatchService = new LabelBatchService(DbFactory,
                    ActionService,
                    LogService,
                    Time,
                    WeightService,
                    ServiceFactory,
                    EmailService,
                    BatchManager,
                    PdfMaker,
                    new AddressService(null, null, null),
                    OrderHistoryService,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.ReserveDirectory,
                    AppSettings.TemplateDirectory,
                    new LabelBatchService.Config(),
                    AppSettings.IsSampleLabels);

            var result = printBatchService.PrintLabel(orderId, companyId.Value, AccessManager.UserId);
            
            if (result.PrintPackId.HasValue)
            {
                result.Url = Models.UrlHelper.GetPrintLabelPathById(result.PrintPackId);

                log.Info("end, result.url=" + result.Url);
                return Redirect(result.Url);
            }

            var printMessage = "-" + String.Join("<br/>- ", result.Messages.Select(m => m.Text).ToList());
            var message = printMessage + "<br/>" + result.FailedSummary;

            log.Info("end, message=" + message);
            return RedirectToAction("Message", "Home", new { Message = HttpUtility.UrlEncode(message.Replace("<br />", Environment.NewLine)) });
        }

        private class PrintResultViewModel
        {
            public string Url { get; set; }
            public bool NoPdf { get; set; }
            public string Message { get; set; }
            public string PickupReadyByTime { get; set; }
            public string PickupConfirmationNumber { get; set; }
        }

        public virtual ActionResult GetPrintResult(int printActionId)
        {
            var printAction = Db.SystemActions.GetAllAsDto().FirstOrDefault(a => a.Id == printActionId);

            if (printAction != null && printAction.Status == (int) SystemActionStatus.Done)
            {
                var printOutput = JsonConvert.DeserializeObject<PrintBatchOutput>(printAction.OutputData);

                var printMessage = String.Join("<br/>- ", printOutput.Messages.Select(m => m.Text).ToList());

                LogI("PrintLabelsForBatch, message=" + printMessage);

                var fileUrl = String.Empty;
                if (printOutput.PrintPackId.HasValue)
                {
                    fileUrl = Models.UrlHelper.GetPrintLabelPathById(printOutput.PrintPackId);
                }

                var model = new PrintResultViewModel()
                {
                    Url = fileUrl,
                    NoPdf = String.IsNullOrEmpty(fileUrl),
                    Message = printMessage,
                    PickupReadyByTime = (printOutput.PickupInfo != null && printOutput.PickupInfo.ReadyByTime != null)
                        ? printOutput.PickupInfo.ReadyByTime.Value.ToString("hh\\:mm")
                        : "",
                    PickupConfirmationNumber = printOutput.PickupInfo != null ? printOutput.PickupInfo.ConfirmationNumber : ""
                };

                return JsonGet(ValueResult<PrintResultViewModel>.Success("", model));
            }
            else
            {
                return JsonGet(ValueResult<SystemActionDTO>.Error("", null));
            }
        }
        
        public virtual ActionResult CheckPurchaseProgress(long? batchId, long? progressId)
        {
            if (!batchId.HasValue)
                return Json(MessageResult.Success("0"));

            SyncHistory progress = null;
            if (progressId.HasValue)
            {
                progress = Db.SyncHistory.GetFiltered(h => h.Id == progressId.Value).FirstOrDefault();
            }
            else
            {
                var additionalData = JsonConvert.SerializeObject(new {batchId = batchId});
                progress = Db.SyncHistory
                    .GetFiltered(h => h.AdditionalData == additionalData)
                    .OrderByDescending(h => h.StartDate)
                    .FirstOrDefault();
            }

            return Json(ValueResult<SyncHistory>.Success("", progress), JsonRequestBehavior.AllowGet);
        }
    }
}
