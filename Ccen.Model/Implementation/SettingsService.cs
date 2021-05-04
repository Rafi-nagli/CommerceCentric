using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DAL;

namespace Amazon.Web.Models
{
    public class SettingsService : ISettingsService
    {
        private IList<MarketplaceInfo> _marketSettings { get; set; }

        public IList<MarketplaceInfo> MarketSettings
        {
            get { return _marketSettings; } 
        }

        
        //Cache
        private DateTime? _cacheUpdateDate;
        private bool? _cacheUpdateInProgress;

        public DateTime? CacheUpdateDate
        {
            get { return _cacheUpdateDate; }
        }

        public bool? CacheUpdateInProgress
        {
            get { return _cacheUpdateInProgress; }
        }

        public const string KeyOrdersSyncDate = "OrdersSyncDate";
        public const string KeyOrdersFulfillmentDate = "OrdersFulfillmentDate";
        public const string KeyOrdersAcknowledgementDate = "KeyOrdersAcknowledgementDate";
        public const string KeyOrdersCancellationDate = "KeyOrdersCancellationDate";
        public const string KeyOrdersAdjustmentDate = "KeyOrdersAdjustmentDate";
        
        public const string KeyOrdersSyncEnabled = "OrdersSyncEnabled";
        public const string KeyOrdersSyncInProgress = "OrdersSyncInProgress";
        public const string KeyOrdersCountOnAmazon = "OrdersCountOnMarket";
        public const string KeyOrdersCountInDB = "OrdersCountInDB";

        
        public const string KeyReturnsDataReadDate = "ListingsReturnsDataDate";
        public const string KeyListingsReadDate = "ListingsReadDate";
        public const string KeyListingsSendDate = "ListingsSendDate";
        public const string KeyListingsOpenSyncDate = "ListingsOpenSyncDate";
        public const string KeyListingsLiteSyncDate = "ListingsLiteSyncDate";
        public const string KeyListingsDefectSyncDate = "ListingsDefectSyncDate";
        public const string KeyListingsUnpublishSyncDate = "ListingsUnpublishSyncDate";
        public const string KeyListingsManualSyncRequest = "ListingsManualSyncRequest";
        public const string KeyListingsSyncInProgress = "ListingsSyncInProgress";
        public const string KeyListingsSyncPause = "ListingsSyncPause";

        public const string KeyFBAListingsSyncDate = "FBAListingsSyncDate";
        public const string KeyFBAListingsFeeSyncDate = "FBAListingsFeeSyncDate";


        public const string KeyListingsQuantityToAmazonSyncDate = "ListingsQuantityToAmazonSyncDate";
        
        public const string KeyListingsPriceSyncDate = "ListingsPriceSyncDate";
        public const string KeyListingsPriceRuleSyncDate = "ListingsPriceRuleSyncDate";
        public const string KeyListingsRelationshipSyncDate = "ListingsRelationshipSyncDate";
        public const string KeyListingsImageSyncDate = "ListingsImageSyncDate";
        public const string KeyListingsTagsSyncDate = "ListingsTagsSyncDate";

        public const string KeyRankSyncDate = "RankSyncDate";
        public const string KeyBuyBoxSyncDate = "BuyBoxSyncDate";

        public const string KeyQuantityDistributeDate = "QuantityDistributeDate";

        public const string KeyCacheUpdateDate = "CacheUpdateDate";
        public const string KeyCacheUpdateInProgress = "CacheUpdateInProgress";

        public const string KeySetSendDhlInvoiceNotification = "SetSendDhlInvoiceNotification";

        public const string KeyImageUpdateDate = "ImageUpdateDate";

        public const string KeyListingsQtyAlert = "ListingsQtyAlert";
        public const string KeyListingsPriceAlert = "ListingsPriceAlert";

        public const string KeyUnansweredMessageCount = "UnansweredMessageCount";

        public const string KeySameDayLastCheck = "SameDayLastCheck";

        private IDbFactory _dbFactory;

        public SettingsService(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }


