using System;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Common.Services;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Orders;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.General;
using Amazon.Model.General.Markets.Amazon;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Labels;
using Amazon.Model.Implementation.Pdf;
using Amazon.Web.Models;
using Ccen.Web;
using log4net;

namespace Amazon.Web.Controllers
{
    public partial class BaseController : Controller
    {
        private IUnitOfWork _db;
        public IUnitOfWork Db
        {
            get
            {
                if (_db == null)
                    _db = DbFactory.GetRWDb();
                return _db;
            }
        }

        private IUnitOfWork _readDb;
        public IUnitOfWork ReadDb
        {
            get
            {
                if (_readDb == null)
                {
                    _readDb = DbFactory.GetRDb();
                }
                return _readDb;
            }
        }

        private IDbFactory _dbFactory;

        public IDbFactory DbFactory
        {
            get
            {
                if (_dbFactory == null)
                {
                    _dbFactory = new DbFactory();
                }
                return _dbFactory;
            }
        }

        private ITime _time;
        public ITime Time
        {
            get
            {
                if (_time == null)
                    _time = new TimeService(DbFactory);
                return _time;
            }
        }

        private IWeightService _weightService;

        public IWeightService WeightService
        {
            get
            {
                if (_weightService == null)
                    _weightService = new WeightService();
                return _weightService;
            }
        }

        private ISystemMessageService _messageService;

        public ISystemMessageService MessageService
        {
            get
            {
                if (_messageService == null)
                    _messageService = new SystemMessageService(LogService, Time, DbFactory);
                return _messageService;
            }
        }


        private IShippingService _shippingService;

        public IShippingService ShippingService
        {
            get
            {
                if (_shippingService == null)
                    _shippingService = new ShippingService(DbFactory, AccessManager.IsFulfilment);
                return _shippingService;
            }
        }

        private IMarketplaceService _marketplaceService;

        public IMarketplaceService MarketplaceService
        {
            get
            {
                if (_marketplaceService == null)
                    _marketplaceService = new MarketplaceKeeper(DbFactory, true);
                return _marketplaceService;
            }
        }

        private IHtmlScraperService _htmlScraper;

        public IHtmlScraperService HtmlScraper
        {
            get
            {
                if (_htmlScraper == null)
                    _htmlScraper = new HtmlScraperService(LogService, Time, DbFactory);
                return _htmlScraper;
            }
        }

        private IMarketCategoryService _amazonCategoryService;

        public IMarketCategoryService AmazonCategoryService
        {
            get
            {
                if (_amazonCategoryService == null)
                    _amazonCategoryService = new AmazonCategoryService(LogService, Time, DbFactory);
                return _amazonCategoryService;
            }
        }

        private IFileMaker _pdfMaker;

        public IFileMaker PdfMaker
        {
            get
            {
                if (_pdfMaker == null)
                    _pdfMaker = new PdfMakerByIText(LogService);

                return _pdfMaker;
            }
        }

        private IAutoCreateListingService _autoCreateListingService;

        public IAutoCreateListingService AutoCreateListingService
        {
            get
            {
                if (_autoCreateListingService == null)
                    _autoCreateListingService = new AutoCreateNonameListingService(LogService, 
                        Time,
                        DbFactory,
                        Cache,
                        BarcodeService,
                        EmailService,
                        ItemHistoryService,
                        AppSettings.IsDebug);
                return _autoCreateListingService;
            }
        }


        private IServiceFactory _serviceFactory;

        public IServiceFactory ServiceFactory
        {
            get
            {
                if (_serviceFactory == null)
                    _serviceFactory = new ServiceFactory();
                return _serviceFactory;
            }
        }

        private IEmailService _emailService;

        public IEmailService EmailService
        {
            get
            {
                if (_emailService == null)
                    _emailService = new EmailService(LogService, SettingsBuilder.GetSmtpSettingsFromCompany(AccessManager.Company), AddressService);
                return _emailService;
            }
        }

        private ISystemActionService _actionService;

        public ISystemActionService ActionService
        {
            get
            {
                if (_actionService == null)
                    _actionService = new SystemActionService(LogService, Time);
                return _actionService;
            }
        }

        private IItemHistoryService _itemHistoryService;

        public IItemHistoryService ItemHistoryService
        {
            get
            {
                if (_itemHistoryService == null)
                    _itemHistoryService = new ItemHistoryService(LogService, Time, DbFactory);
                return _itemHistoryService;
            }
        }

        private IBarcodeService _barcodeService;

        public IBarcodeService BarcodeService
        {
            get
            {
                if (_barcodeService == null)
                    _barcodeService = new BarcodeService(LogService, Time, DbFactory);
                return _barcodeService;
            }
        }

