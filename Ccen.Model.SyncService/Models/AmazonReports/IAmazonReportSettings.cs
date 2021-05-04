using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;

namespace Amazon.Model.SyncService.Models.AmazonReports
{
    public interface IAmazonReportSettings
    {
        string TAG { get; }

        AmazonReportType ReportType { get; }
        ReportRequestMode RequestMode { get; }
        bool IsScheduled { get; }

        TimeSpan RequestInterval { get; }

        string ReportDirectory { get; }
        TimeSpan AwaitInterval { get; }
        int MaxRequestAttempt { get; }


        DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId);
        void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when);

        bool IsManualSyncRequested(ISettingsService settings, MarketType market, string marketplaceId);

        IReportParser GetParser();
    }
}
