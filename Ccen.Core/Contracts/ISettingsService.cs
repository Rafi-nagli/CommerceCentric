using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Core.Contracts
{
    public interface ISettingsService
    {
        IList<MarketplaceInfo> MarketSettings { get; }

        //QuickBooks
        string GetQBClientId();
        string GetQBClientSecret();
        string GetQBCompanyId();
        string GetQBRefreshToken();
        bool SetQBRefreshToken(string refreshToken);
        string GetQBAccessToken();
        bool SetQBAccessToken(string accessToken);
        string GetQBRedirectUrl();
        string GetQBEnvironment();

        //Cache
        DateTime? CacheUpdateDate { get; }
        bool? CacheUpdateInProgress { get; }

        //Intervals
        TimeSpan GetOrdersSyncInterval(MarketType market, string marketplaceId);
        TimeSpan GetOrdersSyncForMissedInterval(MarketType market, string marketplaceId);
        TimeSpan GetOrdersSyncForMissedShortInterval(MarketType market, string marketplaceId);
        
        TimeSpan CacheUpdateInteval { get; }

        TimeSpan OrdersFulfillmentInterval { get; }
        TimeSpan ListingsSyncInterval { get; }
        TimeSpan ListingsOpenSyncInterval { get; }
        TimeSpan ListingsLiteSyncInterval { get; }
        TimeSpan ListingsQuantitySyncInterval { get; }
        TimeSpan ListingsPriceSyncInterval { get; }

        
        void Init();

        DateTime? GetOrdersSyncDate(MarketType market, string marketplaceId);
        bool SetOrderSyncDate(DateTime? date, MarketType market, string marketplaceId);
        DateTime? GetOrdersFulfillmentDate(MarketType market, string marketplaceId);
        bool SetOrdersFulfillmentDate(DateTime? date, MarketType market, string marketplaceId);
        DateTime? GetOrdersAcknowledgementDate(MarketType market, string marketplaceId);
        bool SetOrdersAcknowledgementDate(DateTime? date, MarketType market, string marketplaceId);
        
        bool? GetOrdersSyncEnabled();
        bool SetOrdersSyncEnabled(bool? value);
        bool? GetOrdersSyncInProgress(MarketType market, string marketplaceId);
        bool SetOrdersSyncInProgress(bool? value, MarketType market, string marketplaceId);


        int? GetOrderCountOnMarket(MarketType market, string marketplaceId);
        bool SetOrderCountOnMarket(int? value, MarketType market, string marketplaceId);
        int? GetOrderCountInDB(MarketType market, string marketplaceId);
        bool SetOrderCountInDB(int? value, MarketType market, string marketplaceId);


        DateTime? GetReturnsDataReadDate(MarketType market, string marketplaceId);
        bool SetReturnsDataReadDate(DateTime? value, MarketType market, string marketplaceId);


        DateTime? GetListingsReadDate(MarketType market, string marketplaceId);
        bool SetListingsReadDate(DateTime? value, MarketType market, string marketplaceId);


        DateTime? GetListingsSendDate(MarketType market, string marketplaceId);
        bool SetListingsSendDate(DateTime? value, MarketType market, string marketplaceId);
        bool? GetListingsSyncInProgress(MarketType market, string marketplaceId);
        bool SetListingsSyncInProgress(bool? value, MarketType market, string marketplaceId);

        bool? GetListingsManualSyncRequest(MarketType market, string marketplaceId);
        bool SetListingsManualSyncRequest(bool? value, MarketType market, string marketplaceId);

        bool? GetListingsSyncPause(MarketType market, string marketplaceId);
        bool SetListingsSyncPause(bool? value, MarketType market, string marketplaceId);

        DateTime? GetListingsOpenSyncDate(MarketType market, string marketplaceId);
        bool SetListingsOpenSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetListingsLiteSyncDate(MarketType market, string marketplaceId);
        bool SetListingsLiteSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetListingsDefectSyncDate(MarketType market, string marketplaceId);
        bool SetListingsDefectSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetListingsTagSyncDate(MarketType market, string marketplaceId);
        bool SetListingsTagSyncDate(DateTime? value, MarketType market, string marketplaceId);



        DateTime? GetFBAListingsSyncDate(MarketType market, string marketplaceId);
        bool SetFBAListingsSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetFBAListingsFeeSyncDate(MarketType market, string marketplaceId);
        bool SetFBAListingsFeeSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetLastQBSyncDate();
        bool SetLastQBSyncDate(DateTime? value);


        DateTime? GetListingsQuantityToAmazonSyncDate(MarketType market, string marketplaceId);
        bool SetListingsQuantityToAmazonSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetListingsPriceSyncDate(MarketType market, string marketplaceId);
        bool SetListingsPriceSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetRankSyncDate(MarketType market, string marketplaceId);
        bool SetRankSyncDate(DateTime? value, MarketType market, string marketplaceId);

        DateTime? GetCacheUpdateDate();
        bool SetCacheUpdateDate(DateTime? value);

        //Cache
        bool? GetCacheUpdateInProgress();
        bool SetCacheUpdateInProgress(bool? value);
        
        bool SetUnansweredMessageCount(int value);
        int? GetUnansweredMessageCount();

        bool SetListingsQtyAlert(int value, MarketType market, string marketplaceId);
        int? GetListingsQtyAlert(MarketType market, string marketplaceId);

        bool SetListingsPriceAlert(int value, MarketType market, string marketplaceId);
        int? GetListingsPriceAlert(MarketType market, string marketplaceId);
        
        T GetValue<T>(IList<MarketSettingValue<T>> list, MarketType market, string marketplaceId);
        T GetValue<T>(string key);
    }
}
