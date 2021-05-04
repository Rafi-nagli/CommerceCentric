using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.ReportParser.Processing.Listings;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public class UpdateListingsOpenReportSettings : BaseAmazonReportSettings
    {
        public override string TAG
        {
            get { return "UpdateListingsOpenReport"; }
        }

        public override AmazonReportType ReportType
        {
            get { return AmazonReportType._GET_FLAT_FILE_OPEN_LISTINGS_DATA_; }
        }
        
        public override TimeSpan RequestInterval
        {
            get
            {
                return TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateListingsOpenIntervalMinutes));
            }
        }

        public override DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetListingsOpenSyncDate(market, marketplaceId);
        }

        public override void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when)
        {
            settings.SetListingsOpenSyncDate(DateTime.UtcNow, market, marketplaceId);
        }

        public override IReportParser GetParser()
        {
            return new ListingOpenParser();
        }
    }
}
