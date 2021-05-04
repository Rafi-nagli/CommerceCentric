using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.ReportParser.Processing.Listings;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public class SettlementReportSettings : BaseAmazonReportSettings
    {
        public override string TAG
        {
            get { return "SettlementReportSettings"; }
        }

        public override AmazonReportType ReportType
        {
            get { return AmazonReportType._GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_V2_; }
        }
        
        public override TimeSpan RequestInterval
        {
            get
            {
                return TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateFBAListingsIntervalMinutes));
            }
        }

        public override DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetFBAListingsSyncDate(market, marketplaceId);
        }

        public override void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when)
        {
            settings.SetFBAListingsSyncDate(DateTime.UtcNow, market, marketplaceId);
        }

        public override IReportParser GetParser()
        {
            return new ListingFBAInventoryParser();
        }
    }
}