        private ICacheService _cache;
        public ICacheService Cache
        {
            get
            {
                if (_cache == null)
                    _cache = new CacheService(LogService, Time, ActionService, QuantityManager);
                return _cache;
            }
        }

        private ISettingsService _settings;
        public ISettingsService Settings
        {
            get
            {
                if (_settings == null)
                    _settings = new SettingsService(DbFactory);
                return _settings;
            }
        }

        private IQuantityManager _quantityManager;

        public IQuantityManager QuantityManager
        {
            get
            {
                if (_quantityManager == null)
                    _quantityManager = new QuantityManager(LogService, Time);
                return _quantityManager;
            }
        }

        private IPriceManager _priceManager;

        public IPriceManager PriceManager
        {
            get
            {
                if (_priceManager == null)
                    _priceManager = new PriceManager(LogService, Time, DbFactory, ActionService, Settings);
                return _priceManager;
            }
        }

        private IPriceService _priceService;

        public IPriceService PriceService
        {
            get
            {
                if (_priceService == null)
                    _priceService = new PriceService(DbFactory);
                return _priceService;
            }
        }

        private IOrderHistoryService _orderHistoryService;
        public IOrderHistoryService OrderHistoryService
        {
            get
            {
                if (_orderHistoryService == null)
                    _orderHistoryService = new OrderHistoryService(LogService, Time, DbFactory);
                return _orderHistoryService;
            }
        }

        private IStyleHistoryService _styleHistoryService;
        public IStyleHistoryService StyleHistoryService
        {
            get
            {
                if (_styleHistoryService == null)
                    _styleHistoryService = new StyleHistoryService(LogService, Time, DbFactory);
                return _styleHistoryService;
            }
        }

        private IBatchManager _batchManager;
        public IBatchManager BatchManager
        {
            get
            {
                if (_batchManager == null)
                    _batchManager = new BatchManager(LogService, Time, OrderHistoryService, WeightService);
                return _batchManager;
            }
        }

        protected override JsonResult Json(object data, 
            string contentType, 
            System.Text.Encoding contentEncoding, 
            JsonRequestBehavior behavior)
        {
            return new JsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }

        protected JsonResult JsonGet(object data)
        {
            return Json(data, null, null, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult CustomJson(object data,
            string contentType, 
            System.Text.Encoding contentEncoding, 
            JsonRequestBehavior behavior)
        {
            return new CustomJsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue,
            };
        }

        protected JsonResult CustomJsonGet(object data)
        {
            return CustomJson(data, null, null, JsonRequestBehavior.AllowGet);
        }

        public string GetUsername()
        {
            var user = AccessManager.User;
            if (user != null)
                return user.Name;
            return null;
        }
        
        public UserDTO GetUser()
        {
            return AccessManager.User;
        }

        #region Log Service

        public virtual string TAG
        {
            get
            {
                return "Controller.";
            }
        }
        
        private ILogService _logService;
        public ILogService LogService
        {
            get
            {
                if (_logService == null)
                {
                     var log = LogManager.GetLogger("RequestLogger");
                     _logService = new FileLogService(log, AccessManager.UserId.ToString());
                }
                return _logService;
            }
        }

        private IAddressService _addressService;

        public IAddressService AddressService
        {
            get
            {
                if (_addressService == null)
                {
                    _addressService = new AddressService(null, CompanyAddress.GetReturnAddress(MarketIdentifier.Empty()), CompanyAddress.GetPickupAddress(MarketIdentifier.Empty()));
                }
                return _addressService;
            }
        }

        private ICompanyAddressService _companyAddress;

        public ICompanyAddressService CompanyAddress
        {
            get
            {
                if (_companyAddress == null)
                {
                    _companyAddress = new CompanyAddressService(AccessManager.Company);
                }
                return _companyAddress;
            }
        }

        private ISystemActionService _systemActionService;
        public ISystemActionService SystemActions
        {
            get
            {
                if (_systemActionService == null)
                {
                    _systemActionService = new SystemActionService(LogService, Time);
                }
                return _systemActionService;
            }    
        }

        protected ILogEntry LogD(string message)
        {
            return LogService.Debug(TAG + message);
        }

        protected ILogEntry LogI(string message)
        {
            var by = AccessManager.UserId;
            return LogService.Info(TAG + " (by: " + by + ") " + message);
        }

        protected ILogEntry LogE(string message)
        {
            var by = AccessManager.UserId;
            return LogService.Error(TAG + " (by: " + by + ") " + message);
        }

        protected ILogEntry LogE(string message, Exception ex)
        {
            var by = AccessManager.UserId;
            return LogService.Error(TAG + " (by: " + by + ") " + message, ex);
        }

        protected ILogEntry LogF(string message, Exception ex)
        {
            var by = AccessManager.UserId;
            return LogService.Fatal(TAG + " (by: " + by + ") " + message, ex);
        }
        #endregion
    }
}
