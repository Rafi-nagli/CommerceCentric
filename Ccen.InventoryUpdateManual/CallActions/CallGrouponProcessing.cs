using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Groupon;
using Amazon.Model.Implementation.Sync;
using CsvHelper;
using CsvHelper.Configuration;
using Groupon.Api;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallGrouponProcessing
    {
        private IDbFactory _dbFactory;
        private ITime _time;
        private ILogService _log;
        private IWeightService _weightService;
        private IAutoCreateListingService _autoCreateListingService;

        public CallGrouponProcessing(IDbFactory dbFactory, 
            ICacheService cacheService,
            IEmailService emailService,
            ILogService log, 
            ITime time)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
            _weightService = new WeightService();
            var barcodeService = new BarcodeService(log, time, dbFactory);
            var itemHistoryService = new ItemHistoryService(log, time, dbFactory);
            _autoCreateListingService = new AutoCreateNonameListingService(_log, _time, dbFactory, cacheService, barcodeService, emailService, itemHistoryService, AppSettings.IsDebug);
        }

        public void UpdateOrders(GrouponApi api)
        {
            var updater = new BaseOrderUpdater(api, _log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                updater.UpdateOrders(db);
            }
        }

        public void MergeKidsStyleInfoes(string destFile, string priceFile)
        {
            var itemPrices = new List<ItemDTO>();
            using (StreamReader streamReader = new StreamReader(priceFile))
            {
                using (CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimFields = true,
                }))
                {
                    while (reader.Read())
                    {
                        var sku = reader["StyleID"];
                        if (String.IsNullOrEmpty(sku))
                            continue;

                        var targetCost = StringHelper.TryGetDecimal((reader["Target Unit Cost"] ?? "").Replace("$", ""));
                        var marketRate = StringHelper.TryGetDecimal((reader["Market  Rate"] ?? "").Replace("$", ""));

                        itemPrices.Add(new ItemDTO()
                        {
                            SKU = sku,
                            ListPrice = targetCost,
                            SalePrice = marketRate
                        });
                    }
                }
            }

            var fileName = Path.GetFileNameWithoutExtension(destFile);
            var filePath = Path.GetDirectoryName(destFile);
            using (TextWriter writer = new StreamWriter(Path.Combine(filePath, fileName + "_filtered.csv")))
            {
                using (var csvWriter = new CsvWriter(writer, new CsvConfiguration
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimFields = true,
                }))
                {
                    csvWriter.Configuration.Encoding = Encoding.UTF8;

                    using (StreamReader streamReader = new StreamReader(destFile))
                    {
                        using (CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
                        {
                            HasHeaderRecord = true,
                            Delimiter = ",",
                            TrimFields = true,
                        }))
                        {
                            reader.Read();
                            foreach (var field in reader.FieldHeaders)
                                csvWriter.WriteField(field);
                            csvWriter.NextRecord();

                            while (reader.Read())
                            {
                                var sku = reader["StyleID"];
                                var itemPrice = itemPrices.FirstOrDefault(i => i.SKU == sku);

                                var record = reader.CurrentRecord;
                                record[10] = itemPrice?.ListPrice.ToString();
                                record[11] = itemPrice?.SalePrice.ToString();

                                foreach (var field in record)
                                    csvWriter.WriteField(field);
                                csvWriter.NextRecord();
                            }
                        }
                    }
                }
            }
        }

        public void ImportGroupId(string filepath)
        {
            var items = new List<ItemDTO>();

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);

                var sku = headerRow.FirstOrDefault(i => i != null && StringHelper.ContainsNoCase(i.ToString(), "SKU"));
                var groupId = headerRow.FirstOrDefault(i => i != null && StringHelper.ContainsNoCase(i.ToString(), "Group Id"));

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row != null && row.GetCell(0) != null && row.GetCell(1) != null)
                    {
                        try
                        {
                            items.Add(new ItemDTO()
                            {
                                SKU = StringHelper.TrimWhitespace(row.GetCell(sku.ColumnIndex).ToString()),
                                ParentASIN = StringHelper.TrimWhitespace(row.GetCell(groupId.ColumnIndex).ToString())
                            });
                        }
                        catch (Exception ex)
                        {
                            _log.Info("Issue with processing: " + ex.Message);
                        }
                    }
                }
            }

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var item in items)
                {
                    var dbParentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.Market == (int)MarketType.Groupon
                        && pi.ASIN == item.SKU);
                    if (dbParentItem != null)
                    {
                        _log.Info("ParentASIN: " + dbParentItem.ASIN + " assigned to group: " + item.ParentASIN);
                        dbParentItem.GroupId = item.ParentASIN;
                    }
                    else
                    {
                        _log.Info("Not found parentASIN: " + item.SKU);
                    }
                }
                db.Commit();
            }
        }

        public void CreateGrouponListings(string filename)
        {
            var skuPriceInfo = LoadItems(filename);
            var targetMarket = MarketType.Groupon;
            string targetMarketplaceId = null;

            using (var db = _dbFactory.GetRWDb())
            {
                var styleIds = skuPriceInfo.Select(i => i.SKU).ToList();
                var dtoStyleList = db.Styles.GetAllAsDto().Where(st => styleIds.Contains(st.StyleID) && !st.Deleted).ToList();

                var existMarketItems = db.Items.GetAll().Where(i => i.Market == (int)targetMarket
                    && (i.MarketplaceId == targetMarketplaceId || String.IsNullOrEmpty(targetMarketplaceId)))
                    .ToList();

                IList<MessageString> messages;
                foreach (var style in dtoStyleList)
                {
                    if (existMarketItems.Any(i => i.StyleId == style.Id))
                        continue;

                    var priceInfo = skuPriceInfo.FirstOrDefault(s => s.SKU == style.StyleID);
                    
                    var model = _autoCreateListingService.CreateFromStyle(db,
                        style.Id,
                        targetMarket,
                        targetMarketplaceId,
                        out messages);

                    model.Market = (int)MarketType.Groupon;
                    model.MarketplaceId = "";

                    if (model.Variations.Select(i => i.StyleId).Distinct().Count() != 1)
                    {
                        _log.Info("Parent ASIN is multilisting");
                        continue;
                    }

                    model.Variations.ForEach(v => { if (String.IsNullOrEmpty(v.Barcode)) { v.AutoGeneratedBarcode = true; } });
                    model.Variations.ForEach(v => v.CurrentPrice = (priceInfo?.CurrentPrice > 0 ? (priceInfo?.CurrentPrice ?? 99) : 99));

                    _autoCreateListingService.PrepareData(model);
                    _autoCreateListingService.Save(model, null, db, _time.GetAppNowTime(), null);
                }
            }
        }

        private IList<ItemDTO> LoadItems(string filepath)
        {
            var items = new List<ItemDTO>();

            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (filepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);
                var skuIndex = headerRow.FirstOrDefault(i => i != null && StringHelper.ContainsNoCase(i.ToString(), "SKU"))?.ColumnIndex;
                var costIndex = headerRow.FirstOrDefault(i => i != null && StringHelper.ContainsNoCase(i.ToString(), "Cost"))?.ColumnIndex;

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row != null && row.GetCell(0) != null)
                    {
                        try
                        {
                            items.Add(new ItemDTO()
                            {
                                SKU = StringHelper.TrimWhitespace(row.GetCell(skuIndex.Value).ToString()),
                                CurrentPrice = PriceHelper.RoundToTwoPrecision(ExcelHelper.TryGetCellDecimal(row.GetCell(costIndex.Value))) ?? 99,
                            });
                        }
                        catch (Exception ex)
                        {
                            _log.Info("Issue with processing: " + ex.Message);
                        }
                    }
                }
            }

            return items;
        }

        public void GenerateFeed(string skuFilename)
        {
            IList<string> styleIds = LoadItems(skuFilename).Select(i => i.SKU).ToList();
            IList<string> skuList = new List<string>();

            using (var db = _dbFactory.GetRWDb())
            {
                skuList = (from l in db.Listings.GetAll()
                           join i in db.Items.GetAll() on l.ItemId equals i.Id
                           join st in db.Styles.GetAll() on i.StyleId equals st.Id
                           where l.Market == (int)MarketType.Groupon
                             && styleIds.Contains(st.StyleID)
                             //&& l.RealQuantity > 10
                           select l.SKU).ToList();
            }

            //var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/Groupon_Template.xls");
            var filename = "Groupon_Product_Feed_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            var outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            var grouponFeed = new GrouponProductFeed(_log, _time, _dbFactory, _weightService, AppSettings.GrouponImageBaseUrl, AppSettings.GrouponImageDirectory);//, templateFile);

            grouponFeed.ExportToFile(outputFile, skuList, MarketType.Groupon, "");
        }
    }
}
