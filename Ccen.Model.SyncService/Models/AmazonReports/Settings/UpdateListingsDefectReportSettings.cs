using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.ReportParser.Processing.Listings;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public class UpdateListingsDefectReportSettings : BaseAmazonReportSettings
    {
        public override string TAG
        {
            get { return "UpdateListingsDefectReport"; }
        }

        public override AmazonReportType ReportType
        {
            get { return AmazonReportType._GET_MERCHANT_LISTINGS_DEFECT_DATA_; }
        }
        
        public override TimeSpan RequestInterval
        {
            get
            {
                return TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateListingsDefectIntervalMinutes));
            }
        }

        public override DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetListingsDefectSyncDate(market, marketplaceId);
        }

        public override void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when)
        {
            settings.SetListingsDefectSyncDate(DateTime.UtcNow, market, marketplaceId);
        }

        public override IReportParser GetParser()
        {
            return new ListingDefectParser();
        }
    }
}
