using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.ReportParser.Processing.Listings;

namespace Amazon.Model.SyncService.Models.AmazonReports.Settings
{
    public class UpdateReturnsDataReportSettings : BaseAmazonReportSettings
    {
        public override string TAG
        {
            get { return "UpdateReturnsDataReport"; }
        }

        public override AmazonReportType ReportType
        {
            get { return AmazonReportType._GET_XML_RETURNS_DATA_BY_RETURN_DATE_; }
        }
        
        public override TimeSpan RequestInterval
        {
            get
            {
                return TimeSpan.FromDays(2);
            }
        }

        public override DateTime? GetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId)
        {
            return settings.GetReturnsDataReadDate(market, marketplaceId);
        }

        public override void SetLastSyncDate(ISettingsService settings, MarketType market, string marketplaceId, DateTime? when)
        {
            settings.SetReturnsDataReadDate(DateTime.UtcNow, market, marketplaceId);
        }

        public override IReportParser GetParser()
        {
            return new ReturnsDataFullParser();
        }
    }
}
