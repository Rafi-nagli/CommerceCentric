using Amazon.Api;
using Amazon.Common.Emails;
using Amazon.Common.Services;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using eBay.Api;
using log4net.Config;
using Magento.Api.Wrapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shopify.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Walmart.Api;

namespace Ccen.UnitTest.Models
{
    public class CcenTestContext
    {
        public readonly ILogService _log = LogFactory.Console;
        //static private UserDTO _user;
        public CompanyDTO _company;
        public SettingsService _settings;
        public MarketplaceKeeper _marketplace;
        public Magento20MarketApi _magentoApi;
        public ShopifyApi _shopifyDWSApi;
        public eBayApi _eBayApi;
        public AmazonApi _amazonApi;
        public WalmartApi _walmartApi;


        public ITime _time;
        public IDbFactory _dbFactory;
        public StyleManager _styleManager;
        public IStyleHistoryService _styleHistoryService;
        public ISystemActionService _actionService;
        public IQuantityManager _quantityManager;
        public IPriceManager _priceManager;
        public IEmailService _emailService;
        public ICacheService _cacheService;
        public IAddressService _addressService;
        public IBarcodeService _barcodeService;
        public AutoCreateBaseListingService _autoCreateNonameListingService;
        public IWeightService _weightService;

        public void Setup()
        {
            Database.SetInitializer<AmazonContext>(null);
            XmlConfigurator.Configure(new FileInfo(AppSettings.log4net_Config));

            _dbFactory = new DbFactory();
            _time = new TimeService(_dbFactory);
            _settings = new SettingsService(_dbFactory);

            _styleHistoryService = new StyleHistoryService(_log, _time, _dbFactory);
            _styleManager = new StyleManager(_log, _time, _styleHistoryService);
            _actionService = new SystemActionService(_log, _time);
            _quantityManager = new QuantityManager(_log, _time);
            _priceManager = new PriceManager(_log, _time, _dbFactory, _actionService, _settings);
            _cacheService = new CacheService(_log, _time, _actionService, _quantityManager);
            _barcodeService = new BarcodeService(_log, _time, _dbFactory);
            _weightService = new WeightService();

            IEmailSmtpSettings smtpSettings = new EmailSmtpSettings();

            using (var db = new UnitOfWork())
            {
                _company = db.Companies.GetFirstWithSettingsAsDto();

                if (AppSettings.IsDebug)
                    smtpSettings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
                else
                    smtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(_company);

                _addressService = AddressService.Default;
                _emailService = new EmailService(_log, smtpSettings, _addressService);

                //todo check itemHist
                _autoCreateNonameListingService = new AutoCreateNonameListingService(_log,
                     _time,
                     _dbFactory,
                     _cacheService,
                     _barcodeService,
                     _emailService, null,
                     AppSettings.IsDebug);

                var marketplaces = new MarketplaceKeeper(_dbFactory, true);
                marketplaces.Init();

                var shipmentPrividers = db.ShipmentProviders.GetByCompanyId(_company.Id);

                var apiFactory = new MarketFactory(marketplaces.GetAll(), _time, _log, _dbFactory, AppSettings.JavaPath);

                var weightService = new WeightService();

                var serviceFactory = new ServiceFactory();
                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    weightService,
                    shipmentPrividers,
                    null,
                    null,
                    null,
                    null);

                _magentoApi = (Magento20MarketApi)apiFactory.GetApi(_company.Id, MarketType.Magento, MarketplaceKeeper.ShopifyDWS);
                _shopifyDWSApi = (ShopifyApi)apiFactory.GetApi(_company.Id, MarketType.Shopify, MarketplaceKeeper.ShopifyDWS);
                _eBayApi = (eBayApi)apiFactory.GetApi(_company.Id, MarketType.eBay, "");
                _amazonApi = (AmazonApi)apiFactory.GetApi(_company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);
                _walmartApi = (WalmartApi)apiFactory.GetApi(_company.Id, MarketType.Walmart, "");
            }
        }

        public static implicit operator TestContext(CcenTestContext v)
        {
            throw new NotImplementedException();
        }
    }
}