        //Intervals
        public TimeSpan GetOrdersSyncInterval(MarketType market, string marketplaceId)
        {
            switch (market)
            {
                case MarketType.Amazon:
                case MarketType.AmazonEU:
                case MarketType.AmazonAU:
                    return new TimeSpan(0, 10, 0);
                case MarketType.eBay:
                    return new TimeSpan(0, 30, 0);
                case MarketType.Magento:
                case MarketType.Shopify:
                case MarketType.WooCommerce:
                case MarketType.Groupon:
                case MarketType.Walmart:
                case MarketType.WalmartCA:
                    return new TimeSpan(0, 15, 0);
                case MarketType.OverStock:
                    return new TimeSpan(1, 0, 0);
                case MarketType.Jet:
                    return new TimeSpan(0, 5, 0);
            }
            return new TimeSpan(0, 15, 0);
        }

        public TimeSpan GetOrdersSyncForMissedInterval(MarketType market, string marketplaceId)
        {
            switch (market)
            {
                case MarketType.Amazon:
                case MarketType.AmazonEU:
                case MarketType.AmazonAU:
                case MarketType.eBay:
                case MarketType.Magento:
                case MarketType.Shopify:
                case MarketType.WooCommerce:
                case MarketType.Groupon:
                case MarketType.OverStock:
                case MarketType.Walmart:
                case MarketType.WalmartCA:
                case MarketType.Jet:
                    return new TimeSpan(3, 0, 0, 0);
            }
            return new TimeSpan(7, 0, 0, 0);
        }

        public TimeSpan GetOrdersSyncForMissedShortInterval(MarketType market, string marketplaceId)
        {
            return new TimeSpan(2, 0, 0);
        }

        public TimeSpan CacheUpdateInteval
        {
            get { return new TimeSpan(0, 30, 0); }
        }

        public TimeSpan OrdersFulfillmentInterval
        {
            get { return new TimeSpan(0, 5, 0); }
        }

        public TimeSpan ListingsSyncInterval
        {
            get { return new TimeSpan(5, 0, 0); }
        }

        public TimeSpan ListingsOpenSyncInterval
        {
            get { return new TimeSpan(1, 0, 0); }
        }
        
        public TimeSpan ListingsLiteSyncInterval
        {
            get { return new TimeSpan(1, 0, 0); }
        }

        public TimeSpan ListingsQuantitySyncInterval
        {
            get { return new TimeSpan(0, 5, 0); }
        }

        public TimeSpan ListingsPriceSyncInterval
        {
            get { return new TimeSpan(0, 5, 0); }
        }


