using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Api;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Model.SyncService;
using Amazon.Model.SyncService.Models.AmazonReports;
using Amazon.Model.SyncService.Threads.Simple;
using Amazon.Model.SyncService.Threads.Simple.Demo;
using Amazon.Model.SyncService.Threads.Simple.Notifications;
using Amazon.Model.SyncService.Threads.Simple.PurchaseLabels;
using Amazon.Model.SyncService.Threads.Simple.UpdateAmazonInfo;
using Amazon.Model.SyncService.Threads.Simple.UpdateDSInfo;
using Amazon.Model.SyncService.Threads.Simple.UpdateEBayInfo;
using Amazon.Model.SyncService.Threads.Simple.UpdateGrouponInfo;
using Amazon.Model.SyncService.Threads.Simple.UpdateJetInfo;
using Amazon.Model.SyncService.Threads.Simple.UpdateMagentoInfo;
using Amazon.Model.SyncService.Threads.Simple.UpdateShopifyInfo;
using Amazon.Model.SyncService.Threads.Simple.Walmart;
using Ccen.Model.SyncService;
using Ccen.Model.SyncService.Threads.Simple.UpdateShopifyInfo;
using DropShipper.Api;
using eBay.Api;
using Groupon.Api;
using Jet.Api;
using Magento.Api.Wrapper;
using Shopify.Api;
using WooCommerce.Api;

namespace Amazon.InventoryUpdateService
{
    public class ThreadManager
    {
        private readonly List<IThread> threads = new List<IThread>();
        private ILogService _log;
        private ITime _time;

        public ThreadManager(ILogService log, ITime time)
        {
            _log = log;
            _time = time;
        }


        public void Start()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            var callbackInterval = TimeSpan.FromSeconds(Int32.Parse(AppSettings.ReportCallbackIntervalSeconds));
            var fastCallbackInterval = TimeSpan.FromSeconds(Int32.Parse(AppSettings.ReportCallbackIntervalSeconds) / 10);

