using System;
using System.IO;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public abstract class BaseAmazonReportSettings : IAmazonReportSettings
    {
        public abstract string TAG { get; }

        public abstract AmazonReportType ReportType { get; }

        public virtual ReportRequestMode RequestMode
        {
            get { return ReportRequestMode.Requested; }
        }

        public bool IsScheduled
        {
            get { return false; }
        }

        public abstract TimeSpan RequestInterval { get; }

        public string ReportDirectory
        {
            get { return Path.Combine(AppSettings.ReportBaseDirectory, ReportType.ToString()); }
        }

        public TimeSpan AwaitInterval
        {
            get { return TimeSpan.FromMinutes(int.Parse(AppSettings.AwaitReadyReportIntervalMinutes)); }
        }

        public int MaxRequestAttempt
        {
            get { return int.Parse(AppSettings.MaxReportRequestAttempt); }
        }

        public abstract DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId);

        public abstract void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when);

        public virtual bool IsManualSyncRequested(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return false;
        }

        public abstract IReportParser GetParser();

        public override string ToString()
        {
            var nl = Environment.NewLine;
            return "Report settings:" + nl
                   + "TAG=" + TAG + nl
                   + "ReportType=" + ReportType + nl
                   + "IsScheduled=" + IsScheduled + nl
                   + "RequestInterval=" + RequestInterval + nl
                   + "ReportDirectory=" + ReportDirectory + nl
                   + "AwaitInterval=" + AwaitInterval + nl
                   + "MaxRequestAttempt=" + MaxRequestAttempt;
        }
    }
}
