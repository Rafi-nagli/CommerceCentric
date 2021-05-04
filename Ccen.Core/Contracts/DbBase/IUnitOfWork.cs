using System;
using System.Linq;
using Amazon.Core.Contracts.Audits;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Contracts.Db.Cache;
using Amazon.Core.Contracts.Db.Charts;
using Amazon.Core.Contracts.Db.DropShippers;
using Amazon.Core.Contracts.Db.Emails;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Contracts.Db.Listings;
using Amazon.Core.Contracts.Db.Orders;
using Amazon.DTO.Graphs;
using Ccen.Core.Contracts.Db.BulkEdit;
using Ccen.Core.Contracts.Db.Trackings;

namespace Amazon.Core
{
    public interface IUnitOfWork : IDisposable
    {
        IMarketplaceRepository Marketplaces { get; }

        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        ICompanyRepository Companies { get; }
        IEmailAccountRepository EmailAccounts { get; }
        ISQSAccountRepository SQSAccounts { get; }
        IShipmentProviderRepository ShipmentProviders { get; }
        IAddressProviderRepository AddressProviders { get; }
        ICompanyAddressRepository CompanyAddresses { get; }

        ISystemActionRepository SystemActions { get; }

        IItemRepository Items { get; }
        IItemImageRepository ItemImages { get; }
        IParentItemRepository ParentItems { get; }
        IParentItemImageRepository ParentItemImages { get; }
        IListingRepository Listings { get; }
        IListingDefectRepository ListingDefects { get; }
        IItemAdditionRepository ItemAdditions { get; }
        IItemChangeHistoryRepository ItemChangeHistories { get; }

        IListingSizeIssueRepository ListingSizeIssues { get; }
        IListingFBAEstFeeRepository ListingFBAEstFees { get; }
        IListingFBAInvRepository ListingFBAInvs { get; }

        IProductCommentRepository ProductComments { get; }
        IPriceHistoryRepository PriceHistories { get; }
        IQuantityHistoryRepository QuantityHistories { get; }

        
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IOrderItemSourceRepository OrderItemSources { get; }
        IOrderNotifyRepository OrderNotifies { get; }

        ITrackingNumberStatusRepository TrackingNumberStatuses { get; }
        ICustomTrackingNumberRepository CustomTrackingNumbers { get; }

        ISystemMessageRepository SystemMessages { get; }
        IOrderEmailNotifyRepository OrderEmailNotifies { get; }
        IOrderCommentRepository OrderComments { get; }
        IOrderSearchRepository OrderSearches { get; }
        IOrderChangeHistoryRepository OrderChangeHistories { get; }

        IScanFormRepository ScanForms { get; }
        IScheduledPickupRepository ScheduledPickups { get; }
        IDhlInvoiceRepository DhlInvoices { get; }
        IDhlECommerceRateRepository DhlECommerceRates { get; }
        IDhlRateCodeRepository DhlRateCodes { get; }
        IDhlRateCodePriceRepository DhlRateCodePrices { get; }
        IRateByCountryRepository RateByCountries { get; }
        ISkyPostalCityCodeRepository SkyPostalCityCodes { get; }

        IIBCRateRepository IBCRates { get; }

        IOrderShippingInfoRepository OrderShippingInfos { get; }
        ICustomerRepository Customers { get; }
        ITrackingOrderRepository TrackingOrders { get; }
        IReturnRequestRepository ReturnRequests { get; }
        IReturnRequestItemRepository ReturnRequestItems { get; }


        IBuyerRepository Buyers { get; }

        IBuyerBlackListRepository BuyerBlackLists { get; }
        IFeedbackBlackListRepository FeedbackBlackLists { get; }

