using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Categories;
using Amazon.Core.Entities.CustomReports;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Entities.Emails;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Events;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Feeds;
using Amazon.Core.Entities.General;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Entities.Messages;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Rates;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Entities.Users;
using Amazon.Core.Entities.VendorOrders;
using Amazon.Core.Views;
using Ccen.Core.Entities.TrackingNumbers;
using Z.EntityFramework.Plus;

namespace Amazon.DAL
{
    public class AmazonContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }
        public DbSet<ListingDefect> ItemDefects { get; set; }
        public DbSet<ItemAddition> ItemAdditions { get; set; }
        public DbSet<ItemChangeHistory> ItemChangeHistories { get; set; }

        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingFBAEstFee> ListingFBAEstFees { get; set; }
        public DbSet<ListingFBAInv> ListingFBAInvs { get; set; }
        public DbSet<ListingSizeIssue> ListingSizeIssues { get; set; }
        
        public DbSet<ParentItem> ParentItems { get; set; }
        public DbSet<ParentItemImage> ParentItemImages { get; set; }

        public DbSet<ProductComment> ProductComments { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<QuantityHistory> QuantityHistories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemSource> OrderItemSources { get; set; }
        public DbSet<OrderComment> OrderComments { get; set; }
        public DbSet<OrderNotify> OrderNotifies { get; set; }
        public DbSet<OrderEmailNotify> OrderEmailNotifies { get; set; }
        public DbSet<OrderSearch> OrderSearches { get; set; }
        public DbSet<OrderChangeHistory> OrderChangeHistories { get; set; }

        public DbSet<TrackingNumberStatus> TrackingNumberStatuses { get; set; }
        public DbSet<CustomTrackingNumber> CustomTrackingNumbers { get; set; }

        public DbSet<SystemMessage> SystemMessages { get; set; }

        public DbSet<ScanForm> ScanForms { get; set; }
        public DbSet<ScheduledPickup> ScheduledPickups { get; set; }
        public DbSet<DhlECommerceRate> DhlECommerceRates { get; set; }
        public DbSet<DhlInvoice> DhlInvoices { get; set; }
        public DbSet<DhlRateCode> DhlRateCodes { get; set; }
        public DbSet<DhlRateCodePrice> DhlRateCodePrices { get; set; }
        public DbSet<RateByCountry> RateByCountries { get; set; }
        public DbSet<SkyPostalCityCode> SkyPostalCityCodes { get; set; }
        public DbSet<SkyPostalZone> SkyPostalZones { get; set; }
        public DbSet<SkyPostalRate> SkyPostalRates { get; set; }

        public DbSet<IBCRate> IBCRates { get; set; }

        public DbSet<ReturnRequest> ReturnRequests { get; set; }
        public DbSet<ReturnRequestItem> ReturnRequestItems { get; set; }

        public DbSet<Buyer> Buyers { get; set; }

        public DbSet<BuyerBlackList> BuyerBlackLists { get; set; }
        public DbSet<FeedbackBlackList> FeedbackBlackLists { get; set; }

        public DbSet<OrderShippingInfo> OrderShippingInfos { get; set; } 
        public DbSet<ItemOrderMapping> ItemOrderMappings { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<InventoryReport> InventoryReports { get; set; }

        public DbSet<LabelPrintPack> LabelPrintPacks { get; set; }

        public DbSet<State> States { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Date> Dates { get; set; }
        public DbSet<ZipCode> ZipCodes { get; set; }
        public DbSet<Feed> Feeds { get; set; }
        public DbSet<FeedMessage> FeedMessages { get; set; }
        public DbSet<FeedItem> FeedItems { get; set; }

        public DbSet<ZipToTimezone> ZipToTimezones { get; set; }
        public DbSet<ZipCodeZone> ZipCodeZones { get; set; }
        public DbSet<DhlGroundZipCode> DhlGroundZipCodes { get; set; }
        public DbSet<DhlGBZipCodeZone> DhlGBZipCodeZones { get; set; }
        public DbSet<DhlCAZipCodeZone> DhlCAZipCodeZones { get; set; }
        public DbSet<WalmartBrandInfo> WalmartBrandInfoes { get; set; }

        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailAttachment> EmailAttachments { get; set; }
        public DbSet<EmailToOrder> EmailToOrders { get; set; }


        public DbSet<Marketplace> Marketplaces { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<EmailAccount> EmailAccounts { get; set; }
        public DbSet<SQSAccount> SQSAccounts { get; set; }
        public DbSet<ShipmentProvider> ShipmentProviders { get; set; }
        public DbSet<AddressProvider> AddressProviders { get; set; }
        public DbSet<CompanyAddress> CompanyAddresses { get; set; }


        public DbSet<SystemAction> SystemActions { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<ProxyInfo> ProxyInfos { get; set; }

        public DbSet<NodePosition> NodePositions { get; set; } 

        //INVENTORY
        public DbSet<Style> Styles { get; set; }
        public DbSet<StyleChangeHistory> StyleChangeHistories { get; set; }
        public DbSet<StyleImage> StyleImages { get; set; }
        public DbSet<SealedBox> SealedBoxes { get; set; }
        public DbSet<OpenBox> OpenBoxes { get; set; }
        public DbSet<OpenBoxItem> OpenBoxItems { get; set; }
        public DbSet<OpenBoxTracking> OpenBoxTrackings { get; set; }
        public DbSet<SealedBoxItem> SealedBoxItems { get; set; }
        public DbSet<SealedBoxTracking> SealedBoxTrackings { get; set; }

        public DbSet<VendorInvoice> VendorInvoices { get; set; }

        public DbSet<SealedBoxCounting> SealedBoxCountings { get; set; }
        public DbSet<OpenBoxCounting> OpenBoxCountings { get; set; }
        public DbSet<OpenBoxCountingItem> OpenBoxCountingItems { get; set; }
        public DbSet<SealedBoxCountingItem> SealedBoxCountingItems { get; set; }


        public DbSet<StyleLocation> StyleLocations { get; set; }
        public DbSet<StyleItem> StyleItems { get; set; }
        public DbSet<StyleItemAttribute> StyleItemAttributes { get; set; }

        public DbSet<StyleItemSale> StyleItemSales { get; set; }
        public DbSet<StyleItemSaleToListing> StyleItemSaleToListings { get; set; }
        public DbSet<StyleItemSaleToMarket> StyleItemSaleToMarkets { get; set; }

        public DbSet<StyleGroup> StyleGroups { get; set; }
        public DbSet<StyleToGroup> StyleToGroups { get; set; }
        public DbSet<StyleReference> StyleReferences { get; set; }
        public DbSet<StyleItemReference> StyleItemReferences { get; set; }

        public DbSet<StyleItemBarcode> StyleItemBarcodes { get; set; }
        public DbSet<StyleItemQuantityHistory> StyleItemQuantityHistories { get; set; }
        public DbSet<StyleItemActionHistory> StyleItemActionHistories { get; set; }

        public DbSet<QuantityOperation> QuantityOperations { get; set; }
        public DbSet<QuantityChange> QuantityChanges { get; set; }

        public DbSet<FBAPickList> FBAPickLists { get; set; }
        public DbSet<FBAPickListEntry> FBAPickListEntries { get; set; }

        public DbSet<PhotoshootPickList> PhotoshootPickLists { get; set; }
        public DbSet<PhotoshootPickListEntry> PhotoshootPickListEntries { get; set; }

        public DbSet<CustomBarcode> CustomBarcodes { get; set; }

        public DbSet<SizeMapping> SizeMappings { get; set; }
        public DbSet<PackingSlipSizeMapping> PackingSlipSizeMappings { get; set; }
        public DbSet<AmazonCategoryMapping> AmazonCategoryMappings { get; set; }        

        public DbSet<Size> Sizes { get; set; }
        public DbSet<SizeGroup> SizeGroups { get; set; }
        public DbSet<SizeGroupToItemType> SizeGroupToItemTypes { get; set; }

        public DbSet<ShippingMethod> ShippingMethods { get; set; }
        public DbSet<ShippingCharge> ShippingCharges { get; set; }

        public DbSet<MailLabelInfo> MailLabelInfoes { get; set; }
        public DbSet<MailLabelItem> MailLabelItems { get; set; }
        
        //FEATURES
        public DbSet<Feature> Features { get; set; }
        public DbSet<FeatureValue> FeatureValues { get; set; }
        public DbSet<StyleFeatureValue> StyleFeatureValues { get; set; }
        public DbSet<StyleFeatureTextValue> StyleFeatureTextValues { get; set; }
        public DbSet<FeatureToItemType> FeatureToItemTypes { get; set; }

        //ENUMS
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<ShippingSize> ShippingSizes { get; set; }

        public DbSet<USPSHoliday> USPSHolidays { get; set; }
        public DbSet<OrderBatch> OrderBatches { get; set; }
        public DbSet<OrderToBatch> OrderToBatches { get; set; }

        public DbSet<TrackingOrder> TrackingOrders { get; set; }

        public DbSet<SyncHistory> SyncHistory { get; set; }
        public DbSet<SyncMessage> SyncMessages { get; set; }

        public DbSet<Notification> NotificationMessages { get; set; }

        public DbSet<BuyBoxStatus> BuyBoxStatus { get; set; }
        public DbSet<OfferChangeEvent> OfferChangeEvents { get; set; }
        public DbSet<OfferInfo> OfferInfoes { get; set; }
        public DbSet<BuyBoxQuantity> BuyBoxQuantities { get; set; }

        //VENDORS
        public DbSet<VendorOrder> VendorOrders { get; set; }
        public DbSet<VendorOrderItem> VendorOrderItems { get; set; }
        public DbSet<VendorOrderItemSize> VendorOrderItemSizes { get; set; }
        public DbSet<VendorOrderAttachment> VendorOrderAttachments { get; set; }

        //SALE EVENTS
        public DbSet<SaleEvent> SaleEvents { get; set; }
        public DbSet<SaleEventEntry> SaleEventEntries { get; set; }
        public DbSet<SaleEventSizeHoldInfo> SaleEventSizeHoldInfoes { get; set; }
        public DbSet<SaleEventFeed> SaleEventFeeds { get; set; }

        //REPORTS
        public DbSet<CustomReport> CustomReports { get; set; }
        public DbSet<CustomReportField> CustomReportFields { get; set; }
        public DbSet<CustomReportPredefinedField> CustomReportPredefinedFields { get; set; }
        public DbSet<CustomReportFilter> CustomReportFilters { get; set; }
        


        //CACHES
        public DbSet<StyleCache> StyleCaches { get; set; }
        public DbSet<StyleItemCache> StyleItemCaches { get; set; }
        public DbSet<ParentItemCache> ParentItemCaches { get; set; }
        public DbSet<ListingCache> ListingCaches { get; set; }
        public DbSet<ItemCache> ItemCaches { get; set; }

        
        //VIEWS
        public DbSet<ViewStylesWithoutImage> ViewStyleWithoutImages { get; set; }
        public DbSet<ViewStyle> ViewStyles { get; set; }
        public DbSet<ViewParent> ViewParents { get; set; }
        public DbSet<ViewActualProductComment> ViewActualProductComments { get; set; }
        public DbSet<ViewActualOrderComment> ViewActualOrderComments { get; set; }
        public DbSet<ViewListParent> ViewListParents { get; set; }
        public DbSet<ViewItem> ViewItems { get; set; }

        public DbSet<ViewEmails> ViewEmails { get; set; }

        public DbSet<ViewStyleFeatureValue> ViewStyleFeatureValues { get; set; }
        
        public DbSet<ViewListing> ViewListings { get; set; }
        public DbSet<ViewListingSale> ViewListingSales { get; set; }
        public DbSet<ViewUnmaskedListing> ViewUnmaskedListings { get; set; }
        public DbSet<ViewOrderItem> ViewOrderItems { get; set; }

        public DbSet<ViewMarketsSoldByDateAndMarket> ViewMarketsSoldByDateAndMarkets { get; set; }
        public DbSet<ViewOrdersSoldByDateAndMarket> ViewOrdersSoldByDateAndMarkets { get; set; }
        public DbSet<ViewMarketsSoldByDateAndSKU> ViewMarketsSoldByDateAndSKU { get; set; }
        public DbSet<ViewMarketsSoldByDateAndItemStyle> ViewMarketsSoldByDateAndItemStyles { get; set; }

        public DbSet<ViewMarketsSoldQuantity> ViewMarketsSoldQuantities { get; set; }
        public DbSet<ViewMarketsSoldQuantityByStyleItem> ViewMarketsSoldQuantityByStyleItems { get; set; }
        public DbSet<ViewMarketsSoldQuantityIncludePreOrderByStyleItem> ViewMarketsSoldQuantityIncludePreOrderByStyleItems { get; set; }
        public DbSet<ViewMarketsSoldThisYearQuantityIncludePreOrderByStyleItem> ViewMarketsSoldThisYearQuantityIncludePreOrderByStyleItems { get; set; }

        public DbSet<ViewScannedSoldQuantity> ViewScannedSoldQuantities { get; set; }
        public DbSet<ViewSentToFBAQuantity> ViewFBASentQuantities { get; set; }
        public DbSet<ViewSpecialCaseQuantity> ViewSpecialCaseQuantities { get; set; }
        public DbSet<ViewInventoryQuantity> ViewInventoryQuantities { get; set; }
        public DbSet<ViewInventoryThisYearQuantity> ViewInventoryThisYearQuantities { get; set; }
        public DbSet<ViewInventoryOnHandQuantity> ViewInventoryOnHandQuantities { get; set; }        

        public DbSet<ViewSaleEventSoldByStyleItem> ViewSaleEventSoldByStyleItems { get; set; }
        public DbSet<ViewSaleEventHoldByStyleItem> ViewSaleEventHoldByStyleItems { get; set; }
        public DbSet<ViewPhotoshootHoldedByStyleItem> ViewPhotoshootHoldedByStyleItems { get; set; }

        public DbSet<ViewLastEmailByOrder> ViewLastEmailByOrders { get; set; }

        public DbSet<ViewBatch> ViewBatches { get; set; }

        public DbSet<ViewBuyerBlackList> ViewBuyerBlackLists { get; set; }
        public DbSet<ViewFeedbackBlackList> ViewFeedbackBlackLists { get; set; }


        public DbSet<ViewScanOrder> ViewScanOrders { get; set; }
        public DbSet<ViewScanItem> ViewScanItems { get; set; }
        public DbSet<ViewScanItemOrderMapping> ViewScanItemOrderMappings { get; set; }

        public DbSet<ViewLabel> ViewLabels { get; set; }
        public DbSet<ViewLastLabel> ViewLastLabels { get; set; }

        public DbSet<ViewOrderReport> ViewOrderReports { get; set; }
        public DbSet<ViewShipmentReport> ViewShipmentReports { get; set; }


        public DbSet<Push> Pushes { get; set; }

        //Charts
        public DbSet<Chart> Charts { get; set; }
        public DbSet<ChartPoint> ChartPoints { get; set; }


        //Audit
        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<AuditEntryProperty> AuditEntryProperties { get; set; }

        //DropShippers
        public DbSet<DropShipper> DropShippers { get; set; }

        //Custom Feeds
        public DbSet<CustomFeed> CustomFeeds { get; set; }
        public DbSet<CustomFeedField> CustomFeedFields { get; set; }
        public DbSet<CustomFeedSchedule> CustomFeedSchedules { get; set; }


        //Bulk edit
        public DbSet<Amazon.Core.Entities.BulkEdits.BulkEditOperation> BulkEditOperations { get; set; }
        public DbSet<Amazon.Core.Entities.BulkEdits.BulkEditHistory> BulkEditHistories { get; set; }

        //Categories
        public DbSet<CustomCategory> CustomCategories { get; set; }
        public DbSet<CustomCategoryFilter> CustomCategoryFilters { get; set; }
        public DbSet<CustomCategoryToStyle> CustomCategoryToStyles { get; set; }



        public override int SaveChanges()
        {
            //https://github.com/zzzprojects/EntityFramework-Plus/wiki/EF-Audit-%7C-Entity-Framework-Audit-Trail-Context-and-Track-Changes
            //var audit = new Audit();
            //audit.PreSaveChanges(this);
            var rowAffecteds = base.SaveChanges();
            //audit.PostSaveChanges();
            
            //this.AuditEntries.AddRange(audit.Entries);
            //base.SaveChanges();

            return rowAffecteds;
        }

        //public override int SaveChanges()
        //{
        //    return SaveChanges(null, _time.GetAppNowTime());
        //}

        //public int SaveChanges(Guid? changeBy, DateTime? changeWhen)
        //{
        //    try
        //    {
        //        changeWhen = changeWhen ?? _time.GetAppNowTime();

        //        var addedAuditedEntities = ChangeTracker.Entries<IAuditedEntity>()
        //            .Where(p => p.State == EntityState.Added)
        //            .Select(p => p.Entity);

        //        var modifiedAuditedEntities = ChangeTracker.Entries<IAuditedEntity>()
        //          .Where(p => p.State == EntityState.Modified)
        //          .Select(p => p.Entity);

        //        //var deletedAuditedEntities = ChangeTracker.Entries<IAuditedEntity>()
        //        //    .Where(p => p.State == EntityState.Deleted)
        //        //    .Select(p => p.Entity);

        //        var now = DateTime.UtcNow;

        //        foreach (var added in addedAuditedEntities)
        //        {

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Fatal("SaveChanges()", ex);
        //    }
        //    return base.SaveChanges();
        //}


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditEntry>().HasKey(a => a.AuditEntryID);
            //modelBuilder.Entity<ViewItemsWithRemoved>().ToTable("ViewItemsWithRemoved");  

            //modelBuilder.Properties<string>().Configure(conf => conf.HasMaxLength(255));
            //modelBuilder.Properties<string>().Where(x => x.Name == "ASIN" || x.Name == "ParentASIN").Configure(conf => conf.HasMaxLength(50));
            //modelBuilder.Properties<string>().Where(x => x.Name == "StyleId").Configure(conf => conf.HasMaxLength(50));
            //modelBuilder.Entity<User>().Property(p => p.Balance).HasPrecision(18, 4);

            //modelBuilder.Entity<User>()
            //    .HasMany(u => u.Roles)
            //    .WithMany()
            //    .Map(x =>
            //    {
            //        x.MapLeftKey("UserId");
            //        x.MapRightKey("RoleId");
            //        x.ToTable("UserRoleMapping");
            //    });

            //modelBuilder.Entity<ViewSalesPerformance>().HasKey(p => new { p.OrderId, p.ShippingId, p.ItemId });
            
            ////INVENTORY
            //modelBuilder.Entity<OpenBoxItem>()
            //    .HasRequired(v => v.OpenBox)
            //    .WithMany(c => c.OpenBoxItems)
            //    .HasForeignKey(v => v.BoxId)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<SealedBoxItem>()
            //    .HasRequired(v => v.SealedBox)
            //    .WithMany(c => c.SealedBoxItems)
            //    .HasForeignKey(v => v.BoxId)
            //    .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}
