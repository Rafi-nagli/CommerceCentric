using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.ReportParser.Processing.Listings;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public class UpdateListingsReportSettings : BaseAmazonReportSettings
    {
        public override string TAG
        {
            get { return "UpdateListingsReport"; }
        }

        public override AmazonReportType ReportType
        {
            get { return AmazonReportType._GET_MERCHANT_LISTINGS_DATA_; }
        }
        
        public override TimeSpan RequestInterval
        {
            get
            {
                return TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateListingsIntervalMinutes));
            }
        }

        public override DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetListingsReadDate(market, marketplaceId);
        }

        public override void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when)
        {
            settings.SetListingsReadDate(DateTime.UtcNow, market, marketplaceId);
            settings.SetListingsManualSyncRequest(false, market, marketplaceId);
        }

        public override bool IsManualSyncRequested(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetListingsManualSyncRequest(market, marketplaceId) ?? false;
        }

        public override IReportParser GetParser()
        {
            return new ListingDataParser(true, true);
        }
    }
}
