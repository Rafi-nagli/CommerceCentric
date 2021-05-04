using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.ReportParser.Processing.Listings;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public class UpdateListingsLiteReportSettings : BaseAmazonReportSettings
    {
        public override string TAG
        {
            get { return "UpdateListingsLiteReport"; }
        }

        public override AmazonReportType ReportType
        {
            get { return AmazonReportType._GET_MERCHANT_LISTINGS_DATA_LITE_; }
        }
        
        public override TimeSpan RequestInterval
        {
            get
            {
                return TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateListingsLiteIntervalMinutes));
            }
        }

        public override DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetListingsLiteSyncDate(market, marketplaceId);
        }

        public override void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when)
        {
            settings.SetListingsLiteSyncDate(DateTime.UtcNow, market, marketplaceId);
        }

        public override IReportParser GetParser()
        {
            return new ListingLiteParser();
        }
    }
}
