using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.ReportParser.Processing.Listings;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public class UpdateFBAListingsFeeReportSettings : BaseAmazonReportSettings
    {
        public override string TAG
        {
            get { return "UpdateFBAListingsFeeReport"; }
        }

        public override AmazonReportType ReportType
        {
            get { return AmazonReportType._GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_; }
        }

        public override ReportRequestMode RequestMode
        {
            get
            {
                return ReportRequestMode.Sheduled;
            }
        }

        public override TimeSpan RequestInterval
        {
            get
            {
                return TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateFBAListingsFeeIntervalMinutes));
            }
        }

        public override DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetFBAListingsFeeSyncDate(market, marketplaceId);
        }

        public override void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when)
        {
            settings.SetFBAListingsFeeSyncDate(DateTime.UtcNow, market, marketplaceId);
        }

        public override IReportParser GetParser()
        {
            return new ListingFBAEstimatedFeeParser();
        }
    }
}