        //Update settings methods
        public DateTime? GetOrdersSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersSyncDate, market, marketplaceId));
        }

        public bool SetOrderSyncDate(DateTime? date, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersSyncDate, market, marketplaceId), date);
        }
        public DateTime? GetOrdersFulfillmentDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersFulfillmentDate, market, marketplaceId));
        }

        public bool SetOrdersFulfillmentDate(DateTime? date, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersFulfillmentDate, market, marketplaceId), date);
        }

        public DateTime? GetOrdersAcknowledgementDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersAcknowledgementDate, market, marketplaceId));
        }

        public bool SetOrdersAcknowledgementDate(DateTime? date, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersAcknowledgementDate, market, marketplaceId), date);
        }


        public DateTime? GetOrdersCancellationDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersCancellationDate, market, marketplaceId));
        }

        public bool SetOrdersCancellationtDate(DateTime? date, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersCancellationDate, market, marketplaceId), date);
        }


        public DateTime? GetOrdersAdjustmentDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersAdjustmentDate, market, marketplaceId));
        }

        public bool SetOrdersAdjustmentDate(DateTime? date, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyOrdersAdjustmentDate, market, marketplaceId), date);
        }


        public bool? GetOrdersSyncEnabled()
        {
            return SettingsHelper.GetBoolSetting(_dbFactory, KeyOrdersSyncEnabled);
        }

        public bool SetOrdersSyncEnabled(bool? value)
        {
            return SettingsHelper.SetBoolSetting(_dbFactory, KeyOrdersSyncEnabled, value);
        }

        public bool? GetOrdersSyncInProgress(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetInProgressSetting(_dbFactory, MarketKey(KeyOrdersSyncInProgress, market, marketplaceId));
        }

        public bool SetOrdersSyncInProgress(bool? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetBoolSetting(_dbFactory, MarketKey(KeyOrdersSyncInProgress, market, marketplaceId), value);
        }

        public int? GetOrderCountOnMarket(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetIntSetting(_dbFactory, MarketKey(KeyOrdersCountOnAmazon, market, marketplaceId));
        }

        public bool SetOrderCountOnMarket(int? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetIntSetting(_dbFactory, MarketKey(KeyOrdersCountOnAmazon, market, marketplaceId), value);
        }

        public int? GetOrderCountInDB(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetIntSetting(_dbFactory, MarketKey(KeyOrdersCountInDB, market, marketplaceId));
        }

        public bool SetOrderCountInDB(int? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetIntSetting(_dbFactory, MarketKey(KeyOrdersCountInDB, market, marketplaceId), value);
        }


        private string MarketKey(string key, MarketType market, string marketplaceId)
        {
            return key + "_" + market + "_" + marketplaceId;
        }

        public DateTime? GetReturnsDataReadDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyReturnsDataReadDate, market, marketplaceId));
        }

        public bool SetReturnsDataReadDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyReturnsDataReadDate, market, marketplaceId), value);
        }


        public DateTime? GetListingsReadDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsReadDate, market, marketplaceId));
        }

        public bool SetListingsReadDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsReadDate, market, marketplaceId), value);
        }

        public DateTime? GetListingsSendDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsSendDate, market, marketplaceId));
        }

        public bool SetListingsSendDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsSendDate, market, marketplaceId), value);
        }

        public bool? GetListingsSyncInProgress(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetInProgressSetting(_dbFactory, MarketKey(KeyOrdersSyncInProgress, market, marketplaceId));
        }

        public bool SetListingsSyncInProgress(bool? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetBoolSetting(_dbFactory, MarketKey(KeyOrdersSyncInProgress, market, marketplaceId), value);
        }


        public bool? GetListingsManualSyncRequest(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetBoolSetting(_dbFactory, MarketKey(KeyListingsManualSyncRequest, market, marketplaceId));
        }

        public bool SetListingsManualSyncRequest(bool? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetBoolSetting(_dbFactory, MarketKey(KeyListingsManualSyncRequest, market, marketplaceId), value);
        }

        public bool? GetListingsSyncPause(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetBoolSetting(_dbFactory, MarketKey(KeyListingsSyncPause, market, marketplaceId));
        }

        public bool SetListingsSyncPause(bool? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetBoolSetting(_dbFactory, MarketKey(KeyListingsSyncPause, market, marketplaceId), value);
        }


        public DateTime? GetListingsOpenSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsOpenSyncDate, market, marketplaceId));
        }

        public bool SetListingsOpenSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsOpenSyncDate, market, marketplaceId), value);
        }


        public DateTime? GetListingsLiteSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsLiteSyncDate, market, marketplaceId));
        }

        public bool SetListingsLiteSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsLiteSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetListingsDefectSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsDefectSyncDate, market, marketplaceId));
        }

        public bool SetListingsDefectSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsDefectSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetListingsUnpublishSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsUnpublishSyncDate, market, marketplaceId));
        }

        public bool SetListingsUnpublishSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsUnpublishSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetListingsTagSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsTagsSyncDate, market, marketplaceId));
        }

        public bool SetListingsTagSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsTagsSyncDate, market, marketplaceId), value);
        }


        public DateTime? GetFBAListingsSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyFBAListingsSyncDate, market, marketplaceId));
        }

        public bool SetFBAListingsSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyFBAListingsSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetFBAListingsFeeSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyFBAListingsFeeSyncDate, market, marketplaceId));
        }

        public bool SetFBAListingsFeeSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyFBAListingsFeeSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetLastQBSyncDate()
        {
            throw new NotImplementedException();
        }

        public bool SetLastQBSyncDate(DateTime? value)
        {
            throw new NotImplementedException();
        }


        public DateTime? GetListingsQuantityToAmazonSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsQuantityToAmazonSyncDate, market, marketplaceId));
        }

        public bool SetListingsQuantityToAmazonSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsQuantityToAmazonSyncDate, market, marketplaceId), value);
        }


        public DateTime? GetListingsPriceSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsPriceSyncDate, market, marketplaceId));
        }

        public bool SetListingsPriceSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsPriceSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetListingsPriceRuleSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsPriceRuleSyncDate, market, marketplaceId));
        }

        public bool SetListingsPriceRuleSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsPriceRuleSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetListingsImageSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsImageSyncDate, market, marketplaceId));
        }

        public bool SetListingsImageSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsImageSyncDate, market, marketplaceId), value);
        }

        public DateTime? GetListingsRelationshipSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyListingsRelationshipSyncDate, market, marketplaceId));
        }

        public bool SetListingsRelationshipSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyListingsRelationshipSyncDate, market, marketplaceId), value);
        }


        public DateTime? GetRankSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyRankSyncDate, market, marketplaceId));
        }

        public bool SetRankSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyRankSyncDate, market, marketplaceId), value);
        }


        public DateTime? GetBuyBoxSyncDate(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, MarketKey(KeyBuyBoxSyncDate, market, marketplaceId));
        }

        public bool SetBuyBoxSyncDate(DateTime? value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, MarketKey(KeyBuyBoxSyncDate, market, marketplaceId), value);
        }


        public DateTime? GetQuantityDistributeDate()
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, KeyQuantityDistributeDate);
        }

        public bool SetQuantityDistributeDate(DateTime? value)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, KeyQuantityDistributeDate, value);
        }

        //Cache
        public DateTime? GetCacheUpdateDate()
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, KeyCacheUpdateDate);
        }

        public bool SetCacheUpdateDate(DateTime? value)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, KeyCacheUpdateDate, value);
        }


        public bool? GetCacheUpdateInProgress()
        {
            return SettingsHelper.GetInProgressSetting(_dbFactory, KeyCacheUpdateInProgress);
        }

        public bool SetCacheUpdateInProgress(bool? value)
        {
            return SettingsHelper.SetBoolSetting(_dbFactory, KeyCacheUpdateInProgress, value);
        }

        public DateTime? GetImageUpdateDate()
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, KeyImageUpdateDate);
        }

        public bool SetImageUpdateDate(DateTime? value)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, KeyImageUpdateDate, value);
        }

        public DateTime? GetSendDhlInvoiceNotification()
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, KeySetSendDhlInvoiceNotification);
        }

        public bool SetSendDhlInvoiceNotification(DateTime? value)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, KeySetSendDhlInvoiceNotification, value);
        }
        
        public bool SetUnansweredMessageCount(int value)
        {
            return SettingsHelper.SetIntSetting(_dbFactory, KeyUnansweredMessageCount, value);
        }

        public int? GetUnansweredMessageCount()
        {
            return SettingsHelper.GetIntSetting(_dbFactory, KeyUnansweredMessageCount);
        }

        public bool SetSameDayLastCheck(DateTime date)
        {
            return SettingsHelper.SetDateTimeSetting(_dbFactory, KeySameDayLastCheck, date);
        }

        public DateTime? GetSameDayLastCheck()
        {
            return SettingsHelper.GetDateTimeSetting(_dbFactory, KeySameDayLastCheck);
        }

        public bool SetListingsQtyAlert(int value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetIntSetting(_dbFactory, MarketKey(KeyListingsQtyAlert, market, marketplaceId), value);
        }

        public int? GetListingsQtyAlert(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetIntSetting(_dbFactory, MarketKey(KeyListingsQtyAlert, market, marketplaceId));
        }

        public bool SetListingsPriceAlert(int value, MarketType market, string marketplaceId)
        {
            return SettingsHelper.SetIntSetting(_dbFactory, MarketKey(KeyListingsPriceAlert, market, marketplaceId), value);
        }

        public int? GetListingsPriceAlert(MarketType market, string marketplaceId)
        {
            return SettingsHelper.GetIntSetting(_dbFactory, MarketKey(KeyListingsPriceAlert, market, marketplaceId));
        }

        public void Init()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var allSettings = db.Settings.GetAll().ToList();
                var marketplaces = new MarketplaceKeeper(_dbFactory, false);
                marketplaces.Init();

                _marketSettings = new List<MarketplaceInfo>();
                foreach (var marketplace in marketplaces.GetAll())
                {
                    var market = (MarketType) marketplace.Market;
                    var marketplaceId = marketplace.MarketplaceId;

                    var info = new MarketplaceInfo();
                    info.Market = market;
                    info.MarketplaceId = marketplaceId;

                    info.OrderSyncDate = SettingsHelper.GetDateTimeSetting(allSettings,
                        MarketKey(KeyOrdersSyncDate, market, marketplaceId));
                    info.OrdersFulfillmentDate = SettingsHelper.GetDateTimeSetting(allSettings,
                        MarketKey(KeyOrdersFulfillmentDate, market, marketplaceId));
                    info.OrderCountOnMarket = SettingsHelper.GetIntSetting(allSettings,
                        MarketKey(KeyOrdersCountOnAmazon, market, marketplaceId));
                    info.OrderCountInDb = SettingsHelper.GetIntSetting(allSettings,
                        MarketKey(KeyOrdersCountInDB, market, marketplaceId));

                    info.ListingsSyncDate = SettingsHelper.GetDateTimeSetting(allSettings,
                        MarketKey(KeyListingsSendDate, market, marketplaceId));
                    info.ListingsSyncInProgress = SettingsHelper.GetBoolSetting(allSettings,
                        MarketKey(KeyListingsSyncInProgress, market, marketplaceId));
                    info.ListingsManualSyncRequest = SettingsHelper.GetBoolSetting(allSettings,
                        MarketKey(KeyListingsManualSyncRequest, market, marketplaceId));

                    info.ListingsQuantityToAmazonSyncDate = SettingsHelper.GetDateTimeSetting(allSettings,
                        MarketKey(KeyListingsQuantityToAmazonSyncDate, market, marketplaceId));
                    info.ListingsPriceSyncDate = SettingsHelper.GetDateTimeSetting(allSettings,
                        MarketKey(KeyListingsPriceSyncDate, market, marketplaceId));

                    info.RankSyncDate = SettingsHelper.GetDateTimeSetting(allSettings,
                        MarketKey(KeyRankSyncDate, market, marketplaceId));
                    info.ListingQtyAlert = SettingsHelper.GetIntSetting(allSettings,
                        MarketKey(KeyListingsQtyAlert, market, marketplaceId));
                    info.ListingPriceAlert = SettingsHelper.GetIntSetting(allSettings,
                        MarketKey(KeyListingsPriceAlert, market, marketplaceId));

                    _marketSettings.Add(info);
                }

                //Cache
                _cacheUpdateDate = SettingsHelper.GetDateTimeSetting(allSettings, KeyCacheUpdateDate);
                _cacheUpdateInProgress = SettingsHelper.GetBoolSetting(allSettings, KeyCacheUpdateInProgress);
            }
        }
        
        public T GetValue<T>(IList<MarketSettingValue<T>> list, MarketType market, string marketplaceId)
        {
            var value = list.FirstOrDefault(m => m.Market == market && m.MarketplaceId == marketplaceId);
            if (value != null)
                return value.Value;
            return default(T);
        }

        public T GetValue<T>(string key)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)SettingsHelper.GetStrSetting(_dbFactory, key);

            throw new Exception("Unsupported type: " + typeof(T));
        }

        public string GetQBClientId()
        {
            throw new NotImplementedException();
        }

        public string GetQBClientSecret()
        {
            throw new NotImplementedException();
        }

        public string GetQBCompanyId()
        {
            throw new NotImplementedException();
        }

        public string GetQBRefreshToken()
        {
            throw new NotImplementedException();
        }

        public bool SetQBRefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public string GetQBAccessToken()
        {
            throw new NotImplementedException();
        }

        public bool SetQBAccessToken(string accessToken)
        {
            throw new NotImplementedException();
        }

        public string GetQBRedirectUrl()
        {
            throw new NotImplementedException();
        }

        public string GetQBEnvironment()
        {
            throw new NotImplementedException();
        }
    }
}