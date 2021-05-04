using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Markets.DS;
using Amazon.Model.Implementation.Markets.eBay;
using Amazon.Model.Implementation.Markets.Jet;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Model.Implementation.Markets.Shopify;
using Amazon.Model.Implementation.Markets.SupplierOasis;
using Amazon.Model.Implementation.Markets.User;
using Amazon.Model.Implementation.Markets.Walmart;
using Amazon.Model.Implementation.Markets.WooCommerce;
using Shopify.Api;

namespace Amazon.Model.Implementation.Sync
{
    public class OrderSyncFactory
    {
        public IOrderSynchronizer GetForMarket(IMarketApi api,
            ILogService log,
            CompanyDTO company,
            ISettingsService settings,
            ISyncInformer syncInfo,
            IList<IShipmentApi> rateProviders,
            IQuantityManager quantityManager,
            IEmailService emailService,
            IOrderValidatorService validatorService,
            IOrderHistoryService orderHistoryService,
            ICacheService cacheService,
            ISystemActionService systemAction,
            ICompanyAddressService companyAddress,
            ITime time,
            IWeightService weightService,
            ISystemMessageService messageService)
        {
            IOrderSynchronizer synchronizer = null;
            switch (api.Market)
            {
                case MarketType.Amazon:
                case MarketType.AmazonEU:
                case MarketType.AmazonAU:
                    synchronizer = new AmazonOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.eBay:
                    synchronizer = new EBayOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.Magento:
                    synchronizer = new MagentoOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.Groupon:
                    synchronizer = new GrouponOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.Shopify:
                    synchronizer = new ShopifyOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.WooCommerce:
                    synchronizer = new WooCommerceOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.Walmart:
                case MarketType.WalmartCA:
                    synchronizer = new WalmartOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.Jet:
                    synchronizer = new JetOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.DropShipper:
                    synchronizer = new DSOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.OverStock:
                    synchronizer = new SupplierOasisOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                case MarketType.OfflineOrders:
                    synchronizer = new UserOrdersSynchronizer(log,
                        api,
                        company,
                        settings,
                        syncInfo,
                        rateProviders,
                        quantityManager,
                        emailService,
                        validatorService,
                        orderHistoryService,
                        cacheService,
                        systemAction,
                        companyAddress,
                        time,
                        weightService,
                        messageService);
                    break;
                default:
                    throw new NotImplementedException("MarketType not supported=" + api.Market.ToString());
                    break;
            }

            return synchronizer;
        }
    }
}
