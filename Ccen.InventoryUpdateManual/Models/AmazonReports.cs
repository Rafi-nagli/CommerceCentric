using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Sizes;

namespace Amazon.InventoryUpdateManual.Models
{
    public class OurAmazonReports
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private AmazonApi _amazonApi;

        public OurAmazonReports(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            AmazonApi amazonApi)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
            _amazonApi = amazonApi;
        }

        public class InventoryRecord
        {
            public string StyleId { get; set; }
            public string Name { get; set; }
            public int TotalQty { get; set; }
            public string Locations { get; set; }
            public decimal? AvgCost { get; set; }
        }

        public void BuildFullInventoryReport()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var allStyles = db.Styles.GetAll().Where(st => !st.Deleted)
                    .OrderBy(st => st.StyleID)
                    .ToList();
                var allStyleItemCaches = db.StyleItemCaches.GetAll().ToList();
                var allLocations = db.StyleLocations.GetAll().ToList();
                var allOpenBoxes = db.OpenBoxes.GetAll().ToList();
                var allSealedBoxes = db.SealedBoxes.GetAll().ToList();
                var allOpenBoxItems = db.OpenBoxItems.GetAll().Where(ob => ob.StyleItemId.HasValue).ToList();
                var allSealedBoxItems = db.SealedBoxItems.GetAll().Where(ob => ob.StyleItemId.HasValue).ToList();

                var results = new List<InventoryRecord>();

                foreach (var style in allStyles)
                {
                    var remainingQty = allStyleItemCaches.Where(sic => sic.StyleId == style.Id).Sum(sic => Math.Max(0, sic.RemainingQuantity));
                    var locations = allLocations.Where(l => l.StyleId == style.Id).ToList();
                    var locationString = String.Join("; ",
                        locations.OrderByDescending(l => l.IsDefault)
                            .Select(l => l.Isle + "/" + l.Section + "/" + l.Shelf + (l.IsDefault ? "(def)" : "")));

                    decimal totalItemCost = 0;
                    var totalBoxCount = 0;
                    var openBoxes = allOpenBoxes.Where(ob => ob.StyleId == style.Id).Where(ob => !ob.Deleted && !ob.Archived).ToList();
                    foreach (var openBox in openBoxes)
                    {
                        if (openBox.Price == 0)
                            continue;

                        var qty = allOpenBoxItems.Where(ob => ob.BoxId == openBox.Id).Sum(ob => ob.Quantity);
                        totalItemCost += openBox.BoxQuantity*qty*openBox.Price;
                        totalBoxCount += openBox.BoxQuantity*qty;
                    }
                    var sealedBoxes = allSealedBoxes.Where(sb => sb.StyleId == style.Id).Where(ob => !ob.Deleted && !ob.Archived).ToList();
                    foreach (var sealedBox in sealedBoxes)
                    {
                        if (sealedBox.PajamaPrice == 0)
                            continue;

                        var qty = allSealedBoxItems.Where(sb => sb.BoxId == sealedBox.Id).Sum(sb => sb.BreakDown);
                        totalItemCost += sealedBox.BoxQuantity*qty*sealedBox.PajamaPrice;
                        totalBoxCount += sealedBox.BoxQuantity*qty;
                    }

                    var avgCost = totalBoxCount != 0 ? totalItemCost/(decimal)totalBoxCount : (decimal?)null;
                    if (avgCost == null || avgCost == 0)
                        avgCost = 5.01M;

                    results.Add(new InventoryRecord()
                    {
                        StyleId = style.StyleID,
                        Name = style.Name,
                        TotalQty = remainingQty,
                        Locations = locationString,
                        AvgCost = PriceHelper.RoundToTwoPrecision(avgCost),
                    });
                }

                _log.Info("Records: " + results.Count);

                var filename = "InventoryOnAmazon_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
                var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                var b = new ExportColumnBuilder<InventoryRecord>();
                var columns = new List<ExcelColumnInfo>()
                {
                    b.Build(p => p.StyleId, "Style Id", 30),
                    b.Build(p => p.Name, "Name", 45),
                    b.Build(p => p.TotalQty, "Total Qty", 20),

                    b.Build(p => p.Locations, "Locations list", 30),
                    b.Build(p => p.AvgCost, "Item Cost", 15),
                };

                using (var stream = ExcelHelper.Export(results, columns))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var fileStream = File.Create(filepath))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }



        public class FoundItemInfo
        {
            public string Barcode { get; set; }

            public string StyleString { get; set; }
            public string StyleSize { get; set; }
            public long? RemainingQuantity { get; set; }

            public string ParentASIN { get; set; }
            public string ASIN { get; set; }
            public string AmazonSize { get; set; }
            public string AmazonColor { get; set; }
            public string AmazonUrl { get; set; }

            public string Name { get; set; }

            public FoundItemInfo()
            {
                
            }

            public FoundItemInfo(ItemDTO item)
            {
                Barcode = item.Barcode;
                
                ParentASIN = item.ParentASIN;
                ASIN = item.ASIN;
                AmazonUrl = "https://www.amazon.com/dp/" + item.ASIN;
                AmazonSize = item.Size;
                AmazonColor = item.Color;
                Name = item.Name;
            }
        }

        public void BuildReportMissingSizesOnAmazon()
        {
            List<string> asinWithErrors;
            
            //var amazonParentItem = _amazonApi.GetItems(_log, _time, new List<string>() { "B01NCKLUGM" }, ItemFillMode.Defualt, out asinWithErrors);

            using (var db = _dbFactory.GetRWDb())
            {
                var allParentItems = db.ParentItems
                    .GetAllAsDto(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId)
                    .Where(pi => !String.IsNullOrEmpty(pi.ASIN))
                    .ToList();
                var allChildItems = db.Items.GetAllViewAsDto(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId).ToList();

                var allSizesQuery = from s in db.Styles.GetAll()
                                   join si in db.StyleItems.GetAll() on s.Id equals si.StyleId
                                   join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                                   where !s.Deleted
                                       && sic.RemainingQuantity > 0
                                   select new
                                   {
                                       StyleId = s.Id,
                                       StyleString = s.StyleID,
                                       StyleItemId = si.Id,
                                       StyleSize = si.Size,
                                       RemainingQuantity = sic.RemainingQuantity,
                                   };

                var allStyleSizes = allSizesQuery.ToList();

                var results = new List<ItemDTO>();

                foreach (var parentItem in allParentItems)
                {
                    var childItems = allChildItems.Where(ch => ch.ParentASIN == parentItem.ASIN).ToList();
                    var parentItemStyleIdList = childItems.Where(ch => ch.StyleId.HasValue).Select(ch => ch.StyleId.Value).ToList();
                    var parentItemStyleItemIdList = childItems.Where(ch => ch.StyleItemId.HasValue).Select(ch => ch.StyleItemId.Value).ToList();
                    var styleStyleItemList = allStyleSizes.Where(si => parentItemStyleIdList.Contains(si.StyleId)).ToList();

                    var missingStyleItemList = styleStyleItemList.Where(si => !parentItemStyleItemIdList.Contains(si.StyleItemId)).ToList();

                    if (missingStyleItemList.Any())
                    {
                        _log.Info(parentItem.ASIN + " may has missing sizes");

                        var amazonParentItem = RetryHelper.ActionWithRetries(() =>
                            _amazonApi.GetProductForASIN(new List<string>() {parentItem.ASIN}).FirstOrDefault(),
                            _log,
                            3,
                            5000,
                            RetryModeType.Normal);

                        if (amazonParentItem != null && amazonParentItem.Listings != null)
                        {
                            var amazonChildItems = amazonParentItem.Listings;
                            foreach (var missingStyleItem in missingStyleItemList)
                            {
                                var possibleSizes = db.SizeMappings.GetAllAsDto()
                                        .Where(m => m.StyleSize == missingStyleItem.StyleSize)
                                        .ToList();
                                possibleSizes.Insert(0, new SizeMappingDTO()
                                {
                                    StyleSize = missingStyleItem.StyleSize,
                                    ItemSize = missingStyleItem.StyleSize
                                });

                                ListingDTO existChildItem = null;
                                var existChildItems = amazonChildItems.Where(ai => possibleSizes.Any(s => s.ItemSize == ai.ListingSize)).ToList();
                                if (existChildItems.Any() && existChildItems.All(ch => !String.IsNullOrEmpty(ch.ListingColor)))
                                {
                                    var missingStyleItemColor = childItems.FirstOrDefault(ch => ch.StyleId == missingStyleItem.StyleId)?.Color;
                                    existChildItems = existChildItems.Where(ch => ch.ListingColor == missingStyleItemColor).ToList();
                                }
                                existChildItem = existChildItems.FirstOrDefault();
                                if (existChildItem != null)
                                {
                                    _log.Info("Added ParentASIN=" + parentItem.ASIN 
                                        + ", styleId=" + missingStyleItem.StyleString 
                                        + ", size=" + missingStyleItem.StyleSize);
                                    results.Add(new ItemDTO()
                                    {
                                        ParentASIN = parentItem.ASIN,
                                        ASIN = existChildItem.ASIN,
                                        Size = existChildItem.ListingSize,
                                        Color = existChildItem.ListingColor,
                                        SourceMarketUrl = "https://www.amazon.com/dp/" + existChildItem.ASIN,

                                        StyleString = missingStyleItem.StyleString,
                                        StyleSize = missingStyleItem.StyleSize,
                                        RemainingQuantity = missingStyleItem.RemainingQuantity
                                    });
                                }
                            }
                        }
                    }
                }

                _log.Info("Listings with possible missing sizes: " + results.Count);
                
                var filename = "MissingItemsOnAmazon_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
                var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
                ExportMissingItem(results, filepath);
            }
        }


        public void BuildReportWithUnassignedToAmazonBarcodes()
        {
            var results = new List<FoundItemInfo>();

            using (var db = _dbFactory.GetRWDb())
            {
                var barcodeQuery = from s in db.Styles.GetAll()
                    join si in db.StyleItems.GetAll() on s.Id equals si.StyleId
                    join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                    join sib in db.StyleItemBarcodes.GetAll() on si.Id equals sib.StyleItemId
                    where !s.Deleted
                        && sic.RemainingQuantity > 0
                    select new
                    {
                        StyleId = s.Id,
                        StyleString = s.StyleID,
                        StyleItemId = si.Id,
                        StyleSize = si.Size,
                        Barcode = sib.Barcode,
                        RemainingQuantity = sic.RemainingQuantity,
                    };

                var allBarcodes = barcodeQuery.ToList();

                var amazonItems = db.Items.GetAllViewAsDto(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId).ToList();
                var existBarcodes = amazonItems.Select(i => i.Barcode).ToList();
                var notExistOnAmazonBarcodes = allBarcodes.Where(b => !String.IsNullOrEmpty(b.Barcode)
                                                              && !existBarcodes.Contains(b.Barcode)
                                                              && !b.Barcode.StartsWith("647") //Exclude our barcodes
                                                              ).ToList();

                _log.Info("Not exist barcodes count=" + notExistOnAmazonBarcodes.Count());

                var index = 0;
                var step = 5;
                while (index < notExistOnAmazonBarcodes.Count)
                {
                    var barcodesToCheck = notExistOnAmazonBarcodes.Skip(index).Take(step).ToList();
                    var foundItems = RetryHelper.ActionWithRetries(() => 
                        _amazonApi.GetProductForBarcode(barcodesToCheck.Select(b => b.Barcode).ToList()),
                        _log,
                        3,
                        5000,
                        RetryModeType.Normal);

                    var notEmptyItems = foundItems.Where(i => !String.IsNullOrEmpty(i.ASIN)).ToList();
                    if (notEmptyItems.Any())
                    {
                        foreach (var notEmptyItem in notEmptyItems)
                        {
                            var foundItem = new FoundItemInfo(notEmptyItem);
                            var barcodeInfo = barcodesToCheck.FirstOrDefault(b => b.Barcode == notEmptyItem.Barcode);
                            if (barcodeInfo != null)
                            {
                                foundItem.StyleString = barcodeInfo.StyleString;
                                foundItem.StyleSize = barcodeInfo.StyleSize;
                                foundItem.RemainingQuantity = barcodeInfo.RemainingQuantity;
                            }
                            results.Add(foundItem);
                        }
                    }

                    index += step;
                }
            }

            var filename = "BarcodesExistOnAmazon_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            ExportFoundItem(results, filepath);
        }

        public void ExportFoundItem(IList<FoundItemInfo> items, string filepath)
        {
            var b = new ExportColumnBuilder<FoundItemInfo>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.Barcode, "Barcode", 15),
                b.Build(p => p.StyleString, "Style Id", 20),
                b.Build(p => p.StyleSize, "Style Size", 15),
                b.Build(p => p.RemainingQuantity, "Remaining Qty", 20),
                b.Build(p => p.Name, "Name", 50),
                b.Build(p => p.ASIN, "ASIN", 15),
                b.Build(p => p.AmazonSize, "Amazon Size", 15),
                b.Build(p => p.AmazonColor, "Amazon Color", 15),
                b.Build(p => p.AmazonUrl, "Amazon Url", 40),
            };

            using (var stream = ExcelHelper.Export(items, columns))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Create(filepath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        public void ExportMissingItem(IList<ItemDTO> items, string filepath)
        {
            var b = new ExportColumnBuilder<ItemDTO>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.StyleString, "Style Id", 20),
                b.Build(p => p.StyleSize, "Style Size", 15),
                b.Build(p => p.RemainingQuantity, "Remaining Qty", 20),

                b.Build(p => p.ParentASIN, "ParentASIN", 15),
                b.Build(p => p.ASIN, "ASIN", 15),
                b.Build(p => p.Size, "Amazon Size", 15),
                b.Build(p => p.Color, "Amazon Color", 15),
                b.Build(p => p.SourceMarketUrl, "Amazon Url", 40),
            };

            using (var stream = ExcelHelper.Export(items, columns))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Create(filepath))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
