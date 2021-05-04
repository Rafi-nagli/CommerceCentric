using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Audits;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Contracts.Db.Cache;
using Amazon.Core.Contracts.Db.Charts;
using Amazon.Core.Contracts.Db.DropShippers;
using Amazon.Core.Contracts.Db.Emails;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Contracts.Db.Listings;
using Amazon.Core.Contracts.Db.Orders;
using Amazon.DAL.Repositories;
using Amazon.DAL.Repositories.BulkEdits;
using Amazon.DAL.Repositories.Cache;
using Amazon.DAL.Repositories.Categories;
using Amazon.DAL.Repositories.DropShippers;
using Amazon.DAL.Repositories.Emails;
using Amazon.DAL.Repositories.Features;
using Amazon.DAL.Repositories.Inventory;
using Amazon.DAL.Repositories.Listings;
using Amazon.DAL.Repositories.Messages;
using Amazon.DAL.Repositories.Orders;
using Amazon.DAL.Repositories.Sizes;
using Ccen.Core.Contracts.Db.BulkEdit;
using Ccen.Core.Contracts.Db.Trackings;
using Ccen.DAL.Repositories.Reports;
using Ccen.DAL.Repositories.Trackings;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Amazon.DAL
{
    public class UnitOfWork : IQueryableUnitOfWork
    {
        private AmazonContext context;
        private static object _sync = new object();

        public AmazonContext Context
        {
            get
            {
                if (context == null)
                {
                    lock (_sync)
                    {
                        if (context == null)
                        {
                            context = new AmazonContext();
                            //#if DEBUG
                            //context.Database.Log = s => Debug.WriteLine(s);
                            context.Database.Log = msg => Trace.WriteLine(msg);
                            //#endif
                            ((IObjectContextAdapter)context).ObjectContext.CommandTimeout = 300;
                        }
                    }
                }
                return context;
            }
        }

        private ILogService _log;
        public UnitOfWork(ILogService log)
        {
            _log = log;
        }

        public UnitOfWork()
        {
            _log = null;
        }

        public IMarketplaceRepository Marketplaces
        {
            get { return new MarketplaceRepository(this); }
        }

        public IUserRepository Users
        {
            get { return new UserRepository(this); }
        }

        public IRoleRepository Roles
        {
            get { return new RoleRepository(this); }
        }

        public ICompanyRepository Companies
        {
            get { return new CompanyRepository(this); }
        }

        public IShipmentProviderRepository ShipmentProviders
        {
            get { return new ShipmentProviderRepository(this); }
        }

        public IEmailAccountRepository EmailAccounts
        {
            get { return new EmailAccountRepository(this); }
        }

        public ISQSAccountRepository SQSAccounts
        {
            get { return new SQSAccountRepository(this); }
        }

        public IAddressProviderRepository AddressProviders
        {
            get { return new AddressProviderRepository(this); }
        }

        public ICompanyAddressRepository CompanyAddresses
        {
            get { return new CompanyAddressRepository(this); }
        }

        public ISystemActionRepository SystemActions
        {
            get { return new SystemActionRepository(this); }
        }

        public IItemRepository Items
        {
            get { return new ItemRepository(this); }
        }

        public IItemImageRepository ItemImages
        {
            get { return new ItemImageRepository(this); }
        }

        public IListingDefectRepository ListingDefects
        {
            get { return new ListingDefectRepository(this); }
        }

        public IItemChangeHistoryRepository ItemChangeHistories
        {
            get { return new ItemChangeHistoryRepository(this); }
        }

        public IItemAdditionRepository ItemAdditions
        {
            get { return new ItemAdditionRepository(this); }
        }

        public IListingSizeIssueRepository ListingSizeIssues
        {
            get { return new ListingSizeIssueRepository(this); }
        }

        public IListingRepository Listings
        {
            get { return new ListingRepository(this); }
        }


        public IListingFBAEstFeeRepository ListingFBAEstFees
        {
            get { return new ListingFBAEstFeeRepository(this); }
        }

        public IListingFBAInvRepository ListingFBAInvs
        {
            get { return new ListingFBAInvRepository(this); }
        }

        public IParentItemRepository ParentItems
        {
            get { return new ParentItemRepository(this); }
        }

        public IParentItemImageRepository ParentItemImages
        {
            get { return new ParentItemImageRepository(this); }
        }

        public IProductCommentRepository ProductComments
        {
            get { return new ProductCommentRepository(this); }
        }

        public IPriceHistoryRepository PriceHistories
        {
            get { return new PriceHistoryRepository(this); }
        }

        public IQuantityHistoryRepository QuantityHistories
        {
            get { return new QuantityHistoryRepository(this); }
        }

        public IOrderRepository Orders
        {
            get { return new OrderRepository(this); }
        }

        public IOrderItemRepository OrderItems
        {
            get { return new OrderItemRepository(this); }
        }

        public IOrderItemSourceRepository OrderItemSources
        {
            get { return new OrderItemSourceRepository(this); }
        }

        public IOrderCommentRepository OrderComments
        {
            get { return new OrderCommentRepository(this); }
        }

        public IOrderNotifyRepository OrderNotifies
        {
            get { return new OrderNotifyRepository(this); }
        }

        public ITrackingNumberStatusRepository TrackingNumberStatuses
        {
            get { return new TrackingNumberStatusRepository(this); }
        }

        public ICustomTrackingNumberRepository CustomTrackingNumbers
        {
            get { return new CustomTrackingNumberRepository(this); }
        }

        public ISystemMessageRepository SystemMessages
        {
            get { return new SystemMessageRepository(this); }
        }

        public IOrderEmailNotifyRepository OrderEmailNotifies
        {
            get { return new OrderEmailNotifyRepository(this); }
        }

        public IOrderShippingInfoRepository OrderShippingInfos
        {
            get { return new OrderShippingInfoRepository(this); }
        }

        public ICustomerRepository Customers
        {
            get { return new CustomerRepository(this); }
        }

        public IOrderSearchRepository OrderSearches
        {
            get { return new OrderSearchRepository(this); }
        }

        public IOrderChangeHistoryRepository OrderChangeHistories
        {
            get { return new OrderChangeHistoryRepository(this); }
        }

        public IScanFormRepository ScanForms
        {
            get { return new ScanFormRepository(this); }
        }

        public IScheduledPickupRepository ScheduledPickups
        {
            get { return new ScheduledPickupRepository(this); }
        }

        public IDhlInvoiceRepository DhlInvoices
        {
            get { return new DhlInvoiceRepository(this); }
        }

        public IDhlECommerceRateRepository DhlECommerceRates
        {
            get { return new DhlECommerceRateRepository(this); }
        }

        public IDhlRateCodeRepository DhlRateCodes
        {
            get { return new DhlRateCodeRepository(this); }
        }

        public IDhlRateCodePriceRepository DhlRateCodePrices
        {
            get { return new DhlRateCodePriceRepository(this); }
        }

        public ISkyPostalCityCodeRepository SkyPostalCityCodes
        {
            get { return new SkyPostalCityCodeRepository(this); }
        }

        public ISkyPostalRateRepository SkyPostalRates
        {
            get { return new SkyPostalRateRepository(this); }
        }
        public ISkyPostalZoneRepository SkyPostalZones
        {
            get { return new SkyPostalZoneRepository(this); }
        }

        public IRateByCountryRepository RateByCountries
        {
            get { return new RateByCountryRepository(this); }
        }

        public IReturnRequestRepository ReturnRequests
        {
            get { return new ReturnRequestRepository(this); }
        }

        public IReturnRequestItemRepository ReturnRequestItems
        {
            get { return new ReturnRequestItemRepository(this); }
        }

        public ITrackingOrderRepository TrackingOrders
        {
            get { return new TrackingOrderRepository(this); }
        }

        public IBuyerRepository Buyers
        {
            get { return new BuyerRepository(this); }
        }

        public IBuyerBlackListRepository BuyerBlackLists
        {
            get { return new BuyerBlackListRepository(this); }
        }

        public IFeedbackBlackListRepository FeedbackBlackLists
        {
            get { return new FeedbackBlackListRepository(this); }
        }


        public IItemOrderMappingRepository ItemOrderMappings
        {
            get { return new ItemOrderMappingRepository(this); }
        }

        public IInventoryReportRepository InventoryReports
        {
            get { return new InventoryReportRepository(this); }
        }

        public IDateRepository Dates
        {
            get { return new DateRepository(this); }
        }

        public IZipCodeRepository ZipCodes
        {
            get { return new ZipCodeRepository(this); }
        }

        public IZipCodeZoneRepository ZipCodeZones
        {
            get { return new ZipCodeZoneRepository(this); }
        }

        public IDhlGroundZipCodeRepository DhlGroundZipCodes
        {
            get { return new DhlGroundZipCodeRepository(this); }
        }

        public IDhlCAZipCodeZoneRepository DhlCAZipCodeZones
        {
            get { return new DhlCAZipCodeZoneRepository(this); }
        }

        public IDhlGBZipCodeZoneRepository DhlGBZipCodeZones
        {
            get { return new DhlGBZipCodeZoneRepository(this); }
        }

        public IWalmartBrandInfoRepository WalmartBrandInfoes
        {
            get { return new WalmartBrandInfoRepository(this); }
        }

        public IIBCRateRepository IBCRates
        {
            get { return new IBCRateRepository(this); }
        }

        public ICountryRepository Countries
        {
            get { return new CountryRepository(this); }
        }

        public IStateRepository States
        {
            get { return new StateRepository(this); }
        }

        public IFeedRepository Feeds
        {
            get { return new FeedRepository(this); }
        }

        public IFeedMessageRepository FeedMessages
        {
            get { return new FeedMessageRepository(this); }
        }

        public IFeedItemRepository FeedItems
        {
            get { return new FeedItemRepository(this); }
        }


        public IEmailRepository Emails
        {
            get { return new EmailRepository(this); }
        }

        public IEmailAttachmentRepository EmailAttachments
        {
            get { return new EmailAttachmentRepository(this); }
        }

        public IEmailToOrderRepository EmailToOrders
        {
            get { return new EmailToOrderRepository(this); }
        }


        public ISettingRepository Settings
        {
            get { return new SettingRepository(this); }
        }

        public IProxyInfoRepository ProxyInfos
        {
            get { return new ProxyInfoRepository(this); }
        }

        public IPushRepository Pushes
        {
            get { return new PushRepository(this); }
        }

        public IReportRepository SalesReports
        {
            get { return new ReportRepository(this); }
        }

        public IQuickBookRepository QuickBooks
        {
            get { return new QuickBookRepository(this); }
        }

        //Vendors
        public IVendorOrderRepository VendorOrders
        {
            get { return new VendorOrderRepository(this); }
        }

        public IVendorOrderItemRepository VendorOrderItems
        {
            get { return new VendorOrderItemRepository(this); }
        }

        public IVendorOrderItemSizeRepository VendorOrderItemSizes
        {
            get { return new VendorOrderItemSizeRepository(this); }
        }

        public IVendorOrderAttachmentRepository VendorOrderAttachments
        {
            get { return new VendorOrderAttachmentRepository(this); }
        }

        //Sale events
        public ISaleEventRepository SaleEvents
        {
            get { return new SaleEventRepository(this); }
        }

        public ISaleEventEntryRepository SaleEventEntries
        {
            get { return new SaleEventEntryRepository(this); }
        }

        public ISaleEventSizeHoldInfoRepository SaleEventSizeHoldInfoes
        {
            get { return new SaleEventSizeHoldInfoRepository(this); }
        }
        public ISaleEventFeedRepository SaleEventFeeds
        {
            get { return new SaleEventFeedRepository(this); }
        }


        //Reports
        public ICustomReportRepository CustomReports
        {
            get { return new CustomReportRepository(this); }
        }

        public ICustomReportPredefinedFieldRepository CustomReportPredefinedFields
        {
            get { return new CustomReportPredefinedFieldRepository(this); }
        }

        public ICustomReportFilterRepository CustomReportFilters
        {
            get { return new CustomReportFilterRepository(this); }
        }

       

        #region INVENTORY
        public IStyleRepository Styles
        {
            get { return new StyleRepository(this); }
        }

        public IStyleChangeHistoryRepository StyleChangeHistories
        {
            get { return new StyleChangeHistoryRepository(this); }
        }


        public IStyleImageRepository StyleImages
        {
            get { return new StyleImageRepository(this); }
        }

        public IScannedRepository Scanned
        {
            get { return new ScannedRepository(this); }
        }

        public ISealedBoxRepository SealedBoxes
        {
            get { return new SealedBoxRepository(this); }
        }

        public IOpenBoxRepository OpenBoxes
        {
            get { return new OpenBoxRepository(this); }
        }

        public IStyleLocationRepository StyleLocations
        {
            get { return new StyleLocationRepository(this); }
        }

        public IStyleItemRepository StyleItems
        {
            get { return new StyleItemRepository(this); }
        }

        public IStyleItemAttributeRepository StyleItemAttributes
        {
            get { return new StyleItemAttributeRepository(this); }
        }


        public IStyleItemSaleRepository StyleItemSales
        {
            get { return new StyleItemSaleRepository(this); }
        }

        public IStyleItemSaleToListingRepository StyleItemSaleToListings
        {
            get { return new StyleItemSaleToListingRepository(this); }
        }

        public IStyleItemSaleToMarketRepository StyleItemSaleToMarkets
        {
            get { return new StyleItemSaleToMarketRepository(this); }
        }

        public IStyleReferenceRepository StyleReferences
        {
            get { return new StyleReferenceRepository(this); }
        }

        public IStyleItemReferenceRepository StyleItemReferences
        {
            get { return new StyleItemReferenceRepository(this); }
        }


        public IStyleItemBarcodeRepository StyleItemBarcodes
        {
            get { return new StyleItemBarcodeRepository(this); }
        }

        public IStyleItemQuantityHistoryRepository StyleItemQuantityHistories
        {
            get { return new StyleItemQuantityHistoryRepository(this); }
        }

        public IStyleItemActionHistoryRepository StyleItemActionHistories
        {
            get { return new StyleItemActionHistoryRepository(this); }
        }

        public IQuantityOperationRepository QuantityOperations
        {
            get { return new QuantityOperationRepository(this); }
        }

        public IQuantityChangeRepository QuantityChanges
        {
            get { return new QuantityChangeRepository(this); }
        }


        public IOpenBoxItemRepository OpenBoxItems
        {
            get { return new OpenBoxItemRepository(this); }
        }

        public IOpenBoxTrackingRepository OpenBoxTrackings
        {
            get { return new OpenBoxTrackingRepository(this); }
        }

        public ISealedBoxItemRepository SealedBoxItems
        {
            get { return new SealedBoxItemRepository(this); }
        }

        public ISealedBoxTrackingRepository SealedBoxTrackings
        {
            get { return new SealedBoxTrackingRepository(this); }
        }

        public IVendorInvoiceRepository VendorInvoices
        {
            get { return new VendorInvoiceRepository(this); }
        }

        public IFeatureRepository Features
        {
            get { return new FeatureRepository(this); }
        }

        public IFeatureValueRepository FeatureValues
        {
            get { return new FeatureValueRepository(this); }
        }

        public IStyleFeatureValueRepository StyleFeatureValues
        {
            get { return new StyleFeatureValueRepository(this); }
        }

        public IStyleFeatureTextValueRepository StyleFeatureTextValues
        {
            get { return new StyleFeatureTextValueRepository(this); }
        }

        public IFeatureToItemTypeRepository FeatureToItemTypes
        {
            get { return new FeatureToItemTypeRepository(this); }
        }

        public ISizeRepository Sizes
        {
            get { return new SizeRepository(this); }
        }

        public ISizeGroupRepository SizeGroups
        {
            get { return new SizeGroupRepository(this); }
        }

        public ISizeGroupToItemTypeRepository SizeGroupToItemTypes
        {
            get { return new SizeGroupToItemTypeRepository(this); }
        }

        public IItemTypeRepository ItemTypes
        {
            get { return new ItemTypeRepository(this); }
        }


        //Counting
        public ISealedBoxCountingRepository SealedBoxCountings
        {
            get { return new SealedBoxCountingRepository(this); }
        }

        public IOpenBoxCountingRepository OpenBoxCountings
        {
            get { return new OpenBoxCountingRepository(this); }
        }

        public IOpenBoxCountingItemRepository OpenBoxCountingItems
        {
            get { return new OpenBoxCountingItemRepository(this); }
        }

        public ISealedBoxCountingItemRepository SealedBoxCountingItems
        {
            get { return new SealedBoxCountingItemRepository(this); }
        }

        #endregion

        public IFBAPickListRepository FBAPickLists
        {
            get { return new FBAPickListRepository(this); }
        }

        public IFBAPickListEntryRepository FBAPickListEntries
        {
            get { return new FBAPickListEntryRepository(this); }
        }

        public IPhotoshootPickListRepository PhotoshootPickLists
        {
            get { return new PhotoshootPickListRepository(this); }
        }

        public IPhotoshootPickListEntryRepository PhotoshootPickListEntries
        {
            get { return new PhotoshootPickListEntryRepository(this); }
        }

        public ICustomBarcodeRepository CustomBarcodes
        {
            get { return new CustomBarcodeRepository(this); }
        }

        public ISizeMappingRepository SizeMappings
        {
            get { return new SizeMappingRepository(this); }
        }

        public IPackingSlipSizeMappingRepository PackingSlipSizeMappings
        {
            get { return new PackingSlipSizeMappingRepository(this); }
        }

        public IAmazonCategoryMappingRepository AmazonCategoryMappings
        {
            get { return new AmazonCategoryMappingRepository(this); }
        }

        public IShippingMethodRepository ShippingMethods
        {
            get { return new ShippingMethodRepository(this); }
        }

        public IShippingChargeRepository ShippingCharges
        {
            get { return new ShippingChargeRepository(this); }
        }

        public IMailLabelInfoRepository MailLabelInfos
        {
            get { return new MailLabelInfoRepository(this); }
        }

        public IMailLabelItemRepository MailLabelItems
        {
            get { return new MailLabelItemRepository(this); }
        }

        public ILabelRepository Labels
        {
            get { return new LabelRepository(this); }
        }

        public ILabelPrintPackRepository LabelPrintPacks
        {
            get { return new LabelPrintPackRepository(this); }
        }

        public INodePositionRepository NodePositions
        {
            get { return new NodePositionRepository(this); }
        }

        public IOrderBatchRepository OrderBatches
        {
            get { return new OrderBatchRepository(this); }
        }

        public IOrderToBatchRepository OrderToBatches
        {
            get { return new OrderToBatchRepository(this); }
        }

        public ISyncHistoryRepository SyncHistory
        {
            get { return new SyncHistoryRepository(this); }
        }

        public ISyncMessageRepository SyncMessages
        {
            get { return new SyncMessageRepository(this); }
        }

        public INotificationRepository Notifications
        {
            get { return new NotificatonRepository(this); }
        }

        public IBuyBoxStatusRepository BuyBoxStatus
        {
            get { return new BuyBoxStatusRepository(this); }
        }

        public IOfferChangeEventRepository OfferChangeEvents
        {
            get { return new OfferChangeEventRepository(this); }
        }

        public IOfferInfoRepository OfferInfoes
        {
            get { return new OfferInfoRepository(this); }
        }

        public IBuyBoxQuantityRepository BuyBoxQuantities
        {
            get { return new BuyBoxQuantityRepository(this); }
        }


        //DropShippers
        public IDropShipperRepository DropShippers
        {
            get { return new DropShipperRepository(this); }
        }

        public IDSItemRepository DSItems
        {
            get { throw new NotImplementedException(); }
        }

        public IDSFileRepository DSFiles
        {
            get { throw new NotImplementedException(); }
        }

        public IDSFileLineRepository DSFileLines
        {
            get { throw new NotImplementedException(); }
        }

        public IDSFileMessageRepository DSFileMessages
        {
            get { throw new NotImplementedException(); }
        }

        public IDSFileLineMessageRepository DSFileLineMessages
        {
            get { throw new NotImplementedException(); }
        }

        public IStyleDSHistoryRepository StyleDSHistories { get; }

        public ICustomCategoryRepository CustomCategories
        {
            get { return new CustomCategoryRepository(this); }
        }

        public ICustomCategoryFilterRepository CustomCategoryFilters
        {
            get { return new CustomCategoryFilterRepository(this); }
        }

        public ICustomCategoryToStyleRepository CustomCategoryToStyles
        {
            get { return new CustomCategoryToStyleRepository(this); }
        }

        //Chart
        public IChartRepository Charts
        {
            get { return new ChartRepository(this); }
        }

        public IChartPointRepository ChartPoints
        {
            get { return new ChartPointRepository(this); }
        }

        #region CACHE
        public IParentItemCacheRepository ParentItemCaches
        {
            get { return new ParentItemCacheRepository(this); }
        }

        public IListingCacheRepository ListingCaches
        {
            get { return new ListingCacheRepository(this); }
        }

        public IItemCacheRepository ItemCaches
        {
            get { return new ItemCacheRepository(this); }
        }

        public IStyleCacheRepository StyleCaches
        {
            get { return new StyleCacheRepository(this); }
        }

        public IStyleItemCacheRepository StyleItemCaches
        {
            get { return new StyleItemCacheRepository(this); }
        }
        #endregion

        //Audit
        public IAuditRepository Audits
        {
            get { return new AuditRepository(this); }
        }

        public ISkuMappingRepository SkuMappings
        {
            get
            {
                throw new NotImplementedException();
            }
        }



        public virtual IOrderReportRepository OrderReports
        {
            get { return new OrderReportRepository(this); }
        }

        public virtual IShipmentReportRepository ShipmentReports
        {
            get { return new ShipmentReportRepository(this); }
        }

        public IStyleGroupRepository StyleGroups
        {
            get { return new StyleGroupRepository(this); }
        }

        public IStyleToGroupRepository StyleToGroups
        {
            get { return new StyleToGroupRepository(this); }
        }

        public ICustomFeedRepository CustomFeeds
        {
            get { return new CustomFeedRepository(this); }
        }        
        public ICustomFeedScheduleRepository CustomFeedSchedules
        {
            get { return new CustomFeedScheduleRepository(this); }
        }
        public ICustomFeedFieldRepository CustomFeedFields
        {
            get { return new CustomFeedFieldRepository(this); }
        }


        public IBulkEditOperationRepository BulkEditOperations
        {
            get { return new BulkEditOperationRepository(this); }
        }

        public IBulkEditHistoryRepository BulkEditHistories
        {
            get { return new BulkEditHistoryRepository(this); }
        }

        public ICustomReportFieldRepository CustomReportFields
        {
            get { return new CustomReportFieldRepository(this); }
        }

        public IStyleAdditionRepository StyleAdditions => throw new NotImplementedException();


        #region IQueryableUnitOfWork impl
        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }

        public void DisableProxyCreation()
        {
            Context.Configuration.ProxyCreationEnabled = false;
        }

        public void DisableAutoDetectChanges()
        {
            Context.Configuration.AutoDetectChangesEnabled = false;
        }

        public void EnableValidation()
        {
            //Context.Configuration.AutoDetectChangesEnabled = true;
            Context.Configuration.ValidateOnSaveEnabled = true;
            //Context.Configuration.ProxyCreationEnabled = true;
        }

        public void DisableValidation()
        {
            //Context.Configuration.AutoDetectChangesEnabled = false;
            Context.Configuration.ValidateOnSaveEnabled = false;
            //Context.Configuration.ProxyCreationEnabled = false;
        }

        public void Commit()
        {
            var shouldRetry = false;
            do
            {
                try
                {
                    if (shouldRetry == true)
                    {
                        EnableLogging();
                        LogInfo("Begin retry");
                    }

                    Context.SaveChanges();

                    if (shouldRetry == true)
                    {
                        LogInfo("End retry");
                        DisableLogging();
                        shouldRetry = false;
                    }
                }
                catch (SqlException ex)
                {
                    if (shouldRetry == true)
                    {
                        //NOTE: retry only once
                        LogInfo("End retry");
                        DisableLogging();
                        shouldRetry = false;
                    }
                    else
                    {
                        //https://stackoverflow.com/questions/43087182/conflicted-transactions/43087896#43087896
                        /*System.Data.SqlClient.SqlException: Snapshot isolation transaction aborted due to update conflict. You cannot use snapshot isolation to access table 'dbo.Users' directly or indirectly in database 'IUMobileDbRelease' to update, delete, or insert the row that has been modified or deleted by another transaction. Retry the transaction or change the isolation level for the update/delete statement.*/
                        if (ex.Message.Contains("Snapshot isolation transaction aborted due to update conflict")
                            && ex.Message.Contains("Retry the transaction"))
                        {
                            LogInfo("Enable retrying, sqlexeption:" + ex.Message);
                            shouldRetry = true;
                        }
                    }
                }
            }
            while (shouldRetry);
        }

        private Action<string> _previousLogAction;
        private void EnableLogging()
        {
            _previousLogAction = context.Database.Log;
            context.Database.Log = msg => _log.Error(msg);
        }

        private void DisableLogging()
        {
            context.Database.Log = _previousLogAction;
            _previousLogAction = null;
        }

        private void LogInfo(string message)
        {
            if (_log != null)
                _log.Info(message);
            else
                Trace.WriteLine(message);
        }

        public void DetectChanges()
        {
            Context.ChangeTracker.DetectChanges();
        }

        public void ReCreate()
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }

        public void CommitAndRefreshChanges()
        {
            throw new NotSupportedException();
        }

        public void RollbackChanges()
        {
            throw new NotSupportedException();
        }

        public IEnumerable<T> ExecuteQuery<T>(string sqlQuery, params object[] parameters)
        {
            return Context.Database.SqlQuery<T>(sqlQuery, parameters).ToList();
        }

        public int ExecuteCommand(string sqlCommand, params object[] parameters)
        {
            return Context.Database.ExecuteSqlCommand(sqlCommand, parameters);
        }

        public DbSet<TEntity> GetSet<TEntity>() where TEntity : class
        {
            return Context.Set<TEntity>();
        }

        public void Attach<TEntity>(TEntity item) where TEntity : class
        {
            GetSet<TEntity>().Attach(item);
        }

        public void SetModified<TEntity>(TEntity item) where TEntity : class
        {
            throw new NotSupportedException();
        }

        public void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
