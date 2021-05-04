using System;
using System.Linq;
using System.Threading;
using Amazon.Api;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon.Readers;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports
{
    public class UpdateAmazonRequestedReportThread : ThreadBase
    {
        private readonly AmazonApi _api;
        private IAmazonReportSettings _reportSettings;

        private IAmazonReportService _reportService;
        private ISyncInformer _syncInfo;
        
        public UpdateAmazonRequestedReportThread(string logTag,
            AmazonApi api,
            long companyId,
            ISystemMessageService messageService,
            IAmazonReportSettings reportSettings,
            TimeSpan callbackInterval)
            : base(logTag, companyId, messageService, callbackInterval)
        {
            _api = api;
            _reportSettings = reportSettings;


            LogWrite(reportSettings.ToString());
        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var settings = new SettingsService(dbFactory);

            var styleHistoryService = new StyleHistoryService(log, time, dbFactory);
            var itemHistoryService = new ItemHistoryService(log, time, dbFactory);
            var styleManager = new StyleManager(log, time, styleHistoryService);
            var notificationService = new NotificationService(log, time, dbFactory);
            var actionService = new SystemActionService(log, time);

            var lastSyncDate = _reportSettings.GetLastSyncDate(settings, _api.Market, _api.MarketplaceId);
            var isManualSyncRequested = _reportSettings.IsManualSyncRequested(settings, _api.Market, _api.MarketplaceId);

            LogWrite("Market=" + _api.Market + ", marketplaceId=" + _api.MarketplaceId);
            LogWrite("Last sync date=" + lastSyncDate.ToString() + ", isManualSyncRequested=" + isManualSyncRequested);

            if (!lastSyncDate.HasValue ||
                (DateTime.UtcNow - lastSyncDate) > _reportSettings.RequestInterval ||
                    isManualSyncRequested == true)
            {
                if (_reportSettings.RequestMode == ReportRequestMode.Sheduled)
                {
                    //TODO: add 
                    //http://docs.developer.amazonservices.com/en_US/reports/Reports_Overview.html
                    //NOTE: Trying to get last existing report from Amazon

                    LogWrite("Check Sheduled");
                    _reportService = new AmazonReportService(_reportSettings.ReportType,
                            CompanyId,
                            _api,
                            log,
                            time,
                            dbFactory,
                            _syncInfo,
                            styleManager,
                            notificationService,
                            styleHistoryService,
                            itemHistoryService,
                            actionService,
                            _reportSettings.GetParser(),
                            _reportSettings.ReportDirectory);

                    var lastReports = _reportService.GetReportList();
                    if (lastReports.Any())
                    {
                        var reportId = lastReports[0].ReportId;

                        LogWrite("Getting last exist report, ReportId=" + reportId);
                        if (_reportService.SaveScheduledReport(reportId))
                        {
                            using (var db = dbFactory.GetRWDb())
                            {
                                LogWrite("Add sheduled report");
                                db.InventoryReports.AddSheduledReport(reportId, (int)_reportSettings.ReportType, _api.MarketplaceId, _reportService.ReportFileName);

                                ProcessReport(settings);

                                _reportSettings.SetLastSyncDate(settings, _api.Market, _api.MarketplaceId, DateTime.UtcNow);
                                db.InventoryReports.MarkProcessedByReportId(_reportService.ReportId, ReportProcessingResultType.Success);
                                _reportService = null;
                            }
                        }
                    }
                    else
                    {
                        LogWrite("Do not have any reports of type=" + _reportService.ReportType.ToString());
                        _reportSettings.SetLastSyncDate(settings, _api.Market, _api.MarketplaceId, DateTime.UtcNow);
                        _reportService = null;
                    }
                }

                if (_reportSettings.RequestMode == ReportRequestMode.Requested)
                {
                    if (_reportService == null)
                    {
                        LogWrite("Create reportService");
                        _syncInfo = new DbSyncInformer(dbFactory,
                            log, 
                            time,
                            SyncType.Listings, 
                            _api.MarketplaceId, 
                            _api.Market,
                            String.Empty);

                        _reportService = new AmazonReportService(_reportSettings.ReportType,
                            CompanyId,
                            _api,
                            log,
                            time,
                            dbFactory,
                            _syncInfo,
                            styleManager,
                            notificationService,
                            styleHistoryService,
                            itemHistoryService,
                            actionService,
                            _reportSettings.GetParser(),
                            _reportSettings.ReportDirectory);

                        using (var db = dbFactory.GetRWDb())
                        {
                            var lastUnsavedReport = db.InventoryReports.GetLastUnsaved((int)_reportService.ReportType, _api.MarketplaceId);
                            if (lastUnsavedReport == null ||
                                (time.GetUtcTime() - lastUnsavedReport.RequestDate) > _reportSettings.AwaitInterval)
                            {
                                LogWrite("Request report");
                                _syncInfo.SyncBegin(null);
                                _syncInfo.AddInfo("", "Request report from Amazon");

                                _reportService.RequestReport();
                                db.InventoryReports.AddRequestedReport(_reportService.ReportRequestId, (int)_reportService.ReportType, _api.MarketplaceId);
                            }
                            else
                            {
                                LogWrite("Use last report request, id=" + lastUnsavedReport.Id + ", marketplaceId=" + lastUnsavedReport.MarketplaceId);
                                _syncInfo.OpenLastSync();
                                _syncInfo.AddInfo("", "Use last Amazon report request");

                                _reportService.UseReportRequestId(lastUnsavedReport.RequestIdentifier);
                            }
                        }
                    }
                    else
                    {
                        using (var db = dbFactory.GetRWDb())
                        {
                            LogWrite("Save report");
                            if (_reportService.SaveRequestedReport())
                            {
                                LogWrite("Save success, reportId=" + _reportService.ReportId + ", reportName=" + _reportService.ReportFileName);
                                db.InventoryReports.Update(_reportService.ReportRequestId, _reportService.ReportFileName, _reportService.ReportId);

                                ProcessReport(settings);

                                db.InventoryReports.MarkProcessedByReportId(_reportService.ReportId, ReportProcessingResultType.Success);
                                _reportSettings.SetLastSyncDate(settings, _api.Market, _api.MarketplaceId, time.GetUtcTime());
                                _reportService = null;
                            }
                            else
                            {
                                LogWrite("Could not save report");

                                var requestStatus = _reportService.GetRequestStatus();
                                if (requestStatus == null)
                                {
                                    LogWrite("Request status empty, reportId=" + _reportService);

                                    db.InventoryReports.MarkProcessedByRequestId(_reportService.ReportRequestId, ReportProcessingResultType.Cancelled);
                                    _reportSettings.SetLastSyncDate(settings, _api.Market, _api.MarketplaceId, time.GetUtcTime());
                                    _reportService = null;
                                }
                                else
                                {
                                    if (requestStatus.Status == "_CANCELLED_")
                                    {
                                        LogWrite("Mark as cancelled (requestStatus = _CANCELLED_");

                                        db.InventoryReports.MarkProcessedByRequestId(_reportService.ReportRequestId, ReportProcessingResultType.Cancelled);
                                        _reportSettings.SetLastSyncDate(settings, _api.Market, _api.MarketplaceId, DateTime.UtcNow);
                                        _reportService = null;
                                    }
                                    else
                                    {
                                        if (_reportService.SaveReportAttempts > _reportSettings.MaxRequestAttempt)
                                        {
                                            LogWrite("Mark as failed (MaxAttampts), saveReportAttempts > maxRequestAttempt");

                                            db.InventoryReports.MarkProcessedByRequestId(_reportService.ReportRequestId, ReportProcessingResultType.MaxAttampts);
                                            _reportSettings.SetLastSyncDate(settings, _api.Market, _api.MarketplaceId, DateTime.UtcNow);
                                            _reportService = null;
                                        }
                                        else
                                        {
                                            Thread.Sleep(TimeSpan.FromMinutes(Int32.Parse(AppSettings.ReportRequestAttemptIntervalMinutes)));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ProcessReport(ISettingsService settings)
        {
            _syncInfo.AddSuccess("", "Report was successfully saved");
            LogWrite("Process report");
            _reportService.ProcessReport();
            
            _syncInfo.SyncEnd();
        }
    }
}