        IItemOrderMappingRepository ItemOrderMappings { get; }
        IInventoryReportRepository InventoryReports { get; }
        IReportRepository SalesReports { get; }
        IDateRepository Dates { get; }
        IZipCodeRepository ZipCodes { get; }
        IZipCodeZoneRepository ZipCodeZones { get; }
        IDhlGroundZipCodeRepository DhlGroundZipCodes { get; }
        IDhlCAZipCodeZoneRepository DhlCAZipCodeZones { get; }
        IDhlGBZipCodeZoneRepository DhlGBZipCodeZones { get; }
        IWalmartBrandInfoRepository WalmartBrandInfoes { get; }
        IOrderReportRepository OrderReports { get;  }
        IShipmentReportRepository ShipmentReports { get; }

        ICountryRepository Countries { get; }
        IStateRepository States { get; }        

        IFeedRepository Feeds { get; }
        IFeedMessageRepository FeedMessages { get; }
        IFeedItemRepository FeedItems { get; }


        IEmailRepository Emails { get; }
        IEmailAttachmentRepository EmailAttachments { get; }
        IEmailToOrderRepository EmailToOrders { get; }

        ISettingRepository Settings { get; }
        IProxyInfoRepository ProxyInfos { get; }
        IPushRepository Pushes { get; }

        IQuickBookRepository QuickBooks { get; }

        //VENDOR ORDERS
        IVendorOrderRepository VendorOrders { get; }
        IVendorOrderItemRepository VendorOrderItems { get; }
        IVendorOrderItemSizeRepository VendorOrderItemSizes { get; }
        IVendorOrderAttachmentRepository VendorOrderAttachments { get; }

        //SALE EVENTS
        ISaleEventRepository SaleEvents { get; }
        ISaleEventFeedRepository SaleEventFeeds { get; }
        ISaleEventEntryRepository SaleEventEntries { get; }
        ISaleEventSizeHoldInfoRepository SaleEventSizeHoldInfoes { get; }

        //INVENTORY
        IStyleRepository Styles { get; }
        IStyleAdditionRepository StyleAdditions { get; }
        IStyleChangeHistoryRepository StyleChangeHistories { get; }

        IStyleImageRepository StyleImages { get; }
        IScannedRepository Scanned { get; }
        ISealedBoxRepository SealedBoxes { get; }
        IOpenBoxRepository OpenBoxes { get; }
        IOpenBoxItemRepository OpenBoxItems { get; }
        IOpenBoxTrackingRepository OpenBoxTrackings { get; }
        ISealedBoxItemRepository SealedBoxItems { get; }
        ISealedBoxTrackingRepository SealedBoxTrackings { get; }
        IStyleLocationRepository StyleLocations { get; }
        IStyleItemRepository StyleItems { get; }
        IStyleItemAttributeRepository StyleItemAttributes { get; }
        IStyleItemSaleRepository StyleItemSales { get; }
        IStyleItemSaleToListingRepository StyleItemSaleToListings { get; }
        IStyleItemSaleToMarketRepository StyleItemSaleToMarkets { get; }

        IVendorInvoiceRepository VendorInvoices { get; }

        IStyleGroupRepository StyleGroups { get; }
        IStyleToGroupRepository StyleToGroups { get; }
        IStyleReferenceRepository StyleReferences { get; }
        IStyleItemReferenceRepository StyleItemReferences { get; }
        IStyleItemBarcodeRepository StyleItemBarcodes { get; }
        IStyleItemQuantityHistoryRepository StyleItemQuantityHistories { get; }
        IStyleItemActionHistoryRepository StyleItemActionHistories { get; }
        IQuantityOperationRepository QuantityOperations { get; }
        IQuantityChangeRepository QuantityChanges { get; }

        IFBAPickListRepository FBAPickLists { get; }
        IFBAPickListEntryRepository FBAPickListEntries { get; }

        IPhotoshootPickListRepository PhotoshootPickLists { get; }
        IPhotoshootPickListEntryRepository PhotoshootPickListEntries { get; }

        ICustomBarcodeRepository CustomBarcodes { get; }
        
        ISizeMappingRepository SizeMappings { get; }
        IPackingSlipSizeMappingRepository PackingSlipSizeMappings { get; }
        ISkuMappingRepository SkuMappings { get; }
        IAmazonCategoryMappingRepository AmazonCategoryMappings { get; }

