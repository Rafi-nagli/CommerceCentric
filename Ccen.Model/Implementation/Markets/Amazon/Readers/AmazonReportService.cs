using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.Api;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.Core.Exceptions.Reports;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Markets.Amazon.Readers
{
    public class AmazonReportService : IAmazonReportService
    {
        private readonly ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        private AmazonApi _api;
        private readonly string _path;

        private AmazonReportInfo _reportInfo;
        private int _saveReportAttempts = 0;

        private readonly long _companyId;
        private ISyncInformer _syncInfo;
        private AmazonReportType _reportType;
        private IReportParser _parser;
        
        private ISystemActionService _actionService;

        public AmazonReportType ReportType
        {
            get { return _reportType; }
        }

        public string ReportFileName
        {
            get { return _reportInfo != null ? _reportInfo.ReportFileName : null; }
        }

        public string ReportId
        {
            get { return _reportInfo != null ? _reportInfo.ReportId : null; }
        }

        public string ReportRequestId
        {
            get { return _reportInfo != null? _reportInfo.ReportRequestId : null; }
        }

        public int SaveReportAttempts
        {
            get { return _saveReportAttempts; }
        }

        public AmazonReportService(AmazonReportType reportType, 
            long companyId, 
            AmazonApi api,
            ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ISyncInformer syncInfo, 
            IStyleManager styleManager,
            INotificationService notificationService,
            IStyleHistoryService styleHistoryService,    
            IItemHistoryService itemHistoryService,
            ISystemActionService actionService,
            IReportParser parser,
            string path = "")
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;

            _api = api;

            _reportInfo = new AmazonReportInfo();
            _reportInfo.ReportRequestId = String.Empty;

            _path = path;
            _companyId = companyId;
            _syncInfo = syncInfo;
            _reportType = reportType;
            _parser = parser;
            _actionService = actionService;

            var parseContext = new ParseContext()
            {
                Log = log,
                Time = time,
                DbFactory = dbFactory,
                ActionService = actionService,
                StyleManager = styleManager,
                NotificationService = notificationService,
                StyleHistoryService = styleHistoryService,
                ItemHistoryService = itemHistoryService,
                SyncInformer = syncInfo,
                CompanyId = companyId
            };
            _parser.Init(parseContext);

            _log.Info(string.Format("Path: {0}", path));
        }

        public IList<ReportDTO> GetReportList()
        {
            return _api.GetReportList(_reportType.ToString());
        }

        public void UseReportRequestId(string reportRequestId)
        {
            _reportInfo.ReportRequestId = reportRequestId;
        }

        public bool RequestReport()
        {
            _reportInfo.ReportRequestId = _api.RequestReport(_reportType.ToString());
            if (string.IsNullOrEmpty(_reportInfo.ReportRequestId))
            {
                _log.Info("No _reportRequestId");
                throw new ReportException("ReportRequestId is empty");
            }
            return true;
        }

        public bool SaveScheduledReport(string reportId)
        {
            _saveReportAttempts++;

            _log.Info("API SaveRequestedReport");
            
            _reportInfo.ReportFileName = _api.SaveReport(_log, reportId, _reportType, _path);
            _reportInfo.ReportId = reportId;

            if (string.IsNullOrEmpty(_reportInfo.ReportFileName))
            {
                _log.Info("Didn't save report");
                return false;
            }

            return true;
        }

        public ReportRequestDTO GetRequestStatus()
        {
            if (string.IsNullOrEmpty(_reportInfo.ReportRequestId) || string.IsNullOrEmpty(_path))
            {
                return null;
            }
            return _api.GetRequestStatus(_reportInfo.ReportRequestId);
        }

        public bool SaveRequestedReport()
        {
            _saveReportAttempts++;

            if (string.IsNullOrEmpty(_reportInfo.ReportRequestId) || string.IsNullOrEmpty(_path))
            {
                _log.Debug("SaveRequestedReport, _reportRequestId is emptry");
                return false;
            }
            var result = _api.SaveRequestedReport(_log, _reportInfo.ReportRequestId, _reportType, _path);
            
            if (result == null || string.IsNullOrEmpty(result.FileName))
            {
                _log.Debug("SaveRequestedReport, filename is empty");
                return false;
            }

            _reportInfo.ReportFileName = result.FileName;
            _reportInfo.ReportId = result.ReportId;
            return true;
        }

        public bool ProcessReport()
        {
            return ProcessReport(null);
        }

        public bool ProcessReport(IList<string> asinList)
        {
            if (string.IsNullOrEmpty(_path) || _reportInfo.ReportRequestId == null || string.IsNullOrEmpty(_reportInfo.ReportFileName))
            {
                _log.Info("No path/currentReport/Name");
                return false;
            }

            var directoryInfo = new DirectoryInfo(_path);
            var file = directoryInfo.GetFiles().FirstOrDefault(f => f.FullName == _reportInfo.ReportFileName);
            if (file != null)
            {
                return ProcessFile(file, asinList);
            }
            _log.Info("AmazonReportService No file found");
            return false;
        }

        protected bool ProcessFile(FileInfo file, IList<string> asinList)
        {
            try
            {
                _log.Info(string.Format("Open file:{0}", file.FullName));
                _parser.Open(file.FullName);

                _log.Info(string.Format("Begin parse items"));
                var items = _parser.GetReportItems(_api.Market, _api.MarketplaceId);
                _log.Info(string.Format("End parse items, count=" + items.Count));

                if (asinList != null)
                {
                    items = items
                        .Cast<ItemDTO>()
                        .Where(i => asinList.Contains(i.SKU))
                        .Cast<IReportItemDTO>()
                        .ToList();
                    _reportInfo.WasModified = true;
                }

                _log.Info("Process file");
                _parser.Process(_api, _time, _reportInfo, items);

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error when processing report", ex);
                return false;
            }
            finally
            {
                if (_parser != null)
                    _parser.Close();
            }
        }
    }
}
