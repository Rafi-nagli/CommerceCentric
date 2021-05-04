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
    public class PhotoshootPickListItemViewModel
    {
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
                return UrlHelper.GetThumbnailUrl(Picture, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail: true);
            }
        }

        public bool HasColorVariation { get; set; }

        public IList<PickListSizeInfo> SoldSizes { get; set; }
        public IList<PickListSizeInfo> AllSizes { get; set; }

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

        
        public PhotoshootPickListItemViewModel()
        {
            SoldSizes = new List<PickListSizeInfo>();
            AllSizes = new List<PickListSizeInfo>();
        }

        private static IEnumerable<ListingOrderDTO> GetOrdersWithItemsForPickList(IUnitOfWork db, long photoshootShipmentId)
        {
            var photoshootEntries = db.PhotoshootPickListEntries.GetAllAsDto().Where(i => i.PhotoshootPickListId == photoshootShipmentId).ToList();
            var styleItemIds = photoshootEntries.Select(p => p.StyleItemId).ToList();
            var styleItems = db.StyleItems.GetAll().Where(si => styleItemIds.Contains(si.Id)).ToList();
            var styleIds = styleItems.Select(p => p.StyleId).ToList();
            var styles = db.Styles.GetAll().Where(st => styleIds.Contains(st.Id)).ToList();

            var results = new List<ListingOrderDTO>();
            foreach (var entry in photoshootEntries)
            {
                var styleItem = styleItems.FirstOrDefault(si => si.Id == entry.StyleItemId);
                var style = styles.FirstOrDefault(st => st.Id == styleItem.StyleId);
                
                
                var listing = new ListingOrderDTO()
                {
                    Id = entry.Id,
                    QuantityOrdered = entry.TakenQuantity,

                    ASIN = "",
                    ParentASIN = "",
                    SKU = "",
                    ListingId = "",
                    Picture = style.Image,
                    Size = styleItem.Size,
                    Color = styleItem.Color,

                    StyleID = style.StyleID,
                    StyleImage = style.Image,
                    StyleSize = styleItem.Size,
                    StyleId = style.Id,
                    StyleItemId = styleItem.Id,
                };
                results.Add(listing);
            }
            return results;
        }

        #region Compose Size Group

        private static void UpdateExistGroup(PhotoshootPickListItemViewModel existGroup, 
            bool hasColorVariation,
            ListingOrderDTO item)
        {
            //This group always has appropriate color, only check size
            PickListSizeInfo existSize = null;
            if (hasColorVariation)
                existSize = existGroup.SoldSizes.FirstOrDefault(s => s.StyleSize == item.StyleSize && s.Color == item.Color);
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

        private static PhotoshootPickListItemViewModel ComposeNewGroup(ListingOrderDTO item, 
            bool hasColorVariation,
            Style style, 
            IList<StyleItemDTO> styleItems)
        {
            //Add new group
            IList<PickListSizeInfo> allSizes = new List<PickListSizeInfo>();

            //Prepare image
            var image = item.ItemPicture;
            if (String.IsNullOrEmpty(image))
                image = item.StyleImage;

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

            var newGroup = new PhotoshootPickListItemViewModel
            {
                StyleName = style != null ? style.Name : null,
                StyleId = style != null ? (long?)style.Id : null,
                Color = item.Color,
                StyleString = style != null ? style.StyleID : null,
                ProductName = title,
                Picture = image,
                HasColorVariation = hasColorVariation,

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
            };

            return newGroup;
        }
        
        #endregion

        public static IList<PhotoshootPickListItemViewModel> GetPickListItems(IUnitOfWork db, 
            ILogService log, 
            long fbaShipmentId)
        {
            var shipmentItems = GetOrdersWithItemsForPickList(db, fbaShipmentId).ToList();
            
            var styleIdList = shipmentItems.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();

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
            var summaryItems = new List<PhotoshootPickListItemViewModel>();
            //Filling summary items list with variation detections
            //If one item has color variation all item of his style is color variating
            foreach (var item in shipmentItems)
            {
                try
                {
                    var style = styles.FirstOrDefault(s => s.StyleID == item.StyleID);
                    var styleId = style != null ? style.Id : (long?)null;

                    var hasColorVariation = false;

                    //If has color variation check with color
                    var existGroup = summaryItems.FirstOrDefault(i => i.StyleId == styleId);

                    //Update exist group
                    if (existGroup != null)
                    {
                        UpdateExistGroup(existGroup, 
                            hasColorVariation,
                            item);
                    }
                    else
                    {
                        var newGroup = ComposeNewGroup(item, 
                            hasColorVariation,
                            style, 
                            styleItems);

                        summaryItems.Add(newGroup);
                    }
                }
                catch (Exception ex)
                {
                    log.Fatal("Print Pick List. PickListItemViewModel.", ex);
                }
            }

            foreach (var item in summaryItems)
            {
                ItemPostProcessing(item, 
                    styleLocations, 
                    remainingQties,
                    stylesValues);
            }
            
            var sortedItems = summaryItems
                .OrderBy(m => m.SortIsle)
                .ThenBy(m => m.SortSection)
                .ThenBy(m => m.SortShelf)
                .ThenBy(m => m.Gender)
                .ThenBy(m => m.MainLicense)
                .ThenBy(m => m.SubLicense1)
                .ThenBy(m => (m.Gender != "" && m.StyleId.HasValue) ? m.StyleId.ToString() : "")
                .ThenBy(m => m.MainListing.ASIN).ToList();

            for (int i = 0; i < sortedItems.Count; i++)
                sortedItems[i].Number = i + 1;

            return sortedItems;
        }

        private static void ItemPostProcessing(PhotoshootPickListItemViewModel item,
            IList<StyleLocationDTO> locations,
            IList<SoldSizeInfo> remainingQtyes,
            IList<StyleFeatureValueDTO> stylesValues)
        {
            if (item.StyleId != null)
            {
                //Set Locations
                item.Locations = ComposeLocations(locations, item.StyleId.Value);

                //Set Remaining Quantity from inventory
                foreach (var size in item.AllSizes)
                {
                    var soldSize = item.SoldSizes.FirstOrDefault(s => s.StyleItemId == size.StyleItemId);
                    var remainingQty = remainingQtyes.FirstOrDefault(r => r.StyleItemId == size.StyleItemId);
                    if (remainingQty != null)
                    {
                        size.Quantity = Math.Max(0, (remainingQty.TotalQuantity ?? 0) - (soldSize != null ? soldSize.Quantity : 0));
                    }
                }

                item.SoldQuantity = item.SoldSizes.Sum(s => s.Quantity);
            }

            //Sort sizes
            item.SoldSizes = item.SoldSizes.OrderBy(s => s.SizeIndex).ThenBy(s => s.StyleSize).ThenBy(s => s.Color).ToList();
            item.AllSizes = item.AllSizes.OrderBy(s => s.SizeIndex).ThenBy(s => s.StyleSize).ThenBy(s => s.Color).ToList();

            //Fill quantity
            foreach (var size in item.AllSizes)
            {
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