        IFeatureRepository Features { get; }
        IFeatureValueRepository FeatureValues { get; }
        IStyleFeatureValueRepository StyleFeatureValues { get; }
        IStyleFeatureTextValueRepository StyleFeatureTextValues { get; }
        ISizeRepository Sizes { get; }
        ISizeGroupRepository SizeGroups { get; }
        ISizeGroupToItemTypeRepository SizeGroupToItemTypes { get; }
        IItemTypeRepository ItemTypes { get; }

        //Inventory Counting
        ISealedBoxCountingRepository SealedBoxCountings { get; }
        IOpenBoxCountingRepository OpenBoxCountings { get; }
        IOpenBoxCountingItemRepository OpenBoxCountingItems { get; }
        ISealedBoxCountingItemRepository SealedBoxCountingItems { get; }


        IShippingMethodRepository ShippingMethods { get; }
        IShippingChargeRepository ShippingCharges { get; }

        IMailLabelInfoRepository MailLabelInfos { get; }
        IMailLabelItemRepository MailLabelItems { get; }
        ILabelRepository Labels { get; }
        ILabelPrintPackRepository LabelPrintPacks { get; }

        INodePositionRepository NodePositions { get; }
        IOrderBatchRepository OrderBatches { get; }
        IOrderToBatchRepository OrderToBatches { get; }

        ISyncHistoryRepository SyncHistory { get; }
        ISyncMessageRepository SyncMessages { get; }

        INotificationRepository Notifications { get; }

        IBuyBoxStatusRepository BuyBoxStatus { get; }
        IOfferChangeEventRepository OfferChangeEvents { get; }
        IOfferInfoRepository OfferInfoes { get; }
        IBuyBoxQuantityRepository BuyBoxQuantities { get; }

        //DropShippers
        IDropShipperRepository DropShippers { get; }
        IDSItemRepository DSItems { get; }
        IDSFileRepository DSFiles { get; }
        IDSFileLineRepository DSFileLines { get; }
        IDSFileMessageRepository DSFileMessages { get; }
        IDSFileLineMessageRepository DSFileLineMessages { get; }
        IStyleDSHistoryRepository StyleDSHistories { get; }

        //CustomFeeds
        ICustomFeedRepository CustomFeeds { get; }
        ICustomFeedScheduleRepository CustomFeedSchedules { get; }
        ICustomFeedFieldRepository CustomFeedFields { get; }


        IBulkEditOperationRepository BulkEditOperations { get; }
        IBulkEditHistoryRepository BulkEditHistories { get; }

        ICustomReportPredefinedFieldRepository CustomReportPredefinedFields { get; }
        ICustomReportFilterRepository CustomReportFilters { get; }
        ICustomReportRepository CustomReports { get; }        
        ICustomReportFieldRepository CustomReportFields { get; }

        ISkyPostalRateRepository SkyPostalRates { get; }
        ISkyPostalZoneRepository SkyPostalZones { get; }


        //Categories
        ICustomCategoryRepository CustomCategories { get; }
        ICustomCategoryFilterRepository CustomCategoryFilters { get; }
        ICustomCategoryToStyleRepository CustomCategoryToStyles { get; }

        //Chart
        IChartRepository Charts { get; }
        IChartPointRepository ChartPoints { get; }

        //Cache
        IStyleCacheRepository StyleCaches { get; }
        IStyleItemCacheRepository StyleItemCaches { get; }
        IParentItemCacheRepository ParentItemCaches { get; }
        IItemCacheRepository ItemCaches { get; }
        IListingCacheRepository ListingCaches { get; }

        //Audits
        IAuditRepository Audits { get; }

        void DisableProxyCreation();
        void DisableAutoDetectChanges();
        void DisableValidation();
        void EnableValidation();
        void Commit();
        void DetectChanges();
        void ReCreate();
        void CommitAndRefreshChanges();
        void RollbackChanges();
    }
}