            var updateListingPriceInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateListingsPriceOnAmazonIntervalMinutes));
            var updateListingQtyInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateListingsQtyOnAmazonIntervalMinutes));
            var updateListingDataInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateListingsIntervalMinutes));
            
            var processAmazonOrdersInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.ProcessAmazonOrdersIntervalMinutes));
            var processEBayOrdersInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.ProcessEBayOrdersIntervalMinutes));
            var processJetOrdersInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.ProcessJetOrdersIntervalMinutes));
            var updateFulfillmentInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateFulfillmentIntervalMinutes));

            var updateRatingInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateRatingIntervalMinutes));
            var updateQuantityDistributionInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateQuantityDistributionIntervalMinutes));
            var updateBuyBoxInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateBuyBoxIntervalMinutes));
            var updateSaleEndInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateSalesEndIntervalMinutes));

            var updateQuantityFixupInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateQuantityFixupMinutes));

            var updateOrderTrackingStatusInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateOrderTrackingStatusIntervalMinutes));
            var updateEmailsInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateEmailsIntervalMinutes));
            var updateAmazonImageInterval = TimeSpan.FromMinutes(Int32.Parse(AppSettings.UpdateAmazonImageInvervalMinutes));

            var dbFactory = new DbFactory();
            var messageService = new SystemMessageService(_log, _time, dbFactory);

            using (var db = dbFactory.GetRDb())
            {
                var company = db.Companies.GetFirstWithSettingsAsDto();
                var fromAddressList = new CompanyAddressService(company);
                var addressService = new AddressService(null, fromAddressList.GetReturnAddress(MarketIdentifier.Empty()), fromAddressList.GetPickupAddress(MarketIdentifier.Empty()));
                //Checking email service, sent test message
                var emailSmtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels);
                var emailService = new EmailService(_log, emailSmtpSettings, addressService);
                
                var marketplaces = new MarketplaceKeeper(dbFactory, false);
                marketplaces.Init();

                //TODO: if more than one user have access to the same Amazon/Stamps.com account appears duplicates
                
                var apiFactory = new MarketFactory(marketplaces.GetAll(), _time, _log, dbFactory, AppSettings.JavaPath);

                if (AppSettings.IsDemo)
                {
                    _log.Info("Start Demo threads");
                    var demoTheads = GetDemoThreads(company,
                        messageService,
                        emailService,
                        processAmazonOrdersInterval,
                        callbackInterval,
                        fastCallbackInterval,
                        updateQuantityDistributionInterval,
                        updateSaleEndInterval,
                        updateQuantityFixupInterval,
                        updateOrderTrackingStatusInterval,
                        updateEmailsInterval);
                    threads.AddRange(demoTheads);
                }
                else
                {
                    _log.Info("Start Production threads");
                    var amazonThreads = GetAmazonThreads(apiFactory,
                        company,
                        messageService,
                        processAmazonOrdersInterval,
                        updateFulfillmentInterval,
                        updateListingQtyInterval,
                        updateListingPriceInterval,
                        updateListingDataInterval,
                        updateBuyBoxInterval,
                        updateAmazonImageInterval,
                        callbackInterval);

                    var eBayMarkets = marketplaces.GetAll().Where(m => m.Market == (int)MarketType.eBay).ToList();
                    var eBayThreads = GetEBayThreads(apiFactory,
                        eBayMarkets,
                        company,
                        messageService,
                        processEBayOrdersInterval,
                        updateFulfillmentInterval,
                        TimeSpan.FromHours(2),
                        updateListingQtyInterval,
                        callbackInterval);

                    var dsThreads = GetDSThreads(apiFactory,
                        company,
                        messageService,
                        processEBayOrdersInterval,
                        updateFulfillmentInterval,
                        updateListingDataInterval,
                        updateListingQtyInterval,
                        callbackInterval);

                    //var magentoThreads = GetMagentoThreads(apiFactory,
                    //    company,
                    //    processEBayOrdersInterval,
                    //    updateFulfillmentInterval,
                    //    TimeSpan.FromMinutes(15),
                    //    updateListingQtyInterval,
                    //    callbackInterval);

                    var walmartThreads = GetWalmartThreads(apiFactory,
                        company,
                        messageService,
                        processAmazonOrdersInterval,
                        updateFulfillmentInterval,
                        updateListingQtyInterval,
                        callbackInterval);

                    var walmartCAThreads = GetWalmartCAThreads(apiFactory,
                        company,
                        messageService,
                        processAmazonOrdersInterval,
                        updateFulfillmentInterval,
                        updateListingQtyInterval,
                        callbackInterval);

                    var jetThreads = GetJetThreads(apiFactory,
                        company,
                        messageService,
                        processJetOrdersInterval,
                        updateFulfillmentInterval,
                        updateListingQtyInterval,
                        callbackInterval);

                    var grouponMarkets = marketplaces.GetAll().Where(m => m.Market == (int)MarketType.Groupon).ToList();
                    var grouponThreads = GetGrouponThreads(apiFactory,
                        grouponMarkets,
                        company,
                        messageService,
                        processAmazonOrdersInterval,
                        updateFulfillmentInterval,
                        updateListingQtyInterval,
                        callbackInterval);

                    if (company.ShortName != PortalEnum.PA.ToString())
                    {
                        var shopifyMarkets = marketplaces.GetAll().Where(m => m.Market == (int)MarketType.Shopify).ToList();
                        var shopifyThreads = GetShopifyThreads(apiFactory,
                            shopifyMarkets,
                            company,
                            messageService,
                            processAmazonOrdersInterval,
                            updateFulfillmentInterval,
                            updateListingQtyInterval,
                            updateListingPriceInterval,
                            updateListingDataInterval,
                            callbackInterval);
                        threads.AddRange(shopifyThreads);
                    }

                    var wooCommerceMarkets = marketplaces.GetAll().Where(m => m.Market == (int)MarketType.WooCommerce).ToList();
                    var wooCommerceThreads = GetWooCommerceThreads(apiFactory,
                        wooCommerceMarkets,
                        company,
                        messageService,
                        processAmazonOrdersInterval,
                        updateFulfillmentInterval,
                        updateListingQtyInterval,
                        updateListingPriceInterval,
                        updateListingDataInterval,
                        callbackInterval);
                    threads.AddRange(wooCommerceThreads);


                    //var overstockThreads = GetOverstockThreads(apiFactory,
                    //    company,
                    //    TimeSpan.FromHours(1),
                    //    TimeSpan.FromMinutes(30),
                    //    TimeSpan.FromMinutes(60),
                    //    callbackInterval);

                    var generalThreads = GetGeneralThreads(company,
                        messageService,
                        emailService,
                        processAmazonOrdersInterval,
                        callbackInterval,
                        fastCallbackInterval,
                        updateQuantityDistributionInterval,
                        updateSaleEndInterval,
                        updateQuantityFixupInterval,
                        updateOrderTrackingStatusInterval,
                        updateEmailsInterval);

                    threads.AddRange(amazonThreads);
                    threads.AddRange(eBayThreads);
                    //threads.AddRange(magentoThreads);
                    threads.AddRange(walmartThreads);
                    threads.AddRange(walmartCAThreads);
                    threads.AddRange(jetThreads);
                    threads.AddRange(dsThreads);
                    threads.AddRange(grouponThreads);
                    //threads.AddRange(overstockThreads);
                    threads.AddRange(generalThreads);
                }
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }
        }

        private IList<IThread> GetAmazonThreads(IMarketFactory apiFactory,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processAmazonOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan updateListingPriceInterval,
            TimeSpan updateListingDataInterval,
            TimeSpan updateBuyBoxInterval,
            TimeSpan updateAmazonImageInterval,
            TimeSpan callbackInterval)
        {
            _log.Info("callbackInterval=" + callbackInterval);
            _log.Info("updateFulfillmentInterval=" + updateFulfillmentInterval);

            var results = new List<IThread>();

            var reportFactory = new AmazonReportFactory(_time);

            //US
            var apiCom = apiFactory.GetApi(company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);
            var apiCa = apiFactory.GetApi(company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId);
            var apiMx = apiFactory.GetApi(company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonMxMarketplaceId);
            //Europe
            var apiUk = apiFactory.GetApi(company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonUkMarketplaceId);
            var apiDe = apiFactory.GetApi(company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonDeMarketplaceId);
            var apiEs = apiFactory.GetApi(company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonEsMarketplaceId);
            var apiFr = apiFactory.GetApi(company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonFrMarketplaceId);
            var apiIt = apiFactory.GetApi(company.Id, MarketType.AmazonEU, MarketplaceKeeper.AmazonItMarketplaceId);
            //AU
            var apiAu = apiFactory.GetApi(company.Id, MarketType.AmazonAU, MarketplaceKeeper.AmazonAuMarketplaceId);
            //India
            var apiIn = apiFactory.GetApi(company.Id, MarketType.AmazonIN, MarketplaceKeeper.AmazonInMarketplaceId);

            var allAmazonApiList = ArrayHelper.ToArray<IMarketApi>(
                apiCom,
                apiCa,
                apiMx,
                apiUk,
                apiDe,
                apiEs,
                apiFr,
                apiIt,
                apiAu
            );

            var mainAmazonApiList = ArrayHelper.ToArray<IMarketApi>
            (
                apiCom,
                apiCa,
                apiMx,
                apiUk,
                apiAu
            );

            //Get Orders
            var interval = processAmazonOrdersInterval;
            foreach (var marketApi in allAmazonApiList)
                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApi"
                    + MarketplaceKeeper.GetMarketplaceCodeName(marketApi.Market, marketApi.MarketplaceId),
                    marketApi,
                    company.Id,
                    messageService,
                    interval));

            //Fulfillment feed
            if (AppSettings.IsEnableSendOrderFulfillment)
            {
                interval = updateFulfillmentInterval;
                foreach (var marketApi in allAmazonApiList)
                    results.Add(new UpdateFulfillmentDataThread((AmazonApi)marketApi,
                        company.Id,
                        messageService,
                        callbackInterval,
                        interval));
            }

            //Acknowledgment feed
            if (AppSettings.IsEnableSendCancelations)
            {
                interval = updateFulfillmentInterval;
                foreach (var marketApi in allAmazonApiList)
                    results.Add(new UpdateCancellationDataThread((AmazonApi) marketApi,
                        company.Id,
                        messageService,
                        callbackInterval,
                        interval));
            }

            //Adjustment feed
            interval = updateFulfillmentInterval;
            foreach (var marketApi in allAmazonApiList)
                results.Add(new UpdateAdjustmentDataThread((AmazonApi)marketApi,
                    company.Id,
                    messageService,
                    callbackInterval,
                    interval));

            if (company.ShortName != PortalEnum.PA.ToString())
            {
                if (AppSettings.IsEnableSendItemUpdates)
                {
                    foreach (var marketApi in allAmazonApiList)
                    {
                        var marketCodeName = MarketplaceKeeper.GetMarketplaceCodeName(marketApi.Market, marketApi.MarketplaceId);

                        if (marketApi.MarketplaceId == MarketplaceKeeper.AmazonAuMarketplaceId
                            || marketApi.MarketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId
                            || marketApi.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                        {
                            results.Add(new UpdateListingsDataOnAmazonThread("UpdateListingsDataOnAmazon" + marketCodeName,
                                (AmazonApi)marketApi,
                                company.Id,
                                messageService,
                                callbackInterval,
                                TimeSpan.FromMinutes(15)));

                            results.Add(new UpdateListingsImageOnAmazonThread("UpdateListingsImageOnAmazon" + marketCodeName,
                                (AmazonApi)marketApi,
                                company.Id,
                                messageService,
                                callbackInterval,
                                updateAmazonImageInterval));
                        }
                    }

                    foreach (var marketApi in allAmazonApiList)
                    {
                        var marketCodeName = MarketplaceKeeper.GetMarketplaceCodeName(marketApi.Market, marketApi.MarketplaceId);

                        if (marketApi.MarketplaceId == MarketplaceKeeper.AmazonAuMarketplaceId
                            || marketApi.MarketplaceId == MarketplaceKeeper.AmazonUkMarketplaceId
                            || marketApi.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                            results.Add(new UpdateListingsRelationshipOnAmazonThread("UpdateListingsRelationshipOnAmazon" + marketCodeName,
                                (AmazonApi)marketApi,
                                company.Id,
                                messageService,
                                callbackInterval,
                                updateListingDataInterval));
                    }
                }
            }


            var index = 0;
            foreach (var api in mainAmazonApiList)
            {
                var marketCodeName = MarketplaceKeeper.GetMarketplaceCodeName(api.Market, api.MarketplaceId);

                if (AppSettings.IsEnableSendQtyUpdates
                    && api.MarketplaceId != MarketplaceKeeper.AmazonMxMarketplaceId) //Tempoarary exclude MX, while testing)
                {
                    results.Add(new UpdateListingsQtyOnAmazonThread("UpdateListingsQtyOnAmazon" + marketCodeName, 
                        (AmazonApi) api,
                        company.Id,
                        messageService,
                        callbackInterval, 
                        updateListingQtyInterval));
                }

                if (AppSettings.IsEnableSendPriceUpdates 
                    && api.MarketplaceId != MarketplaceKeeper.AmazonMxMarketplaceId) //Tempoarary exclude MX, while testing
                {
                    results.Add(new UpdateListingsPriceOnAmazonThread("UpdateListingsPriceOnAmazon" + marketCodeName,
                        (AmazonApi) api, 
                        company.Id,
                        messageService,
                        callbackInterval, 
                        updateListingPriceInterval));

                    results.Add(new UpdateListingsPriceRuleOnAmazonThread("UpdateListingsPriceRuleOnAmazon" + marketCodeName,
                        (AmazonApi)api,
                        company.Id,
                        messageService,
                        callbackInterval,
                        updateListingPriceInterval));
                }

                results.Add(new AmazonReadPriceThread(company.Id,
                    messageService,
                    (AmazonApi)api, 
                    new List<TimeSpan>()
                    {
                        new TimeSpan(2 + index * 3, 0, 0)
                    },
                    _time));

                if (api.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                {
                    results.Add(new UpdateAmazonRequestedReportThread("UpdateReturnsDataReport" + marketCodeName,
                        (AmazonApi)api,
                        company.Id,
                        messageService,
                        reportFactory.GetReportService(AmazonReportType._GET_XML_RETURNS_DATA_BY_RETURN_DATE_, api.MarketplaceId),
                        callbackInterval));

                    results.Add(new UpdateListingsUnpublishOnAmazonThread("UpdateReturnsDataReport" + marketCodeName,
                        (AmazonApi)api,
                        company.Id,
                        messageService,
                        callbackInterval,
                        updateListingDataInterval));
                }

                results.Add(new UpdateAmazonRequestedReportThread("UpdateListingsReport" + marketCodeName,
                    (AmazonApi)api,
                    company.Id,
                    messageService,
                    reportFactory.GetReportService(AmazonReportType._GET_MERCHANT_LISTINGS_DATA_, api.MarketplaceId),
                    callbackInterval));

                results.Add(new UpdateAmazonRequestedReportThread("UpdateListingsDefectReport" + marketCodeName,
                    (AmazonApi)api,
                    company.Id,
                    messageService,
                    reportFactory.GetReportService(AmazonReportType._GET_MERCHANT_LISTINGS_DEFECT_DATA_, api.MarketplaceId),
                    callbackInterval));

                index++;
            }

            //Buybox / Ranking
            if (mainAmazonApiList.Any())
                results.Add(new UpdateAmazonBuyBoxStatusThread(mainAmazonApiList.Cast<AmazonApi>().ToList(), company.Id, messageService, callbackInterval, updateBuyBoxInterval));
            

            //TODO: need to review, later version of reports
            //results.Add(new UpdateAmazonRequestedReportThread(apiCOM,
            //    user.Id,
            //    reportFactory.GetReportService(AmazonReportType._GET_MERCHANT_LISTINGS_DATA_LITE_),
            //    callbackInterval));
            //results.Add(new UpdateAmazonRequestedReportThread(apiCOM,
            //    user.Id,
            //    reportFactory.GetReportService(AmazonReportType._GET_FLAT_FILE_OPEN_LISTINGS_DATA_),
            //    callbackInterval));


            //var reportSettings = reportFactory.GetReportService(AmazonReportType._GET_AFN_INVENTORY_DATA_);
            //results.Add(new UpdateAmazonRequestedReportThread(reportSettings.TAG,
            //    (AmazonApi)apiCom,
            //    company.Id,
            //    reportSettings,
            //    callbackInterval));

            //reportSettings = reportFactory.GetReportService(AmazonReportType._GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_);
            //results.Add(new UpdateAmazonRequestedReportThread(reportSettings.TAG,
            //    (AmazonApi)apiCom,
            //    company.Id,
            //    reportSettings,
            //    callbackInterval));

            if (mainAmazonApiList.Any())
                results.Add(new UpdateImageThread(company.Id,
                    messageService,
                    new List<TimeSpan>()
                    {
                        new TimeSpan(1, 0, 0)
                    }, 
                    _time));

            return results;
        }

        private IList<IThread> GetShopifyThreads(IMarketFactory apiFactory,
            IList<MarketplaceDTO> marketplaces,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processEBayOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingDataInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan updateListingPriceInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            foreach (var market in marketplaces)
            {
                var apiShopify = apiFactory.GetApi(company.Id, (MarketType)market.Market, market.MarketplaceId);

                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiShopify",
                    apiShopify,
                    company.Id,
                    messageService,
                    TimeSpan.FromMinutes(5)));

                results.Add(new UpdateShopifyOrderDataThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, updateFulfillmentInterval));

                results.Add(new UpdateShopifyCancellationDataThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, updateFulfillmentInterval));

                results.Add(new UpdateShopifyOrderRefundDataThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, updateFulfillmentInterval));

                results.Add(new UpdateShopifyPaymentStatusThread((ShopifyApi)apiShopify, company.Id, messageService, TimeSpan.FromHours(6)));


                if (AppSettings.IsEnableSendItemUpdates)
                {
                    results.Add(new ReadShopifyListingInfoThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, TimeSpan.FromHours(4)));

                    results.Add(new ImportShopifyListingDataThread((ShopifyApi)apiShopify,
                        company.Id,
                        messageService,
                        TimeSpan.FromMinutes(60),
                        updateListingDataInterval));
                }

                //results.Add(new UpdateShopifyListingDataThread((ShopifyApi)apiShopify,
                //    company.Id,
                //    messageService,
                //    callbackInterval,
                //    updateListingDataInterval));

                if (AppSettings.IsEnableSendQtyUpdates)
                {
                    results.Add(new UpdateShopifyListingQtyThread((ShopifyApi)apiShopify,
                        company.Id,
                        messageService,
                        callbackInterval,
                        updateListingQtyInterval));
                }
                if (AppSettings.IsEnableSendPriceUpdates)
                {
                    results.Add(new UpdateShopifyListingPriceThread((ShopifyApi)apiShopify,
                        company.Id,
                        messageService,
                        callbackInterval,
                        updateListingPriceInterval));
                }

                //results.Add(new UpdateShopifyProductInfoThread((ShopifyApi)apiShopify,
                //    company.Id,
                //    messageService,
                //    TimeSpan.FromMinutes(3)));
            }

            return results;
        }

        private IList<IThread> GetWooCommerceThreads(IMarketFactory apiFactory,
            IList<MarketplaceDTO> marketplaces,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processEBayOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingDataInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan updateListingPriceInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            foreach (var market in marketplaces)
            {
                var api = apiFactory.GetApi(company.Id, (MarketType)market.Market, market.MarketplaceId);

                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiWooCommerce",
                    api,
                    company.Id,
                    messageService,
                    TimeSpan.FromMinutes(5)));

                //results.Add(new UpdateShopifyOrderDataThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, updateFulfillmentInterval));

                //results.Add(new UpdateShopifyCancellationDataThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, updateFulfillmentInterval));

                //results.Add(new UpdateShopifyOrderRefundDataThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, updateFulfillmentInterval));

                //results.Add(new UpdateShopifyPaymentStatusThread((ShopifyApi)apiShopify, company.Id, messageService, TimeSpan.FromHours(6)));


                if (AppSettings.IsEnableSendItemUpdates)
                {
                    //results.Add(new ReadShopifyListingInfoThread((ShopifyApi)apiShopify, company.Id, messageService, callbackInterval, TimeSpan.FromHours(4)));

                    results.Add(new ImportWooCommerceListingDataThread((WooCommerceApi)api,
                        company.Id,
                        messageService,
                        TimeSpan.FromMinutes(60),
                        updateListingDataInterval));
                }

                //results.Add(new UpdateShopifyListingDataThread((ShopifyApi)apiShopify,
                //    company.Id,
                //    messageService,
                //    callbackInterval,
                //    updateListingDataInterval));

                //if (AppSettings.IsEnableSendQtyUpdates)
                //{
                //    results.Add(new UpdateShopifyListingQtyThread((ShopifyApi)apiShopify,
                //        company.Id,
                //        messageService,
                //        callbackInterval,
                //        updateListingQtyInterval));
                //}
                //if (AppSettings.IsEnableSendPriceUpdates)
                //{
                //    results.Add(new UpdateShopifyListingPriceThread((ShopifyApi)apiShopify,
                //        company.Id,
                //        messageService,
                //        callbackInterval,
                //        updateListingPriceInterval));
                //}

                //results.Add(new UpdateShopifyProductInfoThread((ShopifyApi)apiShopify,
                //    company.Id,
                //    messageService,
                //    TimeSpan.FromMinutes(3)));
            }

            return results;
        }

        private IList<IThread> GetEBayThreads(IMarketFactory apiFactory,
            IList<MarketplaceDTO> marketplaces,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processEBayOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingDataInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            foreach (var marketplace in marketplaces)
            {
                var apiEbay = apiFactory.GetApi(company.Id, (MarketType)marketplace.Market, marketplace.MarketplaceId);
                
                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiEBay" + marketplace.MarketplaceId, apiEbay, company.Id, messageService, processEBayOrdersInterval));
                if (AppSettings.IsEnableSendOrderFulfillment)
                {
                    results.Add(new UpdateEBayOrderDataThread((eBayApi)apiEbay, company.Id, messageService, TimeSpan.FromMinutes(30), updateFulfillmentInterval));
                }

                results.Add(new EBayRepublishThread((eBayApi)apiEbay, company.Id, messageService, new List<TimeSpan>() { new TimeSpan(0, 4, 0) }, _time));
                results.Add(new UpdateEBayListingDataThread((eBayApi)apiEbay, company.Id, messageService, TimeSpan.FromMinutes(30), updateListingDataInterval));
                results.Add(new ReadListingEBayInfoFromMarketThread((eBayApi)apiEbay, company.Id, messageService, TimeSpan.FromMinutes(30), updateListingDataInterval));

                if (AppSettings.IsEnableSendQtyUpdates)
                {
                    results.Add(new UpdateEBayListingQtyThread((eBayApi)apiEbay, company.Id, messageService, TimeSpan.FromMinutes(30), updateListingQtyInterval));
                }
                if (AppSettings.IsEnableSendPriceUpdates)
                {
                    results.Add(new UpdateEBayListingPriceThread((eBayApi)apiEbay, company.Id, messageService, TimeSpan.FromMinutes(30), updateListingQtyInterval));
                }
            }

            return results;
        }

        private IList<IThread> GetDSThreads(IMarketFactory apiFactory,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processShopifyOrdersInteval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingDataInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            var apiMbg = apiFactory.GetApi(company.Id, MarketType.DropShipper, MarketplaceKeeper.DsToMBG);
            if (apiMbg != null)
            {
                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiDropShipperMBG", apiMbg, company.Id, messageService, processShopifyOrdersInteval));
                results.Add(new UpdateDSOrderDataThread((DropShipperApi)apiMbg, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
                if (AppSettings.IsEnableSendQtyUpdates)
                {
                    results.Add(new UpdateDSListingQtyThread((DropShipperApi)apiMbg, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
            }

            return results;
        }

        private IList<IThread> GetMagentoThreads(IMarketFactory apiFactory,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processMagentoOrdersInteval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingDataInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            var apiMagento = apiFactory.GetApi(company.Id, MarketType.Magento, "");

            if (apiMagento != null)
            {
                //results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiMagento", apiMagento, user.Id, processMagentoOrdersInteval));
                if (AppSettings.IsEnableSendOrderFulfillment)
                {
                    results.Add(new UpdateMagentoOrderDataThread((Magento20MarketApi)apiMagento, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
                }

                results.Add(new UpdateMagentoListingDataThread((Magento20MarketApi)apiMagento, company.Id, messageService, callbackInterval, updateListingDataInterval));
                if (AppSettings.IsEnableSendQtyUpdates)
                {
                    results.Add(new UpdateMagentoListingQtyThread((Magento20MarketApi)apiMagento, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
            }

            return results;
        }

        private IList<IThread> GetWalmartThreads(IMarketFactory apiFactory,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processWmartOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            var apiWalmart = apiFactory.GetApi(company.Id, MarketType.Walmart, "");

            if (apiWalmart != null)
            {
                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiWmart", apiWalmart, company.Id, messageService, processWmartOrdersInterval));
                results.Add(new UpdateWalmartOrderAcknowledgmentDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, processWmartOrdersInterval));
                results.Add(new UpdateWalmartCancellationDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, processWmartOrdersInterval));
                results.Add(new UpdateWalmartOrderAdjustmentDataThread((IRefundApi)apiWalmart, company.Id, messageService, callbackInterval, processWmartOrdersInterval));

                if (AppSettings.IsEnableSendOrderFulfillment)
                {
                    results.Add(new UpdateWalmartOrderDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
                }

                results.Add(new ReadReturnWalmartInfoFromMarketThread((IWalmartApi)apiWalmart, company.Id, messageService, TimeSpan.FromHours(1), TimeSpan.FromHours(8)));
                results.Add(new ReadListingWalmartInfoFromMarketThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, TimeSpan.FromHours(2)));
                results.Add(new CheckWalmartListingStatusThread(company.Id, messageService, new List<TimeSpan>()
            {
                new TimeSpan(5, 0, 0)
            }, _time));

                results.Add(new UpdateWalmartListingDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, TimeSpan.FromMinutes(15)));
                if (AppSettings.IsEnableSendQtyUpdates)
                {
                    results.Add(new UpdateWalmartListingQtyThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
                if (AppSettings.IsEnableSendPriceUpdates)
                {
                    results.Add(new UpdateWalmartListingPriceThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
            }

            return results;
        }

        private IList<IThread> GetWalmartCAThreads(IMarketFactory apiFactory,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processWmartOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            var apiWalmart = apiFactory.GetApi(company.Id, MarketType.WalmartCA, "");

            if (apiWalmart != null)
            {
                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiWmartCA", apiWalmart, company.Id, messageService, processWmartOrdersInterval));
                results.Add(new UpdateWalmartOrderAcknowledgmentDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, processWmartOrdersInterval));
                results.Add(new UpdateWalmartCancellationDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, processWmartOrdersInterval));
                results.Add(new UpdateWalmartOrderAdjustmentDataThread((IRefundApi)apiWalmart, company.Id, messageService, callbackInterval, processWmartOrdersInterval));

                if (AppSettings.IsEnableSendOrderFulfillment)
                {
                    results.Add(new UpdateWalmartOrderDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
                }

                //results.Add(new ReadListingWalmartInfoFromMarketThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, TimeSpan.FromHours(4)));

                //TEMP: call it manually
                results.Add(new UpdateWalmartListingDataThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, TimeSpan.FromHours(24)));

                if (AppSettings.IsEnableSendQtyUpdates)
                {
                    results.Add(new UpdateWalmartListingQtyThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
                if (AppSettings.IsEnableSendPriceUpdates)
                {
                    results.Add(new UpdateWalmartListingPriceThread((IWalmartApi)apiWalmart, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
            }

            return results;
        }

        private IList<IThread> GetJetThreads(IMarketFactory apiFactory,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processJetOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();

            var apiJet = apiFactory.GetApi(company.Id, MarketType.Jet, "");

            if (apiJet != null)
            {
                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiJet", apiJet, company.Id, messageService, processJetOrdersInterval));
                results.Add(new UpdateJetOrderAcknowledgmentDataThread((JetApi)apiJet, company.Id, messageService, callbackInterval, processJetOrdersInterval));

                //results.Add(new UpdateWalmartCancellationDataThread((WalmartApi)apiJet, company.Id, callbackInterval, processWmartOrdersInterval));
                results.Add(new UpdateJetOrderReturnDataThread((JetApi)apiJet, company.Id, messageService, callbackInterval, processJetOrdersInterval));

                if (AppSettings.IsEnableSendOrderFulfillment)
                {
                    results.Add(new UpdateJetOrderDataThread((JetApi)apiJet, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
                }

                //results.Add(new ReadListingJetInfoFromMarketThread((JetApi)apiJet, company.Id, messageService, callbackInterval, TimeSpan.FromHours(4)));

                results.Add(new UpdateJetListingDataThread((JetApi)apiJet, company.Id, messageService, callbackInterval, TimeSpan.FromMinutes(15)));
                if (AppSettings.IsEnableSendQtyUpdates)
                {
                    results.Add(new UpdateJetListingQtyThread((JetApi)apiJet, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
                if (AppSettings.IsEnableSendPriceUpdates)
                {
                    results.Add(new UpdateJetListingPriceThread((JetApi)apiJet, company.Id, messageService, callbackInterval, updateListingQtyInterval));
                }
            }

            return results;
        }

        private IList<IThread> GetGrouponThreads(IMarketFactory apiFactory,
            IList<MarketplaceDTO> marketplaces,
            CompanyDTO company,
            ISystemMessageService messageService,
            TimeSpan processGrouponOrdersInterval,
            TimeSpan updateFulfillmentInterval,
            TimeSpan updateListingQtyInterval,
            TimeSpan callbackInterval)
        {
            var results = new List<IThread>();
                        
            foreach (var marketplace in marketplaces)
            {
                var api = apiFactory.GetApi(company.Id, (MarketType)marketplace.Market, marketplace.MarketplaceId);

                results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiGroupon", api, company.Id, messageService, processGrouponOrdersInterval));
                results.Add(new UpdateGrouponOrderDataThread((GrouponApi)api, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
                results.Add(new UpdateGrouponOrderAcknowledgementThread((GrouponApi)api, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
                results.Add(new UpdateGrouponOrderCancellationThread((GrouponApi)api, company.Id, messageService, callbackInterval, updateFulfillmentInterval));
            }

            return results;
        }

        //private IList<IThread> GetOverstockThreads(IMarketFactory apiFactory,
        //        CompanyDTO company,
        //        TimeSpan processOverstockOrdersInterval,
        //        TimeSpan updateFulfillmentInterval,
        //        TimeSpan updateListingQtyInterval,
        //        TimeSpan callbackInterval)
        //{
        //    var results = new List<IThread>();

        //    var api = apiFactory.GetApi(company.Id, MarketType.OverStock, "");

        //    results.Add(new UpdateOrdersFromOrderApiThread("UpdateOrdersFromOrderApiOverstock", api, company.Id, processOverstockOrdersInterval));
        //    results.Add(new UpdateOverstockOrderDataThread((SupplieroasisApi)api, company.Id, callbackInterval, updateFulfillmentInterval));
        //    if (AppSettings.IsEnableSendQtyUpdates)
        //    {
        //        //results.Add(new UpdateOverstockListingQtyThread((SupplieroasisApi)api, company.Id, callbackInterval, updateListingQtyInterval));
        //    }
        //    results.Add(new ReadListingOverstockInfoFromMarketThread((SupplieroasisApi)api, company.Id, callbackInterval, TimeSpan.FromHours(4)));


        //    return results;
        //}

        private IList<IThread> GetGeneralThreads(CompanyDTO company,
            ISystemMessageService messageService,
            IEmailService emailService,
            TimeSpan processAmazonOrdersInterval,
            TimeSpan callbackInterval,
            TimeSpan fastCallbackInterval,
            TimeSpan updateQuantityDistributionInterval,
            TimeSpan updateSaleEndInterval,
            TimeSpan updateQuantityFixupInterval,
            TimeSpan updateOrderTrackingStatusInterval,
            TimeSpan updateEmailsInterval)
        {
            var results = new List<IThread>();

            results.Add(new PrintLabelsThread(company.Id, messageService, fastCallbackInterval));

            if (company.ShortName == "PA")
            {
                results.Add(new PurchaseLabelsForOverdueThread(company.Id, messageService, new List<TimeSpan>() { AppSettings.OverdueAutoPurchaseTime }, _time));
                results.Add(new PurchaseLabelsForSameDayThread(company.Id, messageService, new List<TimeSpan>() { AppSettings.SameDayAutoPurchaseTime }, _time));

                results.Add(new CheckSameDay(company.Id, messageService, new List<TimeSpan>()
                {
                    AppSettings.SameDayCheckTime
                },
                _time));

                results.Add(new CheckOverdueStatusThread(company.Id, messageService, new List<TimeSpan>()
                {
                    new TimeSpan(16, 20, 0),
                    new TimeSpan(22, 0, 0),
                    new TimeSpan(23, 0, 0),
                    new TimeSpan(0, 0, 0),
                    new TimeSpan(1, 0, 0),
                },
                _time));

                //results.Add(new UpdateRecountingThread(company.Id, TimeSpan.FromHours(3)));

                //NOTE: disabled, not need auto purchase
                //results.Add(new PurchaseLabelsForPrimeThread(company.Id, messageService, TimeSpan.FromHours(1)));
                //results.Add(new PurchaseLabelsForAmazonNextDayThread(company.Id, messageService, TimeSpan.FromHours(1)));

                results.Add(new AutoCreateListingsThread(company.Id, messageService, new List<TimeSpan>() { new TimeSpan(4, 0, 0) }, _time));

                results.Add(new RestartServiceThread(company.Id, messageService, fastCallbackInterval));
            }

            results.Add(new UpdateChartInfoThread(company.Id, messageService, new List<TimeSpan> { new TimeSpan(1, 0, 0) }, _time));
            results.Add(new UpdateStampsBalanceThread(company.Id, messageService, processAmazonOrdersInterval));

            results.Add(new UpdateCachesThread(company.Id, messageService, fastCallbackInterval));

            //results.Add(new ListingsFixupThread(company.Id, messageService, new List<TimeSpan>() { new TimeSpan(5, 0, 0) }, _time));
            
            results.Add(new RefreshRatesThread(company.Id, messageService, new List<TimeSpan>()
            {
                new TimeSpan(4, 0, 0)
            }, _time));

            results.Add(new ReValidateAddressThread(company.Id, messageService, TimeSpan.FromMinutes(60)));

            results.Add(new UpdateQuantityDistibutionThread(company.Id, messageService, emailService, updateQuantityDistributionInterval));

            results.Add(new UpdateSalesEndThread(company.Id, messageService, updateSaleEndInterval));

            results.Add(new UpdateQuantityPriceFixupThread(company.Id, messageService, updateQuantityFixupInterval));

            results.Add(new UpdateOrderTrackingStatus(company.Id, messageService, updateOrderTrackingStatusInterval));

            if (company.ShortName == PortalEnum.PA.ToString())
            {
                results.Add(new UpdateEmailsThread(company.Id, messageService, updateEmailsInterval));

                results.Add(new SendEmailsThread(company.Id, messageService, callbackInterval));

                results.Add(new CheckEmailStatusThread(company.Id, messageService, new List<TimeSpan>()
                {
                    new TimeSpan(15, 0, 0)
                },
                _time));

                results.Add(new CheckSupportNotificationsThread(company.Id, messageService, callbackInterval));
            }

            results.Add(new SystemActionsThread(company.Id, messageService, callbackInterval));

            results.Add(new CheckSizeMappingThread(company.Id, messageService, new List<TimeSpan>()
            {
                new TimeSpan(12, 0, 0)
            },
            _time));

            results.Add(new CheckKioskBarcodeThread(company.Id, messageService, TimeSpan.FromMinutes(60)));

            results.Add(new CheckDhlInvoiceThread(company.Id, messageService, new List<TimeSpan>()
            {
                new TimeSpan(10, 0, 0)
            },
            _time));

            results.Add(new BatchArchiveThread(company.Id, messageService, new List<TimeSpan>()
            {
                new TimeSpan(4, 0, 0)
            },
            _time));

            results.Add(new OrderFixupThread(company.Id, messageService, new List<TimeSpan>()
            {
                new TimeSpan(4, 0, 0)
            },
            _time));

            return results;
        }

        private IList<IThread> GetDemoThreads(CompanyDTO company,
            ISystemMessageService messageService,
            IEmailService emailService,
            TimeSpan processAmazonOrdersInterval,
            TimeSpan callbackInterval,
            TimeSpan fastCallbackInterval,
            TimeSpan updateQuantityDistributionInterval,
            TimeSpan updateSaleEndInterval,
            TimeSpan updateQuantityFixupInterval,
            TimeSpan updateOrderTrackingStatusInterval,
            TimeSpan updateEmailsInterval)
        {
            var results = new List<IThread>();

            results.Add(new UpdateDemoTimeStampsThread(company.Id, messageService, TimeSpan.FromMinutes(15)));

            results.Add(new UpdateCachesThread(company.Id, messageService, fastCallbackInterval));

            results.Add(new UpdateQuantityDistibutionThread(company.Id, messageService, emailService, updateQuantityDistributionInterval));

            results.Add(new UpdateSalesEndThread(company.Id, messageService, updateSaleEndInterval));

            results.Add(new UpdateOrderTrackingStatus(company.Id, messageService, updateOrderTrackingStatusInterval));

            results.Add(new SystemActionsThread(company.Id, messageService, callbackInterval));

            return results;
        }

        public void Stop()
        {
            foreach (var thread in threads)
            {
                thread.Stop();
            }
        }
    }
}
