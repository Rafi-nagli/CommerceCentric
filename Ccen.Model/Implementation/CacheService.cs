using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Caches;
using Amazon.DTO.Inventory;
using Amazon.Model.Implementation.Markets;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation
{
    public class CacheService : ICacheService
    {
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;
        private IQuantityManager _quantityManager;

        public CacheService(ILogService log, 
            ITime time,
            ISystemActionService actionService,
            IQuantityManager quantityManager)
        {
            _log = log;
            _time = time;
            _actionService = actionService;
            _quantityManager = quantityManager;
        }
        

        #region Cache Change Events

        public void RequestItemIdUpdates(IUnitOfWork db,
            IList<long> itemIdList,
            long? by)
        {
            _actionService.AddAction(db,
                SystemActionType.UpdateCache,
                null,
                new UpdateCacheInput()
                {
                    ItemIdList = itemIdList,
                },
                null,
                by);
        }

        public void RequestParentItemIdUpdates(IUnitOfWork db,
            IList<long> parentItemIdList,
            UpdateCacheMode updateMode,
            long? by)
        {
            _actionService.AddAction(db,
                SystemActionType.UpdateCache,
                null,
                new UpdateCacheInput()
                {
                    UpdateMode = updateMode,
                    ParentIdList = parentItemIdList,
                },
                null,
                by);
        }


        public void RequestStyleItemIdUpdates(IUnitOfWork db, 
            IList<long> styleItemIdList, 
            long? by)
        {
            _actionService.AddAction(db,
                SystemActionType.UpdateCache,
                null,
                new UpdateCacheInput()
                {
                    StyleItemIdList = styleItemIdList
                },
                null,
                by);
        }

        public void RequestStyleIdUpdates(IUnitOfWork db,
            IList<long> styleIdList, 
            UpdateCacheMode updateMode,
            long? by)
        {
            _actionService.AddAction(db,
                SystemActionType.UpdateCache,
                null,
                new UpdateCacheInput()
                {
                    StyleIdList = styleIdList,
                    UpdateMode = updateMode
                },
                null,
                by);
        }

        #endregion

        public void ProcessUpdateCacheActions(IUnitOfWork db)
        {
            var updateCacheActions = _actionService.GetUnprocessedByType(db, SystemActionType.UpdateCache, null, null);
            foreach (var action in updateCacheActions)
            {
                var input = JsonConvert.DeserializeObject<UpdateCacheInput>(action.InputData);
                
                if (input.StyleIdList != null && input.StyleIdList.Any())
                    UpdateStyleCacheForStyleId(db, input.StyleIdList);
                if (input.StyleIdList != null && input.StyleIdList.Any() && input.UpdateMode == UpdateCacheMode.IncludeChild)
                    UpdateStyleItemCacheForStyleId(db, input.StyleIdList);
                if (input.StyleItemIdList != null && input.StyleItemIdList.Any())
                    UpdateStyleItemCacheForStyleItemId(db, input.StyleItemIdList);

                _actionService.SetResult(db, action.Id, SystemActionStatus.Done, null);

                _log.Debug(String.Format("Update cache action was done, styleIdList={0}, styleItemIdList={1}, updateMode={2}",
                    input.StyleIdList != null ? String.Join(", ", input.StyleIdList) : "-",
                    input.StyleItemIdList != null ? String.Join(", ", input.StyleItemIdList) : "-",
                    input.UpdateMode));
            }
            db.Commit();
        }

        public bool UpdateDbCacheUsingSettings(IUnitOfWork db, ISettingsService settings)
        {
            UpdateDbCache(db);

            _log.Info("Cache was updated");

            return true;
        }

        public void UpdateDbCache(IUnitOfWork db)
        {
            var styleCacheItems = ComposeStyleCaches(db);
            db.StyleCaches.RefreshCacheItems(styleCacheItems, _time.GetAppNowTime());

            _log.Info("After compose style caches");

            var styleItemCacheItems = ComposeStyleItemCache(db);
            var updateResults = db.StyleItemCaches.RefreshCacheItems(styleItemCacheItems, _time.GetAppNowTime());
            ProcessStyleItemUpdateResults(db,
                _quantityManager,
                updateResults,
                _time.GetAppNowTime());

            _log.Info("After compose style item cache");

            var parentItemCacheItems = ComposeParentItemCache(db);
            db.ParentItemCaches.RefreshCacheItems(parentItemCacheItems, _time.GetAppNowTime());

            _log.Info("After compose parent item cache");
            
            //var itemCacheItems = ComposeItemCache(db);
            //db.ItemCaches.RefreshCacheItems(itemCacheItems, _time.GetAmazonNowTime());

            //_log.Info("After compose item cache");

            var listingCacheItems = ComposeListingCache(db);
            db.ListingCaches.RefreshCacheItems(listingCacheItems, _time.GetAppNowTime());

            _log.Info("After compose listing cache");

            UpdateProcessedRefunds(db);

            _log.Info("After update processed refunds amount");
        }

        private class RefundAmount
        {
            public string OrderItemId { get; set; }
            public decimal RefundItemPrice { get; set; }
            public decimal RefundShippingPrice { get; set; }
            public int RefundQuantity { get; set; }
        }

        public void UpdateProcessedRefunds(IUnitOfWork db, bool fullUpdate = false)
        {
            var refunds = new List<SystemActionDTO>();

            if (fullUpdate)
            {
                refunds = db.SystemActions.GetAllAsDto().Where(a => a.Type == (int) SystemActionType.UpdateOnMarketReturnOrder
                                                              && a.Status == (int) SystemActionStatus.Done).ToList();
            }
            else
            {
                var fromDate = _time.GetAppNowTime().AddDays(-7);
                var lastRefundOrderIdList = db.SystemActions.GetAllAsDto().Where(a => a.Type == (int) SystemActionType.UpdateOnMarketReturnOrder
                                                              && a.Status == (int) SystemActionStatus.Done
                                                              && a.CreateDate > fromDate)
                                                              .Select(a => a.Tag).ToList();

                refunds = db.SystemActions.GetAllAsDto().Where(a => a.Type == (int) SystemActionType.UpdateOnMarketReturnOrder
                                                              && a.Status == (int) SystemActionStatus.Done
                                                              && lastRefundOrderIdList.Contains(a.Tag)).ToList();

            }
            var refundsByOrderItems = new List<RefundAmount>();
            foreach (var refund in refunds)
            {
                var data = JsonConvert.DeserializeObject<ReturnOrderInput>(refund.InputData);
                if (data.RefundAmount.HasValue 
                    && data.RefundAmount.Value > 0
                    && data.Items == null)
                {
                    var orderItems = db.OrderItems.GetAll().Where(oi => oi.OrderId == data.OrderId).ToList();
                    var totalQty = orderItems.Sum(i => i.QuantityOrdered);
                    if (totalQty > 0)
                    {
                        foreach (var item in orderItems)
                        {
                            refundsByOrderItems.Add(new RefundAmount()
                            {
                                OrderItemId = item.ItemOrderIdentifier,
                                RefundItemPrice = PriceHelper.RoundToTwoPrecision(data.RefundAmount / totalQty) ?? 0,
                                RefundQuantity = item.QuantityOrdered,
                            });
                        }
                    }
                }
                else
                {
                    foreach (var item in data.Items)
                    {
                        var refundItemPrice = item.RefundItemPrice;
                        if (data.DeductShipping)
                            refundItemPrice -= item.DeductShippingPrice;
                        if (data.IsDeductPrepaidLabelCost)
                            refundItemPrice -= item.DeductPrepaidLabelCost;
                        if (refundItemPrice < 0)
                            refundItemPrice = 0;
                        var refundShippingPrice = data.IncludeShipping ? item.RefundShippingPrice : 0;

                        refundsByOrderItems.Add(new RefundAmount()
                        {
                            OrderItemId = item.ItemOrderId,
                            RefundQuantity = item.Quantity,
                            RefundItemPrice = refundItemPrice,
                            RefundShippingPrice = refundShippingPrice,
                        });
                    }
                }
            }

            var orderItemIdList = refundsByOrderItems.Select(r => r.OrderItemId).Distinct();
            var dbOrderItems = db.OrderItems.GetAll().Where(oi => orderItemIdList.Contains(oi.ItemOrderIdentifier)).ToList();
            foreach (var dbOrderItem in dbOrderItems)
            {
                var orderItemRefunds = refundsByOrderItems.Where(oi => oi.OrderItemId == dbOrderItem.ItemOrderIdentifier).ToList();
                dbOrderItem.RefundItemPrice = orderItemRefunds.Sum(i => i.RefundItemPrice);
                dbOrderItem.RefundItemPriceInUSD = PriceHelper.RougeConvertToUSD(dbOrderItem.ItemPriceCurrency, dbOrderItem.RefundItemPrice);
                dbOrderItem.RefundShippingPrice = orderItemRefunds.Sum(i => i.RefundShippingPrice);
                dbOrderItem.RefundShippingPriceInUSD = PriceHelper.RougeConvertToUSD(dbOrderItem.ShippingPriceCurrency, dbOrderItem.RefundShippingPrice);
                dbOrderItem.QuantityRefunded = orderItemRefunds.Sum(i => i.RefundQuantity);
            }
            db.Commit();
        }


        #region Style Cache

        public void UpdateStyleCacheForStyleId(IUnitOfWork db, IList<long> styleIdList)
        {
            var styles = styleIdList.Select(s => new StyleEntireDto()
            {
                Id = s
            }).ToList();

            var stylesValues = db.StyleFeatureValues.GetAllFeatureValuesByStyleIdAsDto(styleIdList);
            var stylesTextValues = db.StyleFeatureTextValues.GetAllFeatureTextValuesByStyleIdAsDto(styleIdList);
            var itemsDates = db.ItemOrderMappings.GetLastShippedDateForItem().Where(i => i.StyleId.HasValue && styleIdList.Contains(i.StyleId.Value)).ToList();

            var itemsQuery = from i in db.Items.GetAllViewAsDto()
                join p in db.ParentItems.GetAll()
                    on new {ASIN = i.ParentASIN, i.Market, i.MarketplaceId} equals new {p.ASIN, p.Market, p.MarketplaceId}
                where i.StyleId.HasValue 
                    && styleIdList.Contains(i.StyleId.Value)
                select new ItemInfoForStyleCache
                {
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId,
                    StyleId = i.StyleId,
                    
                    ASIN = i.ASIN,
                    ParentASIN = i.ParentASIN,
                    SourceMarketId = i.SourceMarketId,

                    Rank = p.Rank,
                    RealQuantity = i.RealQuantity,

                    ItemPublishedStatus = i.PublishedStatus,
                };
            var styleListings = itemsQuery.ToList();

            var styleCaches = BuildStyleCacheFor(styles, styleListings, stylesValues, stylesTextValues, itemsDates);
            
            foreach (var styleCache in styleCaches)
                db.StyleCaches.UpdateCacheItem(styleCache);
        }

        private class ItemInfoForStyleCache
        {
            public long? StyleId { get; set; }
            public int Market { get; set; }
            public string MarketplaceId { get; set; }
            
            public bool IsFBA { get; set; }

            public string ASIN { get; set; }
            public string ParentASIN { get; set; }
            public string SourceMarketId { get; set; }

            public decimal? Rank { get; set; }
            public int? RealQuantity { get; set; }

            public int? ItemPublishedStatus { get; set; }
        }

        private IList<StyleCacheDTO> ComposeStyleCaches(IUnitOfWork db)
        {
            var styles = db.Styles.GetAllAsDto().ToList();
            var stylesValues = db.StyleFeatureValues.GetFeatureValueForAllStyleByFeatureId(new int[] { 
                StyleFeatureHelper.MAIN_LICENSE,
                StyleFeatureHelper.SUB_LICENSE1,
                StyleFeatureHelper.GENDER,
                StyleFeatureHelper.SHIPPING_SIZE,
                StyleFeatureHelper.INTERNATIONAL_PACKAGE,
                StyleFeatureHelper.ITEMSTYLE,
                StyleFeatureHelper.EXCESSIVE_SHIPMENT,
                StyleFeatureHelper.HOLIDAY,
            });

            var stylesTextValues = db.StyleFeatureTextValues.GetFeatureTextValueForAllStyleByFeatureId(new int[]
            {
                StyleFeatureHelper.EXCESSIVE_SHIPMENT
            });

            var itemsDates = db.ItemOrderMappings.GetLastShippedDateForItem().ToList();

            var itemsQuery = from i in db.Items.GetAllViewAsDto()
                             join p in db.ParentItems.GetAll()
                             on new {ASIN = i.ParentASIN, i.Market, i.MarketplaceId} equals new {p.ASIN, p.Market, p.MarketplaceId}
                select new ItemInfoForStyleCache
                {
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId,
                    StyleId = i.StyleId,
                    
                    IsFBA = i.IsFBA,
                    ASIN = i.ASIN,
                    ParentASIN = i.ParentASIN,
                    SourceMarketId = i.SourceMarketId,

                    Rank = p.Rank,
                    RealQuantity = i.RealQuantity,

                    ItemPublishedStatus = i.PublishedStatus,
                };
            var styleListings = itemsQuery.ToList();
            
            return BuildStyleCacheFor(styles, styleListings, stylesValues, stylesTextValues, itemsDates);
        }

        private class MarketplaceListingInfo
        {
            public int Count { get; set; }
            public string MarketplaceId { get; set; }
            public int Market { get; set; }

            public long? StyleId { get; set; }
            public long? StyleItemId { get; set; }

            public decimal CurrentPrice { get; set; }
            public decimal? SalePrice { get; set; }
            public DateTime? SaleStartDate { get; set; }
            public int? PiecesSoldOnSale { get; set; }
            public int? MaxPiecesOnSale { get; set; }
            public int? OnSaleListingCount { get; set; }

            public int? ItemPublishedStatus { get; set; }

            public string SourceMarketId { get; set; }
            public string ASIN { get; set; }
        }

        private IList<StyleCacheDTO> BuildStyleCacheFor(IList<StyleEntireDto> styles,
            IList<ItemInfoForStyleCache> styleListings,
            IList<StyleFeatureValueDTO> stylesValues,
            IList<StyleFeatureValueDTO> stylesTextValues,
            IList<ItemDTO> itemMappings)
        {
            var styleCaches = new List<StyleCacheDTO>();
            
            foreach (var style in styles)
            {
                var mainLicense = stylesValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.MAIN_LICENSE);
                var subLicense = stylesValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.SUB_LICENSE1);
                var gender = stylesValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.GENDER);
                var itemStyle = stylesValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.ITEMSTYLE);
                var shippingSize = stylesValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.SHIPPING_SIZE);
                var internationalPackage = stylesValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.INTERNATIONAL_PACKAGE);
                var holiday = stylesValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.HOLIDAY);
                var excessiveShipment = stylesTextValues.FirstOrDefault(s => s.StyleId == style.Id && s.FeatureId == StyleFeatureHelper.EXCESSIVE_SHIPMENT);

                var lastItemDate = itemMappings
                    .Where(i => i.StyleId == style.Id)
                    .OrderByDescending(i => i.LastSoldDate)
                    .FirstOrDefault();

                DateTime? lastSoldDate = lastItemDate != null ? lastItemDate.LastSoldDate : null;

                IList<MarketplaceListingInfo> marketplaces = new List<MarketplaceListingInfo>();
                var listings = styleListings
                    .Where(l =>
                        !l.ItemPublishedStatus.HasValue ||
                        l.ItemPublishedStatus == (int)PublishedStatuses.Published ||
                        l.ItemPublishedStatus == (int)PublishedStatuses.HasChanges ||
                        l.ItemPublishedStatus == (int)PublishedStatuses.None)
                    .Where(l => l.StyleId == style.Id)                    
                    .ToList()
                    .OrderByDescending(l => String.IsNullOrEmpty(l.SourceMarketId) ? 0 : 1)
                    .ToList();
                foreach (var listing in listings)
                {
                    var existInfo = marketplaces.FirstOrDefault(m => m.Market == listing.Market
                        && m.MarketplaceId == listing.MarketplaceId);
                    if (existInfo == null)
                    {
                        marketplaces.Add(new MarketplaceListingInfo()
                        {
                            Count = 1,
                            Market = listing.Market,
                            MarketplaceId = listing.MarketplaceId,
                            SourceMarketId = listing.SourceMarketId,
                            ASIN = listing.ASIN,
                        });
                    }
                    else
                    {
                        existInfo.Count ++;
                    }
                }

                var mainListing = listings
                    .Where(l => !l.IsFBA)
                    .OrderBy(l => MarketHelper.GetMarketIndex((MarketType)l.Market, l.MarketplaceId))
                    .ThenBy(l => l.Rank ?? RankHelper.DefaultRank)
                    .ThenByDescending(l => l.RealQuantity)
                    .FirstOrDefault();

                ////var marketplaceCacheInfoes = marketplaces
                ////    .Where(i => i.Count > 0)
                ////    .OrderBy(m => MarketHelper.GetMarketIndex((MarketType)m.Market, m.MarketplaceId))
                ////    .Select(i => new MarketplaceCacheInfo()
                ////    {
                ////        ASIN = i.ASIN,
                ////        SourceMarketId = i.SourceMarketId,
                ////        ListingsCount = i.Count,
                ////        MarketName = MarketHelper.GetShortName(i.Market, i.MarketplaceId),                        
                ////    }).ToList();

                //var marketplacesInfoString = 
                //    String.Join(";",
                //                            marketplaces
                //                                .Where(m => m.Count > 0)
                //                                .OrderBy(m => MarketHelper.GetMarketIndex((MarketType)m.Market, m.MarketplaceId))
                //                                .Select(m => MarketHelper.GetShortName(m.Market, m.MarketplaceId) + ":" + m.Count)
                //                                .ToList());

                styleCaches.Add(new StyleCacheDTO()
                {
                    Id = style.Id,
                    MainLicense = mainLicense != null ? mainLicense.FeatureValueId.ToString() : String.Empty,
                    SubLicense = subLicense != null ? subLicense.FeatureValueId.ToString() : String.Empty,
                    Gender = gender != null ? gender.FeatureValueId.ToString() : String.Empty,
                    ItemStyle = itemStyle != null ? itemStyle.Value : String.Empty,
                    ShippingSizeValue = shippingSize != null ? shippingSize.Value : String.Empty,
                    InternationalPackageValue = internationalPackage != null ? internationalPackage.Value : String.Empty,
                    HolidayValue = holiday != null ? holiday.Value : String.Empty,
                    ExcessiveShipmentValue = excessiveShipment != null ? excessiveShipment.Value : String.Empty,
                    
                    LastSoldDateOnMarket = lastSoldDate,
                    //MarketplacesInfo = marketplacesInfoString, // JsonConvert.SerializeObject(marketplaceCacheInfoes),

                    AssociatedASIN = mainListing != null ? (String.IsNullOrEmpty(mainListing.ParentASIN) ? mainListing.ASIN : mainListing.ParentASIN) : null,
                    AssociatedSourceMarketId = mainListing != null ? mainListing.SourceMarketId : null,
                    AssociatedMarketplaceId = mainListing != null ? mainListing.MarketplaceId : null,
                    AssociatedMarket = mainListing != null ? mainListing.Market : (int?)null,

                    UpdateDate = _time.GetUtcTime()
                });
            }

            return styleCaches;
        }

        #endregion

        private IList<ListingCacheDTO> ComposeListingCache(IUnitOfWork db)
        {
            var qtyQuery = from soldQ in db.Items.GetMarketsSoldQuantityByListing()
                           join l in db.Listings.GetAll() on soldQ.ListingId equals l.Id
                group soldQ by soldQ.ListingId
                into groupedByListingId
                select new 
                {
                    Id = groupedByListingId.Key ?? 0,
                    SoldQuantity = groupedByListingId.Sum(s => s.SoldQuantity ?? 0),
                    TotalSoldQuantity = groupedByListingId.Sum(s => s.TotalSoldQuantity ?? 0),
                    MaxOrderDate = groupedByListingId.Max(s => s.MaxOrderDate)
                };

            var query = from c in qtyQuery
                join l in db.Listings.GetAll() on c.Id equals l.Id
                join i in db.Items.GetAll() on l.ItemId equals i.Id
                select new ListingCacheDTO()
                {
                    Id = c.Id,
                    ItemId = i.Id,
                    SoldQuantity = c.TotalSoldQuantity, //NOTE: Using total sold for lisings, don't interest sold from date
                    MaxOrderDate = c.MaxOrderDate,
                };

            return query.ToList();
        }


        #region Style Item Cache
        
        public void UpdateStyleItemCacheForStyleId(IUnitOfWork db, IList<long> styleIdList)
        {
            var inventoryQty = db.StyleItems.GetInventoryQuantities().Where(si =>si.StyleId.HasValue && styleIdList.Contains(si.StyleId.Value)).ToList();
            var sentInStore = db.Scanned.GetSentInStoreQuantities().Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value)).ToList();
            var sentToFBA = db.Scanned.GetSentToFBAQuantities().Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value)).ToList();
            var soldMarkets = db.Items.GetMarketsSoldQuantityByStyleItem().Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value)).ToList();
            var specialCases = db.QuantityOperations.GetSpecialCaseQuantities().Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value)).ToList();
            var inVirtualStyleStyleItemIds = (from sif in db.StyleItemReferences.GetAll()
                                    join st in db.Styles.GetAll() on sif.StyleId equals st.Id
                                    join si in db.StyleItems.GetAll() on sif.LinkedStyleItemId equals si.Id
                                    where styleIdList.Contains(si.StyleId)
                                     && !st.Deleted
                                    select sif.LinkedStyleItemId).Distinct().ToList();
            var sentToPhotoshoots = db.PhotoshootPickListEntries.GetHoldedQuantitiesByStyleItem().Where(s => s.StyleId.HasValue && styleIdList.Contains(s.StyleId.Value)).ToList();

            var styleItemListings = db.Items.GetAllViewAsDto()
                .Select(i => new MarketplaceListingInfo()
                {
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId,
                    Count = 1,
                    StyleId = i.StyleId,
                    StyleItemId = i.StyleItemId,
                    CurrentPrice = i.CurrentPrice,
                    SalePrice = i.SalePrice,
                    SaleStartDate = i.SaleStartDate,
                    MaxPiecesOnSale = i.MaxPiecesOnSale,
                    PiecesSoldOnSale = i.PiecesSoldOnSale,
                    ItemPublishedStatus = i.PublishedStatus,
                })
                .Where(i => i.StyleItemId.HasValue && i.StyleId.HasValue && styleIdList.Contains(i.StyleId.Value))
                .ToList();

            var results = BuildStyleItemCachesFor(inventoryQty, 
                soldMarkets, 
                sentInStore, 
                sentToFBA, 
                specialCases,
                styleItemListings,
                inVirtualStyleStyleItemIds,
                sentToPhotoshoots);

            try
            {
                var updateResults = new List<EntityUpdateStatus<long>>();
                foreach (var result in results)
                {
                    var updateResult = db.StyleItemCaches.UpdateCacheItem(result);
                    updateResults.Add(updateResult);
                }

                ProcessStyleItemUpdateResults(db,
                    _quantityManager,
                    updateResults,
                    _time.GetAppNowTime());
            }
            catch (Exception ex)
            {
                _log.Error("Update Style cache, styleId=" + String.Join(", ", styleIdList), ex);
            }
        }

        public void UpdateStyleItemCacheForStyleItemId(IUnitOfWork db, IList<long> styleItemIdList)
        {
            var styleItemIdNullList = styleItemIdList.Select(s => (long?) s).ToList();

            var inventoryQty = db.StyleItems.GetInventoryQuantities().Where(si => styleItemIdNullList.Contains(si.StyleItemId)).ToList();
            var sentInStore = db.Scanned.GetSentInStoreQuantities().Where(s => styleItemIdNullList.Contains(s.StyleItemId)).ToList();
            var sentToFBA = db.Scanned.GetSentToFBAQuantities().Where(s => styleItemIdNullList.Contains(s.StyleItemId)).ToList();
            var soldMarkets = db.Items.GetMarketsSoldQuantityByStyleItem().Where(s => styleItemIdNullList.Contains(s.StyleItemId)).ToList();
            var specialCases = db.QuantityOperations.GetSpecialCaseQuantities().Where(s => styleItemIdNullList.Contains(s.StyleItemId)).ToList();
            var inVirtualStyleStyleItemIds = (from sif in db.StyleItemReferences.GetAll()
                                              join st in db.Styles.GetAll() on sif.StyleId equals st.Id
                                              where styleItemIdList.Contains(sif.LinkedStyleItemId)
                                               && !st.Deleted
                                              select sif.LinkedStyleItemId).Distinct().ToList();

            var sentToPhotoshoots = db.PhotoshootPickListEntries.GetHoldedQuantitiesByStyleItem().Where(s => styleItemIdNullList.Contains(s.StyleItemId)).ToList();

            var styleItemListings = db.Items.GetAllViewAsDto()
                .Select(i => new MarketplaceListingInfo()
                {
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId,
                    Count = 1,
                    StyleId = i.StyleId,
                    StyleItemId = i.StyleItemId,
                    CurrentPrice = i.CurrentPrice,
                    SalePrice = i.SalePrice,
                    SaleStartDate = i.SaleStartDate,
                    MaxPiecesOnSale = i.MaxPiecesOnSale,
                    PiecesSoldOnSale = i.PiecesSoldOnSale,
                    ItemPublishedStatus = i.PublishedStatus,
                })
                .Where(i => i.StyleItemId.HasValue && styleItemIdNullList.Contains(i.StyleItemId))
                .ToList();

            var results = BuildStyleItemCachesFor(inventoryQty, 
                soldMarkets, 
                sentInStore, 
                sentToFBA, 
                specialCases,
                styleItemListings,
                inVirtualStyleStyleItemIds,
                sentToPhotoshoots);

            var updateResults = new List<EntityUpdateStatus<long>>();
            foreach (var result in results)
                updateResults.Add(db.StyleItemCaches.UpdateCacheItem(result));

            ProcessStyleItemUpdateResults(db,
                    _quantityManager,
                    updateResults,
                    _time.GetAppNowTime());
        }

        private IList<StyleItemCacheDTO> ComposeStyleItemCache(IUnitOfWork db)
        {
            var inventoryAll = db.StyleItems.GetInventoryQuantities().ToList();
            var sentInStoreAll = db.Scanned.GetSentInStoreQuantities().ToList();
            var sentToFBAAll = db.Scanned.GetSentToFBAQuantities().ToList();
            var soldMarketsAll = db.Items.GetMarketsSoldQuantityByStyleItem().ToList();
            var specialCaseAll = db.QuantityOperations.GetSpecialCaseQuantities().ToList();
            var inVirtualStyleStyleItemIds = (from sif in db.StyleItemReferences.GetAll()
                                              join st in db.Styles.GetAll() on sif.StyleId equals st.Id
                                              where !st.Deleted
                                              select sif.LinkedStyleItemId).Distinct().ToList();
            var sendToPhotoshootAll = db.PhotoshootPickListEntries.GetHoldedQuantitiesByStyleItem().ToList();

            var styleItemListings = db.Items.GetAllViewAsDto()
                .Select(i => new MarketplaceListingInfo()
                {
                    Market = i.Market,
                    MarketplaceId = i.MarketplaceId,
                    Count = 1,
                    StyleId = i.StyleId,
                    StyleItemId = i.StyleItemId,
                    CurrentPrice = i.CurrentPrice,
                    SalePrice = i.SalePrice,
                    SaleStartDate = i.SaleStartDate,
                    MaxPiecesOnSale = i.MaxPiecesOnSale,
                    PiecesSoldOnSale = i.PiecesSoldOnSale,
                    ItemPublishedStatus = i.PublishedStatus,
                })
                .Where(i => i.StyleItemId.HasValue)
                .ToList();

            var results = BuildStyleItemCachesFor(inventoryAll, 
                soldMarketsAll, 
                sentInStoreAll, 
                sentToFBAAll, 
                specialCaseAll,
                styleItemListings,
                inVirtualStyleStyleItemIds,
                sendToPhotoshootAll);

            return results;
        }

        private IList<StyleItemCacheDTO> BuildStyleItemCachesFor(IList<SoldSizeInfo> inventoryItems,
            IList<SoldSizeInfo> marketsSoldItems,
            IList<SoldSizeInfo> sentInStoreItems,
            IList<SoldSizeInfo> sentToFBAItems,
            IList<SoldSizeInfo> specialCaseItems,
            IList<MarketplaceListingInfo> styleItemListings,
            IList<long> inVirtualStyleStyleItemIds,
            IList<SoldSizeInfo> sendToPhotoshootItems)
        {
            IList<StyleItemCacheDTO> results = new List<StyleItemCacheDTO>();
            var today = _time.GetAppNowTime().Date;
            var nowUtc = _time.GetUtcTime();
            foreach (var size in inventoryItems)
            {
                if (!size.StyleItemId.HasValue)
                    throw new ArgumentNullException("StyleItemId", "Into remaining quantity list");
                if (!size.StyleId.HasValue)
                    throw new ArgumentNullException("StyleId", "Into remaining quantity list");

                var sentInStore = sentInStoreItems.FirstOrDefault(s => s.StyleItemId == size.StyleItemId);
                var sentToFBA = sentToFBAItems.FirstOrDefault(s => s.StyleItemId == size.StyleItemId);
                var soldMarkets = marketsSoldItems.Where(s => s.StyleItemId == size.StyleItemId).ToList(); 
                var specialCase = specialCaseItems.FirstOrDefault(s => s.StyleItemId == size.StyleItemId);
                var sentToPhotoshoots = sendToPhotoshootItems.Where(s => s.StyleItemId == size.StyleItemId).ToList();

                var listings = styleItemListings.Where(l => l.StyleItemId == size.StyleItemId)
                    .Where(l =>
                        !l.ItemPublishedStatus.HasValue ||
                        l.ItemPublishedStatus == (int)PublishedStatuses.Published ||
                        l.ItemPublishedStatus == (int)PublishedStatuses.HasChanges ||
                        l.ItemPublishedStatus == (int)PublishedStatuses.None)
                    .ToList();
                var byMarketplace = listings.GroupBy(l => new { l.Market, l.MarketplaceId }).Select(l =>
                    new MarketplaceListingInfo
                    {
                        MarketplaceId = l.Key.MarketplaceId,
                        Market = l.Key.Market,
                        Count = l.Count(),

                        SalePrice = l.Max(i => i.SalePrice),
                        SaleStartDate = l.Min(i => i.SaleStartDate),
                        MaxPiecesOnSale = l.Max(i => i.MaxPiecesOnSale),
                        PiecesSoldOnSale = l.Max(i => i.PiecesSoldOnSale),
                        OnSaleListingCount = l.Count(i => i.SalePrice.HasValue && i.SalePrice < i.CurrentPrice && i.SaleStartDate <= today)
                    });

                var marketplacesInfoString = String.Join(";",
                                            byMarketplace
                                                .OrderBy(m => MarketHelper.GetMarketIndex((MarketType)m.Market, m.MarketplaceId))
                                                .Select(m => MarketHelper.GetShortName(m.Market, m.MarketplaceId) + ":" + m.Count)
                                                .ToList());

                var saleInfoString = String.Join(";",
                    byMarketplace.OrderBy(m => MarketHelper.GetMarketIndex((MarketType)m.Market, m.MarketplaceId))
                                                .Where(m => m.OnSaleListingCount > 0)
                                                .Select(m => MarketHelper.GetShortName(m.Market, m.MarketplaceId) 
                                                    + ":" + PriceHelper.GetCurrencySymbol((MarketType)m.Market, m.MarketplaceId) + m.SalePrice 
                                                    + ":" + m.OnSaleListingCount
                                                    + ":" + m.MaxPiecesOnSale 
                                                    + ":" + m.PiecesSoldOnSale)
                                                .ToList());

                var cacheItem = new StyleItemCacheDTO()
                {
                    Id = size.StyleItemId.Value,
                    StyleId = size.StyleId.Value,
                    Size = size.Size,
                    Cost = size.ItemPrice,

                    MarketsSoldQuantityFromDate = soldMarkets.Any() ? soldMarkets.Sum(s => s.SoldQuantity ?? 0) : 0,
                    ScannedSoldQuantityFromDate = sentInStore != null ? (sentInStore.SoldQuantity ?? 0) : 0,
                    SentToFBAQuantityFromDate = sentToFBA != null ? (sentToFBA.SoldQuantity ?? 0) : 0,
                    SpecialCaseQuantityFromDate = specialCase != null ? (specialCase.SoldQuantity ?? 0) : 0,
                    SentToPhotoshootQuantityFromDate = sentToPhotoshoots != null ? sentToPhotoshoots.Sum(s => s.SoldQuantity ?? 0) : 0,

                    TotalMarketsSoldQuantity = soldMarkets.Any() ? soldMarkets.Sum(s => s.TotalSoldQuantity ?? 0) : 0,
                    TotalScannedSoldQuantity = sentInStore != null ? (sentInStore.TotalSoldQuantity ?? 0) : 0,
                    TotalSentToFBAQuantity = sentToFBA != null ? (sentToFBA.TotalSoldQuantity ?? 0) : 0,
                    TotalSpecialCaseQuantity = specialCase != null ? (specialCase.TotalSoldQuantity ?? 0) : 0,
                    TotalSentToPhotoshootQuantity = sentToPhotoshoots != null ? sentToPhotoshoots.Sum(s => s.TotalSoldQuantity ?? 0) : 0,

                    InventoryQuantity = size.TotalQuantity ?? 0,
                    InventoryQuantitySetDate = size.QuantitySetDate,

                    BoxQuantity = size.BoxQuantity,
                    BoxQuantitySetDate = size.BoxQuantitySetDate,

                    ScannedMaxOrderDate = sentInStore != null ? sentInStore.MaxOrderDate : null,

                    MarketplacesInfo = marketplacesInfoString,
                    SalesInfo = saleInfoString,
                    
                    IsInVirtual = inVirtualStyleStyleItemIds.Contains(size.StyleItemId.Value),

                    CalcDate = nowUtc
                };

                cacheItem.RemainingQuantity = cacheItem.InventoryQuantity
                                              - cacheItem.MarketsSoldQuantityFromDate
                                              - cacheItem.ScannedSoldQuantityFromDate
                                              - cacheItem.SpecialCaseQuantityFromDate
                                              - cacheItem.SentToFBAQuantityFromDate
                                              - cacheItem.SentToPhotoshootQuantityFromDate;

                //NOTE: keeping Remaining Quantity negative, needed to detect Oversold, only when quantity became negative. Zero still fine, when sold last item
                //if (cacheItem.RemainingQuantity < 0)
                //    cacheItem.RemainingQuantity = 0;

                results.Add(cacheItem);
            }

            return results;
        }

        private void ProcessStyleItemUpdateResults(IUnitOfWork db,
            IQuantityManager quantityManager, 
            IList<EntityUpdateStatus<long>> updates,
            DateTime when)
        {
            var historyRecordsToUpdateDate = when.AddHours(-24);
            var unProccessedChangeLogEntries = db.StyleItemQuantityHistories.GetAll().Where(h =>
                h.CreateDate > historyRecordsToUpdateDate
                && !h.RemainingQuantity.HasValue
                && h.Type != (int)QuantityChangeSourceType.RemainingChanged).ToList();

            foreach (var update in updates)
            {
                //NOTE: added snapshots to style size history
                if (update.Tag != update.TagSecond)
                {
                    quantityManager.LogStyleItemQuantity(db,
                        update.Id,
                        StringHelper.TryGetInt(update.Tag),
                        StringHelper.TryGetInt(update.TagSecond),
                        QuantityChangeSourceType.RemainingChanged,
                        null,
                        null,
                        null,
                        when,
                        null);
                }

                //NOTE: Update previous records with remaining quantity values
                var logEntriesToUpdate = unProccessedChangeLogEntries.Where(silog => silog.StyleItemId == update.Id
                        //&& silog.CreateDate < update.CalcDate
                        ).ToList();
                logEntriesToUpdate.ForEach(l => l.RemainingQuantity = StringHelper.TryGetInt(update.Tag));
            }

            db.Commit();
        }

        #endregion

        private IList<ParentItemCacheDTO> ComposeParentItemCache(IUnitOfWork db)
        {
            var parentItems = db.ParentItems.GetAllAsDto().ToList();
            var items = db.Items.GetAllViewAsDto().ToList();
            var itemsDates = db.ItemOrderMappings.GetLastShippedDateForItem().ToList();
            var positions = db.NodePositions.GetAll().ToList();

            var parentItemCaches = new List<ParentItemCacheDTO>();
            foreach (var parentItem in parentItems)
            {
                var childItems = items.Where(i => i.ParentASIN == parentItem.ASIN
                    && i.Market == parentItem.Market
                    && (String.IsNullOrEmpty(parentItem.MarketplaceId) || i.MarketplaceId == parentItem.MarketplaceId)).ToList();
                
                var displayQuantitySum = childItems.Sum(ch => ch.DisplayQuantity < 0 ? 0 : ch.DisplayQuantity) ?? 0;
                var realQuantitySum = childItems.Sum(ch => ch.RealQuantity < 0 ? 0 : ch.RealQuantity);

                var hasChildWithFakeParentItem = childItems.Any(ch => ch.IsAmazonParentASIN == false && ch.RealQuantity > 0);
                var hasQtyDifferences = childItems.Any(ch => ch.AmazonRealQuantity != Math.Min(ch.DisplayQuantity ?? ch.RealQuantity, ch.RealQuantity) && !ch.IsFBA);

                //NOTE: Amazon Current Price is a price without sales
                var hasPriceDifferences = childItems.Any(ch => ch.AmazonCurrentPrice != ch.CurrentPrice 
                    && !ch.IsFBA
                    && ch.AmazonCurrentPrice.HasValue
                    && ch.IsExistOnAmazon == true
                    && ch.RealQuantity > 0);

                var hasListings = childItems.Any(); //NOTE: based on ViewItems, already have not removed

                var lastSoldItem = itemsDates
                    .Where(i => childItems.Any(ch => ch.Id == i.Id))
                    .OrderByDescending(i => i.LastSoldDate)
                    .FirstOrDefault();

                var positionString = String.Join(";", positions.Where(p => p.ASIN == parentItem.ASIN
                                && p.Market == parentItem.Market
                                && p.MarketplaceId == parentItem.MarketplaceId)
                    .Select(p => p.NodeName + ": " + p.Position)
                    .ToList());


                #region Get Min/Max Prices

                var minOriginalPrice = childItems.Where(ch => ch.RealQuantity > 0)
                    .Select(ch => ch.CurrentPrice)
                    .OrderBy(p => p)
                    .FirstOrDefault();
                var minSalePrice = childItems.Where(ch => ch.RealQuantity > 0 
                        && ch.SalePrice.HasValue 
                        && ch.SaleStartDate <= DateTime.UtcNow)
                    .Select(ch => ch.SalePrice)
                    .OrderBy(pr => pr)
                    .FirstOrDefault();
                var maxOriginalPrice = childItems.Where(ch => ch.RealQuantity > 0)
                    .Select(ch => ch.CurrentPrice)
                    .OrderByDescending(p => p)
                    .FirstOrDefault();

                var maxSalePrice = childItems.Where(ch => ch.RealQuantity > 0
                        && ch.SalePrice.HasValue
                        && ch.SaleStartDate <= DateTime.UtcNow)
                    .Select(ch => ch.SalePrice)
                    .OrderByDescending(pr => pr)
                    .FirstOrDefault();

                var minPrice = PriceHelper.Min(minOriginalPrice, minSalePrice);
                var maxPrice = PriceHelper.Max(maxOriginalPrice, maxSalePrice);
                
                #endregion


                var lastOpenDate = childItems.Any() ? (DateTime?) childItems.Max(ch => ch.OpenDate ?? ch.CreateDate) : null;

                DateTime? lastDate = null;
                if (lastSoldItem != null)
                    lastDate = lastSoldItem.LastSoldDate;

                parentItemCaches.Add(new ParentItemCacheDTO
                {
                    Id = parentItem.Id,

                    Market = parentItem.Market,
                    MarketplaceId = parentItem.MarketplaceId,

                    DisplayQuantity = displayQuantitySum,
                    RealQuantity = realQuantitySum,

                    HasListings = hasListings,
                    HasChildWithFakeParentASIN = hasChildWithFakeParentItem,
                    HasQtyDifferences = hasQtyDifferences,
                    HasPriceDifferences = hasPriceDifferences,

                    MaxPrice = maxPrice,
                    MinPrice = minPrice,

                    LastSoldDate = lastDate,
                    LastOpenDate = lastOpenDate,

                    PositionsInfo = positionString,
                });
            }

            return parentItemCaches;
        }


        private IList<ItemCacheDTO> ComposeItemCache(IUnitOfWork db)
        {
            var items = db.Items.GetAllViewAsDto().ToList();
            var itemsDates = db.ItemOrderMappings.GetLastShippedDateForItem().ToList();

            var itemCaches = new List<ItemCacheDTO>();
            foreach (var item in items)
            {
                var lastSoldItem = itemsDates
                    .Where(i => i.Id == item.Id)
                    .OrderByDescending(i => i.LastSoldDate)
                    .FirstOrDefault();

                DateTime? lastDate = null;
                if (lastSoldItem != null)
                    lastDate = lastSoldItem.LastSoldDate;

                itemCaches.Add(new ItemCacheDTO()
                {
                    Id = item.Id,

                    LastSoldDate = lastDate
                });
            }

            return itemCaches;
        }
    }
}
