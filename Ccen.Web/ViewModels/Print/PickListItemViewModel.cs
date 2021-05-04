using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Features;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Print
{
    public class PickListItemViewModel
    {
        public class OrderInfo
        {
            public string OrderId { get; set; }
            public int ShippingMethodId { get; set; }
            public DateTime? OrderDate { get; set; }
            public bool IsPrime { get; set; }
            public string InitialShippingService { get; set; }
            public int ShipmentProviderType { get; set; }
        }

        public class ListingInfo
        {
            public string ASIN { get; set; }
            public int Market { get; set; }
            public string MarketplaceId { get; set; }
            public string ParentASIN { get; set; }
            public string SourceMarketId { get; set; }
            public int Quantity { get; set; }
            public int? Rank { get; set; }
        }

        public string Picture { get; set; }
        public long? StyleId { get; set; }
        public string StyleString { get; set; }

        public string Color { get; set; }

        public string StyleName { get; set; }
        public string ProductName { get; set; }

        public string CuttedProductName
        {
            get { return StringHelper.Substring(ProductName, 50); }
        }

        public int Number { get; set; }
        public int SoldQuantity { get; set; }

        //for Sorting
        public string Gender { get; set; }
        public string MainLicense { get; set; }
        public string SubLicense1 { get; set; }

        public IList<ListingInfo> ListingInfoes { get; set; }

        public ListingInfo MainListing
        {
            get
            {
                if (ListingInfoes == null || ListingInfoes == null)   
                    return new ListingInfo();
                return ListingInfoes
                    .OrderByDescending(l => l.Quantity)
                    .ThenBy(l => l.Rank ?? RankHelper.DefaultRank)
                    .FirstOrDefault();
            }
        }

        public string ProductUrl
        {
            get { return UrlHelper.GetProductUrl(MainListing.ParentASIN, 
                (MarketType)MainListing.Market, 
                MainListing.MarketplaceId); }
        }

        public string MarketProductUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(MainListing.ParentASIN, MainListing.SourceMarketId, (MarketType)MainListing.Market, MainListing.MarketplaceId);
            }
        }

        public string ProductStyleUrl
        {
            get { return UrlHelper.GetProductByStyleUrl(StyleString,
                    (MarketType)MainListing.Market,
                    MainListing.MarketplaceId);
            }
        }

        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleString); }
        }
        
        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(Picture, 0, 150, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }

        public bool HasColorVariation { get; set; }
        public bool RemovePriceTag { get; set; }

        public IList<PickListSizeInfo> SoldSizes { get; set; }
        public IList<PickListSizeInfo> AllSizes { get; set; }

        //Orders of item
        public IList<OrderInfo> Orders { get; set; }

        #region Locations
        public IList<StyleLocationDTO> Locations { get; set; }

        public int SortIsle
        {
            get 
            {
                return Locations != null && Locations.Count > 0 ? Locations.First().SortIsle : Int32.MaxValue; 
            }
        }

        public int SortSection
        {
            get 
            {
                return Locations != null && Locations.Count > 0 ? Locations.First().SortSection : Int32.MaxValue; 
            }
        }

        public int SortShelf
        {
            get 
            {
                return Locations != null && Locations.Count > 0 ? Locations.First().SortShelf : Int32.MaxValue; 
            }
        }
        
        #endregion

        
        public PickListItemViewModel()
        {
            SoldSizes = new List<PickListSizeInfo>();
            AllSizes = new List<PickListSizeInfo>();
        }

        private static IEnumerable<DTOOrder> GetOrdersWithItemsForPickList(IUnitOfWork db, 
            IWeightService weightService,
            OrderSearchFilterViewModel search)
        {
            var filter = search.GetModel();
            filter.UnmaskReferenceStyles = true;

            //if (filter.BatchId.HasValue)
            //{
            //    var batchOrderList = db.OrderToBatches.GetViewAllAsDto().Where(o => o.BatchId == filter.BatchId.Value).ToList();
            //    if (batchOrderList.Any()) //Batch created using OrderToBatch
            //    {
            //        filter.EqualOrderIds = batchOrderList.Select(o => o.OrderId).Distinct().ToArray();
            //        filter.BatchId = null;
            //    }
            //}

            var orders = db.ItemOrderMappings.GetFilteredOrdersWithItems(weightService, filter)
                .ToList();

            return orders;
        }

        #region Compose Size Group

        private static void UpdateExistGroup(PickListItemViewModel existGroup, 
            bool hasColorVariation,
            ListingOrderDTO item,
            DTOOrder order)
        {
            //This group always has appropriate color, only check size
            PickListSizeInfo existSize = null;
            if (hasColorVariation)
                existSize = existGroup.SoldSizes.FirstOrDefault(s => (s.StyleSize == item.StyleSize && s.Color == item.Color) || s.StyleItemId == item.StyleItemId);
            else
                existSize = existGroup.SoldSizes.FirstOrDefault(s => s.StyleSize == item.StyleSize);

            var currency = PriceHelper.GetCurrencySymbol((MarketType)item.Market, item.MarketplaceId);
            var itemPrice = item.ItemPrice;
            if (item.Market == (int)MarketType.Amazon
                || item.Market == (int)MarketType.AmazonEU
                || item.Market == (int)MarketType.AmazonAU)
                itemPrice = item.QuantityOrdered != 0 ? item.ItemPrice / item.QuantityOrdered : 0;

            if (existSize != null)
            {
                existSize.Quantity += item.QuantityOrdered;

                if (!existSize.Prices.Any(p => p.Currency == currency && Math.Abs(p.Price - itemPrice) <= 0.1M))
                {
                    existSize.Prices.Add(new PriceInfo()
                    {
                        Price = itemPrice,
                        Currency = currency
                    });
                }

                if (existSize.Sizes.All(s => s != item.Size))
                    existSize.Sizes.Add(item.Size);
            }
            else
            {
                existGroup.SoldSizes.Add(new PickListSizeInfo
                {
                    Quantity = item.QuantityOrdered,
                    Sizes = new List<string>() { item.Size },
                    StyleSize = item.StyleSize,
                    StyleItemId = item.StyleItemId,
                    Color = item.Color,
                    Weight = item.Weight,
                    Prices = new List<PriceInfo>()
                    {
                        new PriceInfo()
                        {
                            Price = itemPrice,
                            Currency = currency, 
                        }
                    }
                });
            }

            //Orders of item
            existGroup.Orders.Add(new OrderInfo
            {
                OrderDate = order.OrderDate,
                OrderId = order.OrderId,
                ShippingMethodId = order.ShippingMethodId,
                IsPrime = order.OrderType == (int)OrderTypeEnum.Prime,
                InitialShippingService = order.InitialServiceType,
                ShipmentProviderType = order.ShipmentProviderType,
            });

            var existListing = existGroup.ListingInfoes.FirstOrDefault(l => l.ASIN == item.ASIN
                                                                            && l.Market == item.Market
                                                                            && l.MarketplaceId == item.MarketplaceId);
            if (existListing == null)
            {
                existGroup.ListingInfoes.Add(new ListingInfo()
                {
                    ASIN = item.ASIN,
                    ParentASIN = item.ParentASIN,
                    SourceMarketId = item.SourceMarketId,
                    MarketplaceId = item.MarketplaceId,
                    Market = item.Market,
                    Quantity = item.QuantityOrdered,
                    Rank = item.Rank,
                });
            }
            else
            {
                existListing.Quantity += item.QuantityOrdered;
            }
        }

        private static PickListItemViewModel ComposeNewGroup(ListingOrderDTO item, 
            bool hasColorVariation,
            Style style, 
            IList<StyleItemDTO> styleItems,
            DTOOrder order)
        {
            //Add new group
            IList<PickListSizeInfo> allSizes = new List<PickListSizeInfo>();

            //Prepare image
            ///NOTE: Always get from style
                //var image = item.ItemPicture;
                //if (String.IsNullOrEmpty(image))
            var image = item.StyleImage;

            var title = item.Title;
            if (item.SourceStyleString != item.StyleID)        //NOTE: happens when item has linked style OR item style was changed
            {
                if (!String.IsNullOrEmpty(item.StyleImage))
                    image = item.StyleImage;

                if (style != null && !String.IsNullOrEmpty(style.Name))
                    title = style.Name;
            }

            title = SizeHelper.ExcludeSizeInfo(title);

            if (style != null)
            {
                allSizes = styleItems.Where(i => i.StyleId == style.Id)
                    .Select(i => new PickListSizeInfo()
                    {
                        StyleSize = i.Size,
                        StyleItemId = i.StyleItemId,
                        Weight = i.Weight
                    }).ToList();
            }

            var itemPrice = item.ItemPrice;
            if (item.Market == (int)MarketType.Amazon 
                || item.Market == (int)MarketType.AmazonEU
                || item.Market == (int)MarketType.AmazonAU)
                itemPrice = item.QuantityOrdered != 0 ? item.ItemPrice/item.QuantityOrdered : 0;

            var newGroup = new PickListItemViewModel
            {
                StyleName = style != null ? style.Name : null,
                StyleId = style != null ? (long?)style.Id : null,
                Color = item.Color,
                StyleString = style != null ? style.StyleID : null,
                ProductName = title,
                Picture = image,
                HasColorVariation = hasColorVariation,
                RemovePriceTag = style != null ? style.RemovePriceTag : false,

                ListingInfoes = new List<ListingInfo>()
                {
                    new ListingInfo() {
                        ASIN = item.ASIN,
                        ParentASIN = item.ParentASIN,
                        SourceMarketId = item.SourceMarketId,
                        MarketplaceId = item.MarketplaceId,
                        Market = item.Market,
                        Quantity = item.QuantityOrdered,
                        Rank = item.Rank,
                    }
                },

                SoldSizes = new List<PickListSizeInfo>
                {
                    new PickListSizeInfo
                    {
                        Quantity = item.QuantityOrdered,
                        Sizes = new List<string>() { item.Size },
                        StyleSize = item.StyleSize,
                        StyleItemId = item.StyleItemId,
                        Color = item.Color,
                        Weight = item.Weight,
                        Prices = new List<PriceInfo>()
                        {
                            new PriceInfo() {
                                Price = itemPrice,
                                Currency = PriceHelper.GetCurrencySymbol((MarketType)item.Market, item.MarketplaceId)
                            }
                        }
                    }
                },
                AllSizes = allSizes,

                //Orders of item
                Orders = new List<OrderInfo>
                {
                    new OrderInfo
                    {
                        OrderId = order.OrderId,
                        OrderDate = order.OrderDate,
                        ShippingMethodId = order.ShippingMethodId,
                        IsPrime = order.OrderType == (int)OrderTypeEnum.Prime,
                        InitialShippingService = order.InitialServiceType,
                        ShipmentProviderType = order.ShipmentProviderType,
                    }
                }
            };

            return newGroup;
        }
        
        #endregion

        public static IList<PickListItemViewModel> GetPickListItems(IUnitOfWork db, 
            ILogService log, 
            IWeightService weightService,
            OrderSearchFilterViewModel search)
        {
            var orders = GetOrdersWithItemsForPickList(db, weightService, search);
            
            var styleIdList = orders.SelectMany(o => o.Items.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList()).ToList();

            var styleItems = db.StyleItems.GetAllAsDto()
                .Where(s => styleIdList.Contains(s.StyleId))
                .ToList();
            
            var remainingQties = db.StyleItemCaches.GetAllCacheItems()
                .Where(s => styleIdList.Contains(s.StyleId))
                .Select(ch => new SoldSizeInfo()
                {
                    StyleItemId = ch.Id,
                    StyleId = ch.StyleId,
                    TotalQuantity = ch.RemainingQuantity
                }).ToList();
            
            var pendingAndNoBatchSoldQtyes = db.ItemOrderMappings.GetPendingAndOtherUnshippedOrderItemQtyes(search.BatchId);

            var styles = db.Styles.GetAllActive().Where(s => styleIdList.Contains(s.Id)).ToList();
            var stylesValues = db.StyleFeatureValues.GetFeatureValueByStyleIdByFeatureId(
                styleIdList,
                new int[]
                {
                    StyleFeatureHelper.MAIN_LICENSE,
                    StyleFeatureHelper.SUB_LICENSE1,
                    StyleFeatureHelper.GENDER
                });
            var styleLocations = db.StyleLocations.GetAllAsDTO().Where(l => styleIdList.Contains(l.StyleId)).ToList();

            //Step #1. Detect which styles have color variations
            //Note: we set variate true only if items into one parent item has color variation

            //NOTE: Using only for color variation calculation
            var items = db.Items.GetAllViewAsDto(MarketType.Amazon, String.Empty).Where(l => 
                l.StyleId.HasValue && styleIdList.Contains(l.StyleId.Value)).ToList(); //TODO: for all marketplaces
            var isItemVariate = GetSKUColorVariationStatus(items);
            
            var summaryItems = new List<PickListItemViewModel>();
            //Filling summary items list with variation detections
            //If one item has color variation all item of his style is color variating
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    try
                    {
                        //Correct price for WM second day
                        if (order.Market == (int)MarketType.Walmart
                            && item.IsPrime)
                        {
                            if (item.IsPrime)
                                item.ItemPrice = item.ItemPrice - 5;
                        }

                        var style = styles.FirstOrDefault(s => s.StyleID == item.StyleID);
                        var styleId = style != null ? style.Id : (long?)null;

                        //Has color variation (Parent Item have more then 1 item with same size)
                        var itemStatus = isItemVariate.FirstOrDefault(v => v.SKU == item.SKU);
                        var hasColorVariation = itemStatus != null ? itemStatus.HasColorVariation : false;

                        //If has color variation check with color
                        var existGroup = summaryItems.FirstOrDefault(i => i.StyleId == styleId);

                        //Update exist group
                        if (existGroup != null)
                        {
                            UpdateExistGroup(existGroup, 
                                hasColorVariation,
                                item, 
                                order);
                        }
                        else
                        {
                            var newGroup = ComposeNewGroup(item, 
                                hasColorVariation,
                                style, 
                                styleItems,
                                order);

                            summaryItems.Add(newGroup);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Fatal("Print Pick List. PickListItemViewModel.", ex);
                    }
                }
            }

            foreach (var item in summaryItems)
            {
                ItemPostProcessing(item, 
                    styleLocations, 
                    remainingQties,
                    pendingAndNoBatchSoldQtyes, 
                    stylesValues);
            }

            var sortedItems = summaryItems
                .OrderBy(m => m.SortIsle)
                .ThenBy(m => m.SortSection)
                .ThenBy(m => m.SortShelf)
                .ThenBy(m => m.StyleString)
                .ToList();

            for (int i = 0; i < sortedItems.Count; i++)
                sortedItems[i].Number = i + 1;

            return sortedItems;
        }

        private static void ItemPostProcessing(PickListItemViewModel item,
            IList<StyleLocationDTO> locations,
            IList<SoldSizeInfo> remainingQtyes,
            IList<SoldSizeInfo> pendingAndNoBatchSoldQtyes,
            IList<StyleFeatureValueDTO> stylesValues)
        {
            if (item.StyleId != null)
            {
                //Set Locations
                item.Locations = ComposeLocations(locations, item.StyleId.Value);

                //Set Remaining Quantity from inventory
                foreach (var size in item.AllSizes)
                {
                    var remainingQty = remainingQtyes.FirstOrDefault(r => r.StyleItemId == size.StyleItemId);
                    if (remainingQty != null)
                    {
                        size.Quantity = Math.Max(0, remainingQty.TotalQuantity ?? 0);
                    }
                }

                item.SoldQuantity = item.SoldSizes.Sum(s => s.Quantity);
            }

            //Sort sizes
            item.SoldSizes = item.SoldSizes.OrderBy(s => s.SizeIndex).ThenBy(s => s.StyleSize).ThenBy(s => s.Color).ToList();
            item.AllSizes = item.AllSizes.OrderBy(s => s.SizeIndex).ThenBy(s => s.StyleSize).ThenBy(s => s.Color).ToList();

            //Fill pending items
            foreach (var size in item.AllSizes)
            {
                var pendingAndNoBatchSoldQuery = pendingAndNoBatchSoldQtyes.Where(s => s.StyleItemId == size.StyleItemId);

                //if (item.HasColorVariation)
                //    pendingAndNoBatchSoldQuery = pendingAndNoBatchSoldQuery.Where(s => s.Color == item.Color);

                size.PendingQuantity = pendingAndNoBatchSoldQuery.Sum(s => s.SoldQuantity) ?? 0;
                var soldItem = item.SoldSizes.FirstOrDefault(i => i.StyleItemId == size.StyleItemId);
                size.OrderedQuantity = soldItem != null ? soldItem.Quantity : 0;
            }


            //Fill feature fields
            StyleFeatureValueDTO mainLicenseValue = null;
            StyleFeatureValueDTO subLicenseValue = null;
            StyleFeatureValueDTO genderValue = null;
            if (item.StyleId != null)
            {
                genderValue = stylesValues.FirstOrDefault(v => v.StyleId == item.StyleId && v.FeatureId == StyleFeatureHelper.GENDER);
                mainLicenseValue = stylesValues.FirstOrDefault(v => v.StyleId == item.StyleId && v.FeatureId == StyleFeatureHelper.MAIN_LICENSE);
                subLicenseValue = stylesValues.FirstOrDefault(v => v.StyleId == item.StyleId && v.FeatureId == StyleFeatureHelper.SUB_LICENSE1);
            }
            item.Gender = genderValue != null ? genderValue.Value : "";
            item.MainLicense = mainLicenseValue != null ? mainLicenseValue.Value : null;
            item.SubLicense1 = subLicenseValue != null ? subLicenseValue.Value : null;

            
            //Keep only sold items for record w/o styles
            if (String.IsNullOrEmpty(item.StyleString))
            {
                item.AllSizes = item.AllSizes.Where(s => item.SoldSizes.Any(ss => ss.StyleSize == s.StyleSize)).ToList();
                //Replace null value
                item.StyleString = item.StyleString ?? "-";
            }
        }

        private class ColorVariationStatus
        {
            public string SKU { get; set; }
            public string StyleId { get; set; }
            public bool HasColorVariation { get; set; }
        }

        public static string[] _colorTable = new string[]
        {
            "NAVY",
            "RED",
            "GRY",
            "BLK",
            "WHT",
            "CAR",
            "SHELL",
            "BLUE",
            "ROYAL",
            "GRN",
            "CHO",
            "TAN",
            "BROWN",
            "BLUE",
            "LBL",
            "OLV",
            "MOC",
            "OST", //"BLK-OST"
            "CROC", //"BLK-CROC"
            "MAM",
            "VAM",
            "YEL",
            "BRL"
        };

        private static IList<ColorVariationStatus> GetSKUColorVariationStatus(IList<ItemDTO> items)
        {
            var results = new List<ColorVariationStatus>();
            foreach (var item in items)
            {
                if (String.IsNullOrEmpty(item.SKU))
                    continue;
                
                if (results.All(r => r.SKU != item.SKU))
                {
                    bool hasColorVariation = _colorTable.Any(c => item.SKU.ToUpper().Contains("-" + c));
                    results.Add(new ColorVariationStatus() {
                        SKU = item.SKU, 
                        StyleId = item.StyleString,
                        HasColorVariation = hasColorVariation
                    });
                }
            }

            //If any item belong to style has color variation, all items of that style marks as color variate
            foreach (var result in results)
            {
                var hasStyleColorVariation = results.Where(r => r.StyleId == result.StyleId).Any(r => r.HasColorVariation);
                result.HasColorVariation = hasStyleColorVariation;
            }

            return results;
        }
        
        private static IList<StyleLocationDTO> ComposeLocations(IList<StyleLocationDTO> styleLocations, long styleId)
        {
            var locations = styleLocations.Where(sl => sl.StyleId == styleId).OrderByDescending(sl => sl.IsDefault).ToList();
            return locations;
        }
    }
}