using System;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Model.SyncService.Models.AmazonReports.Settings;

namespace Amazon.Model.SyncService.Models.AmazonReports
{
    public class AmazonReportFactory
    {
        private ITime _time;
        public AmazonReportFactory(ITime time)
        {
            _time = time;
        }

        public IAmazonReportSettings GetReportService(AmazonReportType type,
            string marketplaceId)
        {
            switch (type)
            {
                case AmazonReportType._GET_MERCHANT_LISTINGS_DATA_:
                    if (marketplaceId == MarketplaceKeeper.AmazonAuMarketplaceId
                        || marketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId)
                        return new UpdateListingsReportCombineModeSettings(_time);
                    if (marketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                        return new UpdateListingsReportCombineModeSettings(_time);
                    if (marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                        return new UpdateListingsReportCombineModeSettings(_time);
                    return new UpdateListingsReportSettings();
                case AmazonReportType._GET_FLAT_FILE_OPEN_LISTINGS_DATA_:
                    return new UpdateListingsOpenReportSettings();
                case AmazonReportType._GET_MERCHANT_LISTINGS_DATA_LITE_:
                    return new UpdateListingsLiteReportSettings();
                case AmazonReportType._GET_MERCHANT_LISTINGS_DEFECT_DATA_:
                    return new UpdateListingsDefectReportSettings();

                case AmazonReportType._GET_AFN_INVENTORY_DATA_:
                    return new UpdateFBAListingsReportSettings();
                case AmazonReportType._GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_:
                    return new UpdateFBAListingsFeeReportSettings();
                case AmazonReportType._GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_V2_:
                    return new SettlementReportSettings();
                case AmazonReportType._GET_XML_RETURNS_DATA_BY_RETURN_DATE_:
                    return new UpdateReturnsDataReportSettings();
                default:
                    throw new NotImplementedException("Report setting for type is not implemented, type=" + type.ToString());
            }
        }
    }
}
