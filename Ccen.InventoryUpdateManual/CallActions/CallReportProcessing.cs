using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Amazon.Api;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Common.Models;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon.Readers;
using Amazon.Model.Implementation.Trackings;
using Amazon.Model.Implementation.Trackings.Rules;
using Amazon.Model.SyncService.Models.AmazonReports;
using Amazon.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using eBay.Api;
using Newtonsoft.Json;
using Amazon.Core.Models.Inventory;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Amazon.InventoryUpdateManual.TestCases
{
    public class CallReportProcessing
    {
        private ILogService _log;
        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private eBayApi _eBayApi;
        private AmazonApi _amazonApiCom;
        private AmazonApi _amazonApiCA;
        private CompanyDTO _company;
        private ITime _time;
        private IHtmlScraperService _htmlScraper;

        public CallReportProcessing(ILogService log,
            IDbFactory dbFactory,
            IEmailService emailService,
            AmazonApi amazonApiCom,
            AmazonApi amazonApiCA,
            eBayApi eBayApi,
            CompanyDTO company,
            ITime time)
        {
            _log = log;
            _dbFactory = dbFactory;
            _emailService = emailService;
            _amazonApiCA = amazonApiCA;
            _amazonApiCom = amazonApiCom;
            _eBayApi = eBayApi;
            _company = company;
            _time = time;
            _htmlScraper = new HtmlScraperService(log, time, dbFactory);
        }

        public void GenerateSKUInfoesForStyles(string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);
                    
                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var sku = row.GetCell(0).ToString();

                        if (String.IsNullOrEmpty(sku))
                            continue;

                        var dbListing = db.Listings.GetAll()
                            .OrderBy(l => l.Market)
                            .FirstOrDefault(l => l.SKU == sku
                                && !l.IsRemoved
                                && l.Market == (int)MarketType.Walmart);
                        if (dbListing == null)
                            continue;

                        var dbItem = db.Items.GetAll().FirstOrDefault(it => it.Id == dbListing.ItemId);
                        var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.Id == dbItem.StyleId);
                        var dbStyleItemCache = db.StyleItemCaches.GetAll().FirstOrDefault(sic => sic.Id == dbItem.StyleItemId);
                        var dbStyleItem = db.StyleItems.GetAll().FirstOrDefault(st => st.Id == dbItem.StyleItemId);
                        var dbStyleImages = db.StyleImages.GetAll()
                                .Where(sim => sim.StyleId == dbStyle.Id
                                    && !sim.IsSystem)
                                .OrderByDescending(sim => sim.IsDefault)
                                .OrderBy(sim => sim.OrderIndex)
                                .ToList();

                        if (dbListing == null
                            || dbItem == null
                            || dbStyle == null
                            || dbStyleItemCache == null)
                            continue;

                        var brandName = db.StyleFeatureValues.GetAllWithFeature().FirstOrDefault(f => f.StyleId == dbStyle.Id
                            && f.FeatureName == StyleFeatureHelper.MAIN_LICENSE_KEY)?.Value;




                        SetCellValue(row, 1, dbStyle.Name);
                        SetCellValue(row, 2, dbStyle.Description);
                        SetCellValue(row, 3, brandName);
                        SetCellValue(row, 4, dbItem.Barcode);
                        SetCellValue(row, 5, dbStyleItemCache.Cost.ToString());
                        SetCellValue(row, 6, dbStyleItem.Weight.ToString());
                        for (var j = 0; j < Math.Min(dbStyleImages.Count, 3); j++)
                            SetCellValue(row, 7 + j, dbStyleImages[j].Image);

                        SetCellValue(row, 11, dbStyle.BulletPoint1);
                        SetCellValue(row, 12, dbStyle.BulletPoint2);
                        SetCellValue(row, 13, dbStyle.BulletPoint3);
                        SetCellValue(row, 14, dbStyle.BulletPoint4);
                        SetCellValue(row, 15, dbStyle.BulletPoint5);

                        SetCellValue(row, 19, dbStyleItem.Color);
                        SetCellValue(row, 20, dbStyleItem.Size);

                        //SetCellValue(row, 3, dbItem.SourceMarketId);
                        //SetCellValue(row, 1, dbStyleItemCache.RemainingQuantity.ToString());
                        //SetCellValue(row, 2, dbStyleItemCache.Cost.ToString());
                        //SetCellValue(row, 2, brandName);
                        //SetCellValue(row, 4, dbListing.SKU);

                        //SetCellValueIfEmpty(row, 6, dbListing.CurrentPrice.ToString());// dbStyle.MSRP?.ToString());
                        //SetCellValue(row, 7, dbListing.RealQuantity.ToString());
                        //SetCellValue(row, 8, dbStyleItemCache.Cost.ToString());
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void UpdateItemsBarcode(string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var barcode = row.GetCell(4).ToString();
                        var asin = row.GetCell(5)?.ToString();

                        if (!String.IsNullOrEmpty(asin)
                            && !String.IsNullOrEmpty(barcode))
                        {
                            var items = db.Items.GetAll().Where(l => l.Market == (int)MarketType.Amazon
                                && l.ASIN == asin).ToList();

                            foreach (var item in items)
                            {
                                _log.Info("ASIN: " + item.ASIN + ", barcode: " + item.Barcode + " => " + barcode);
                                item.Barcode = barcode;
                            }
                        }
                    }
                }
                db.Commit();
            }
        }


        public void FillWithApiASIN(AmazonApi api, string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var barcode = row.GetCell(4).ToString();

                        if (String.IsNullOrEmpty(barcode))
                            continue;

                        var productInfo = api.GetProductForBarcode(new List<string>() { barcode }).FirstOrDefault();
                        var apiASIN = productInfo?.ASIN;

                        SetCellValue(row, 5, apiASIN);
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void FillWithListingInfo(string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);


                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var sku = row.GetCell(0).ToString();

                        if (String.IsNullOrEmpty(sku))
                            continue;

                        var dbListing = db.Listings.GetAll()
                            .OrderBy(l => l.Market)
                            .FirstOrDefault(l => l.SKU == sku 
                                && !l.IsRemoved
                                && l.Market == (int)MarketType.Walmart);
                        if (dbListing == null)
                            continue;

                        var dbItem = db.Items.GetAll().FirstOrDefault(it => it.Id == dbListing.ItemId);
                        var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.Id == dbItem.StyleId);
                        var dbStyleItemCache = db.StyleItemCaches.GetAll().FirstOrDefault(sic => sic.Id == dbItem.StyleItemId);
                        var dbStyleItem = db.StyleItems.GetAll().FirstOrDefault(st => st.Id == dbItem.StyleItemId);
                        var dbStyleImages = db.StyleImages.GetAll()
                                .Where(sim => sim.StyleId == dbStyle.Id
                                    && !sim.IsSystem)
                                .OrderByDescending(sim => sim.IsDefault)
                                .OrderBy(sim => sim.OrderIndex)
                                .ToList();

                        if (dbListing == null
                            || dbItem == null
                            || dbStyle == null
                            || dbStyleItemCache == null)
                            continue;

                        var brandName = db.StyleFeatureValues.GetAllWithFeature().FirstOrDefault(f => f.StyleId == dbStyle.Id
                            && f.FeatureName == StyleFeatureHelper.MAIN_LICENSE_KEY)?.Value;

                        SetCellValue(row, 1, dbStyle.Name);
                        SetCellValue(row, 2, dbStyle.Description);
                        SetCellValue(row, 3, brandName);
                        SetCellValue(row, 4, dbItem.Barcode);
                        SetCellValue(row, 5, dbStyleItemCache.Cost.ToString());
                        SetCellValue(row, 6, dbStyleItem.Weight.ToString());
                        for (var j = 0; j < Math.Min(dbStyleImages.Count, 3); j++)
                            SetCellValue(row, 7 + j, dbStyleImages[j].Image);

                        SetCellValue(row, 11, dbStyle.BulletPoint1);
                        SetCellValue(row, 12, dbStyle.BulletPoint2);
                        SetCellValue(row, 13, dbStyle.BulletPoint3);
                        SetCellValue(row, 14, dbStyle.BulletPoint4);
                        SetCellValue(row, 15, dbStyle.BulletPoint5);

                        SetCellValue(row, 19, dbStyleItem.Color);
                        SetCellValue(row, 20, dbStyleItem.Size);

                        //SetCellValue(row, 3, dbItem.SourceMarketId);
                        //SetCellValue(row, 1, dbStyleItemCache.RemainingQuantity.ToString());
                        //SetCellValue(row, 2, dbStyleItemCache.Cost.ToString());
                        //SetCellValue(row, 2, brandName);
                        //SetCellValue(row, 4, dbListing.SKU);

                        //SetCellValueIfEmpty(row, 6, dbListing.CurrentPrice.ToString());// dbStyle.MSRP?.ToString());
                        //SetCellValue(row, 7, dbListing.RealQuantity.ToString());
                        //SetCellValue(row, 8, dbStyleItemCache.Cost.ToString());
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void FillWithQtyInfo(string filepath, int skuColumnIndex, int qtyColumnIndex)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);


                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var sku = row.GetCell(skuColumnIndex).ToString();

                        if (String.IsNullOrEmpty(sku))
                            continue;

                        var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == sku && !l.IsRemoved);
                        if (dbListing == null)
                            continue;

                        var dbItem = db.Items.GetAll().FirstOrDefault(it => it.Id == dbListing.ItemId);
                        var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.Id == dbItem.StyleId);
                        var dbStyleItemCache = db.StyleItemCaches.GetAll().FirstOrDefault(sic => sic.Id == dbItem.StyleItemId);

                        if (dbListing == null
                            || dbItem == null
                            || dbStyle == null
                            || dbStyleItemCache == null)
                            continue;

                        var brandName = db.StyleFeatureValues.GetAllWithFeature().FirstOrDefault(f => f.StyleId == dbStyle.Id
                            && f.FeatureName == StyleFeatureHelper.MAIN_LICENSE_KEY)?.Value;

                        //SetCellValue(row, 3, dbItem.SourceMarketId);
                        SetCellValue(row, qtyColumnIndex, dbStyleItemCache.RemainingQuantity.ToString());
                        //SetCellValue(row, 2, dbStyleItemCache.Cost.ToString());
                        //SetCellValue(row, 2, brandName);
                        //SetCellValue(row, 4, dbListing.SKU);
                        //SetCellValue(row, 5, dbStyle.Name);
                        //SetCellValueIfEmpty(row, 6, dbListing.CurrentPrice.ToString());// dbStyle.MSRP?.ToString());
                        //SetCellValue(row, 7, dbListing.RealQuantity.ToString());
                        //SetCellValue(row, 8, dbStyleItemCache.Cost.ToString());
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void FillWithStyleInfo(string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);


                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var sku = row.GetCell(0).ToString();

                        if (String.IsNullOrEmpty(sku))
                            continue;

                        var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.SKU == sku
                            && !l.IsRemoved
                            && l.Market == (int)MarketType.Walmart);
                        if (dbListing == null)
                            continue;

                        var dbItem = db.Items.GetAll().FirstOrDefault(it => it.Id == dbListing.ItemId);
                        var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.Id == dbItem.StyleId);
                        var dbStyleItemCache = db.StyleItemCaches.GetAll().FirstOrDefault(sic => sic.Id == dbItem.StyleItemId);

                        if (dbListing == null
                            || dbItem == null
                            || dbStyle == null
                            || dbStyleItemCache == null)
                            continue;

                        var dbStyleLocation = db.StyleLocations.GetAll()
                            .Where(s => s.StyleId == dbStyle.Id)
                            .OrderByDescending(s => s.IsDefault)
                            .ThenBy(s => s.Id)
                            .FirstOrDefault();

                        
                        //var brandName = db.StyleFeatureValues.GetAllWithFeature().FirstOrDefault(f => f.StyleId == dbStyle.Id
                        //    && f.FeatureName == StyleFeatureHelper.MAIN_LICENSE_KEY)?.Value;

                        //SetCellValue(row, 1, dbStyle.Name);
                        //SetCellValueIfEmpty(row, 2, dbListing.CurrentPrice.ToString());// dbStyle.MSRP?.ToString());
                        //SetCellValue(row, 3, dbStyle.Image);
                        //SetCellValue(row, 4, dbStyle.Description);

                        //SetCellValue(row, 7, dbItem.Color);
                        //SetCellValue(row, 8, dbItem.Size);

                        SetCellValue(row, 3, dbStyleLocation != null ? dbStyleLocation.Isle + "/" + dbStyleLocation.Section + "/" + dbStyleLocation.Shelf : "-");
                        SetCellValue(row, 4, dbStyleLocation != null ? (dbStyleLocation.SortIsle * 100000 + dbStyleLocation.SortSection * 1000 + dbStyleLocation.SortShelf).ToString() : "");

                        //SetCellValue(row, 3, dbItem.SourceMarketId);
                        //SetCellValue(row, 1, dbItem.Barcode);
                        //SetCellValue(row, 2, brandName);
                        //SetCellValue(row, 4, dbListing.SKU);
                        //SetCellValue(row, 7, dbListing.RealQuantity.ToString());

                        //SetCellValue(row, 9, dbStyleItemCache.RemainingQuantity.ToString());

                        //SetCellValue(row, 2, dbStyleItemCache.Cost.ToString());
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void FillWithRateInfo(string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    var shippingMethods = db.ShippingMethods.GetAllAsDto();

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var trackingNumber = row.GetCell(0).ToString();
                        var weight = row.GetCell(3)?.ToString();

                        if (String.IsNullOrEmpty(trackingNumber))
                            continue;

                        if (weight != "0")
                            continue;
                        
                        var dbShippingInfo = db.OrderShippingInfos.GetAllAsDto().FirstOrDefault(l => l.TrackingNumber == trackingNumber);
                        if (dbShippingInfo != null && dbShippingInfo.WeightD == 0)
                        {
                            dbShippingInfo.WeightD = (from oi in db.OrderItems.GetWithListingInfo()
                                                      join m in db.ItemOrderMappings.GetAll() on oi.OrderItemEntityId equals m.OrderItemId
                                                      where m.ShippingInfoId == dbShippingInfo.Id
                                                      select oi.Weight).Sum();
                        }
                        if (dbShippingInfo == null)
                        {
                            dbShippingInfo = db.MailLabelInfos.GetAllAsDto().FirstOrDefault(m => m.TrackingNumber == trackingNumber);
                            if (dbShippingInfo != null && dbShippingInfo.WeightD == 0)
                            {
                                //dbShippingInfo.WeightD = (from oi in db.OrderItems.GetWithListingInfo()
                                //                          join m in db.MailLabelItems.GetAll() on oi.ItemOrderId equals m.ItemOrderIdentifier
                                //                          where m.Id == dbShippingInfo.Id
                                //                          select oi.Weight).Sum();
                            }
                        }
                        if (dbShippingInfo == null)
                        {
                            continue;
                        }


                        var shippingMethod = shippingMethods.FirstOrDefault(m => m.Id == dbShippingInfo.ShippingMethodId);
                        SetCellValue(row, 1, PriceHelper.PriceToString(dbShippingInfo.StampsShippingCost));
                        SetCellValue(row, 2, shippingMethod.ServiceIdentifier);
                        SetCellValue(row, 3, dbShippingInfo.WeightD.ToString());
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        private void SetCellValue(IRow row, int columnIndex, string newValue)
        {
            if (String.IsNullOrEmpty(newValue))
                return;

            var cell = row.GetCell(columnIndex);
            if (cell == null)
                cell = row.CreateCell(columnIndex);
            cell.SetCellValue(newValue);
        }

        private void SetCellValueIfEmpty(IRow row, int columnIndex, string newValue)
        {
            if (String.IsNullOrEmpty(newValue))
                return;

            var cell = row.GetCell(columnIndex);
            if (cell == null)
                cell = row.CreateCell(columnIndex);
            var currentValue = cell.ToString();
            if (String.IsNullOrEmpty(currentValue))
                cell.SetCellValue(newValue);
        }

        public void AssingCustomBarcodesToFileSKUs(string filepath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);


                    for (var i = 0; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);
                        var sku = row.GetCell(0).ToString();
                        
                        if (String.IsNullOrEmpty(sku))
                            continue;

                        var existBarcode = db.CustomBarcodes.GetAll().FirstOrDefault(b => b.SKU == sku);
                        if (existBarcode == null)
                        {
                            existBarcode = db.CustomBarcodes.GetAll().OrderBy(b => b.Id).FirstOrDefault(b => String.IsNullOrEmpty(b.SKU));
                            existBarcode.SKU = sku;
                            existBarcode.AttachSKUDate = _time.GetAppNowTime();
                            existBarcode.AttachSKUBy = null;
                            db.Commit();
                            _log.Info("Attach barcode: " + existBarcode.Barcode + ", to SKU: " + existBarcode.SKU);
                        }
                        var cell = row.GetCell(1);
                        if (cell == null)
                            cell = row.CreateCell(1);
                        cell.SetCellValue(existBarcode.Barcode);
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void ReadFeatureExAttributes(string filename)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                using (StreamReader streamReader = new StreamReader(filename))
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
                            var id = StringHelper.TryGetInt(reader["Id"]);
                            var wmCharacter = reader["WM Character"];
                            var isPermanent = reader["Permanent"] == "Yes";

                            if (!id.HasValue)
                                continue;

                            var featureValue = db.FeatureValues.Get(id.Value);
                            featureValue.ExtendedData = JsonHelper.Serialize<FeatureExAttributes>(new FeatureExAttributes()
                            {
                                WMCharacter = wmCharacter,
                                WMCharacterPermanent = isPermanent
                            });
                        }
                    }
                }
                db.Commit();
            }
        }

        public void ParseRefundReport(string filename)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var reportItems = GetAllReportItems(filename);
                reportItems = reportItems.OrderByDescending(r => r.ReceiveDate).ToList();

                var updatedList = new List<string>(reportItems.Count);
                foreach (var item in reportItems)
                {
                    var returnRequest = db.ReturnRequests.GetAll()
                        .OrderByDescending(r => r.CreateDate)
                        .FirstOrDefault(r => r.OrderNumber == item.OrderNumber);

                    if (returnRequest != null)
                    {
                        if (!updatedList.Contains(item.OrderNumber))
                        {
                            _log.Info("Return request has been updated, orderNumber=" + item.OrderNumber);
                            returnRequest.HasPrepaidLabel = item.HasPrepaidLabel;
                            returnRequest.PrepaidLabelCost = item.PrepaidLabelCost;
                            returnRequest.PrepaidLabelBy = item.PrepaidLabelBy;

                            updatedList.Add(item.OrderNumber);
                        }
                        else
                        {
                            _log.Info("Order has multiple returns: " + item.OrderNumber);
                        }
                    }
                    else
                    {
                        _log.Info("Return request == null, orderNumber=" + item.OrderNumber);
                    }
                }
                db.Commit();
            }
        }

        private IList<ReturnRequestDTO> GetAllReportItems(string filename)
        {
            var results = new List<ReturnRequestDTO>();
            var doc = new XmlDocument();
            var xml = File.ReadAllText(filename);
            doc.LoadXml(xml);
            var items = doc.SelectNodes(".//return_details");
            foreach (XmlNode item in items)
            {
                var orderId = item.SelectSingleNode("./order_id")?.InnerText;
                var labelPaidBy = item.SelectSingleNode("./label_to_be_paid_by")?.InnerText;
                var labelCost = item.SelectSingleNode("./label_details/label_cost")?.InnerText;
                var receiveDate = DateHelper.FromDateString(item.SelectSingleNode("./return_request_date")?.InnerText);
                
                var labelCostValue = StringHelper.TryGetDecimal(labelCost);
                results.Add(new ReturnRequestDTO()
                {
                    OrderNumber = orderId,
                    PrepaidLabelCost = labelCostValue,
                    HasPrepaidLabel = labelCostValue > 0, // labelPaidBy == "Customer"
                    PrepaidLabelBy = labelPaidBy,
                    ReceiveDate = receiveDate
                });
            }

            return results;
        }


        private class RefundReportLine
        {
            public string OrderNumber { get; set; }
            public decimal? ItemPrice { get; set; }
            public decimal? ShippingPrice { get; set; }
            public decimal? RefundAmount { get; set; }
            public DateTime? OrderDate { get; set; }
            public DateTime? RefundDate { get; set; }
            public string RefundReason { get; set; }
            public string RefundByName { get; set; }
        }

        public void ComposeRefundReport(DateTime fromDate)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var results = new List<RefundReportLine>();
                var failedActionTags = db.SystemActions.GetAll()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder)
                    .Select(a => a.Tag)
                    .ToList();
                var successActionTags = db.SystemActions.GetAll()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder)
                    .Select(a => a.Tag)
                    .ToList();

                var actions = db.SystemActions.GetAll()
                    .Where(t => t.Type == (int) SystemActionType.UpdateOnMarketReturnOrder
                        && t.CreateDate >= fromDate
                        && failedActionTags.Contains(t.Tag)
                        && successActionTags.Contains(t.Tag))
                    .ToList();

                var users = db.Users.GetAll().ToList();



                foreach (var action in actions)
                {
                    var refundInfo = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                    var order = db.Orders.GetAll().FirstOrDefault(o => o.AmazonIdentifier == refundInfo.OrderNumber);

                    if (order == null)
                    {
                        continue;
                    }
                    var orderItems = db.OrderItems.GetAll()
                        .Where(oi => oi.OrderId == order.Id)
                        .ToList();

                    var refundAmount = RefundHelper.GetAmount(refundInfo);

                    var shippingPrice = refundInfo.IncludeShipping ? refundInfo.Items.Sum(i => i.RefundShippingPrice) : 0;
                    var itemPrice = refundInfo.Items.Sum(i => i.RefundItemPrice);

                    results.Add(new RefundReportLine()
                    {
                        OrderNumber = order.AmazonIdentifier,
                        OrderDate = order.OrderDate,
                        //ItemPrice = itemPrice,
                        //ShippingPrice = shippingPrice,
                        RefundAmount = refundAmount,
                        RefundReason = refundInfo.RefundReason.HasValue ? 
                            RefundReasonCodeHelper.GetName((RefundReasonCodes)refundInfo.RefundReason)
                            : "-",
                        RefundDate = action.CreateDate,
                        RefundByName = action.CreatedBy.HasValue ? users.FirstOrDefault(u => u.Id == action.CreatedBy.Value).Name : "-"
                    });
                }

                var filename = "WMListingPositions_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
                var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                var b = new ExportColumnBuilder<RefundReportLine>();
                var columns = new List<ExcelColumnInfo>()
                {
                    b.Build(p => p.OrderNumber, "Order #", 25),
                    b.Build(p => p.OrderDate, "Order Date", 25),
                    //b.Build(p => p.ItemPrice, "Item Price", 35),
                    //b.Build(p => p.ShippingPrice, "Shipping Price", 15),
                    b.Build(p => p.RefundAmount, "Refund Amount", 20),
                    b.Build(p => p.RefundReason, "Refund Reason", 45),
                    b.Build(p => p.RefundDate, "Refund Date", 45),
                    b.Build(p => p.RefundByName, "Refund By Name", 45)
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

        private class RefundCompareReportLine
        {
            public string OrderNumber { get; set; }
            public decimal CcenRefundAmount { get; set; }
            public decimal AmazonRefundAmount { get; set; }
        }

        public void ComposeCompareRefundReport(DateTime fromDate)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var results = new List<RefundCompareReportLine>();
                var failedActionTags = db.SystemActions.GetAll()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                        && a.Status == (int)SystemActionStatus.Fail)
                    .Select(a => a.Tag)
                    .ToList();
                var successActionTags = db.SystemActions.GetAll()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                        && a.Status == (int)SystemActionStatus.Done)
                    .Select(a => a.Tag)
                    .ToList();

                var actions = db.SystemActions.GetAll()
                    .Where(t => t.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                        && t.CreateDate >= fromDate
                        && failedActionTags.Contains(t.Tag)
                        && successActionTags.Contains(t.Tag))
                    .ToList();

                var users = db.Users.GetAll().ToList();

                foreach (var action in actions)
                {
                    var refundInfo = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                    var order = db.Orders.GetAll().FirstOrDefault(o => o.AmazonIdentifier == refundInfo.OrderNumber);

                    var refundAmount = RefundHelper.GetAmount(refundInfo);

                    var existResult = results.FirstOrDefault(r => r.OrderNumber == refundInfo.OrderNumber);
                    if (existResult == null)
                    {
                        existResult = new RefundCompareReportLine()
                        {
                            OrderNumber = refundInfo.OrderNumber,
                        };
                        results.Add(existResult);                        
                    }
                    if (action.Status == (int)SystemActionStatus.Done)
                        existResult.AmazonRefundAmount += refundAmount ?? 0;
                    if (action.Status == (int)SystemActionStatus.Fail)
                        existResult.CcenRefundAmount += refundAmount ?? 0;
                }

                var filename = "CompareRefunds_" + DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss") + ".xls";
                var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

                var b = new ExportColumnBuilder<RefundCompareReportLine>();
                var columns = new List<ExcelColumnInfo>()
                {
                    b.Build(p => p.OrderNumber, "Order #", 25),
                    b.Build(p => p.CcenRefundAmount, "Failed Refund Amount", 35),
                    b.Build(p => p.AmazonRefundAmount, "Success Refund Amount", 35),
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


        public void ProcessScheduledReport(AmazonReportType reportType, AmazonApi api, CompanyDTO company)
        {
            var syncInfo = new DbSyncInformer(_dbFactory, _log, _time, SyncType.Listings, MarketplaceKeeper.DefaultMarketplaceId, MarketType.Amazon, String.Empty);
            var reportSettings = new AmazonReportFactory(_time).GetReportService(reportType, api.MarketplaceId);
            var dbFactory = new DbFactory();
            var styleHistoryService = new StyleHistoryService(_log, _time, _dbFactory);
            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            var styleManager = new StyleManager(_log, _time, styleHistoryService);
            var notificationService = new NotificationService(_log, _time, dbFactory);
            var actionService = new SystemActionService(_log, _time);

            IAmazonReportService reportService = new AmazonReportService(reportType,
                company.Id,
                api,
                _log,
                _time,
                dbFactory,
                syncInfo,
                styleManager,
                notificationService,
                styleHistoryService,
                itemHistoryService,
                actionService,
                reportSettings.GetParser(),
                AppSettings.ReportDirectory);

            var reports = reportService.GetReportList();
            if (reports.Any())
            {
                _log.Info("Save report");
                if (reportService.SaveScheduledReport(reports[0].ReportId))
                {
                    Console.WriteLine("Report Saved");
                    _log.Info("Process report");
                    reportService.ProcessReport();
                    reportService = null;
                    syncInfo.SyncEnd();
                }
                else
                {
                    _log.Info("Could not save report");
                }
            }
        }

        public void ProcessRequestedReport(AmazonReportType reportType, 
            AmazonApi api,
            IList<string> asinList)
        {
            var processed = false;
            AmazonReportService reportService = null;
            var reportSettings = new AmazonReportFactory(_time).GetReportService(reportType, api.MarketplaceId);
            var syncInfo = new DbSyncInformer(_dbFactory, _log, _time, SyncType.Listings,
                api.MarketplaceId, MarketType.Amazon, null);

            var styleHistoryService = new StyleHistoryService(_log, _time, _dbFactory);
            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            var styleManager = new StyleManager(_log, _time, styleHistoryService);
            var notificationService = new NotificationService(_log, _time, _dbFactory);
            var actionService = new SystemActionService(_log, _time);

            using (var db = _dbFactory.GetRWDb())
            {
                while (!processed)
                {
                    if (reportService == null)
                    {
                        _log.Info("Create reportService");
                        reportService = new AmazonReportService(reportType,
                            _company.Id,
                            api,
                            _log,
                            _time,
                            _dbFactory,
                            syncInfo,
                            styleManager,
                            notificationService,
                            styleHistoryService,
                            itemHistoryService,
                            actionService,
                            reportSettings.GetParser(),
                            AppSettings.ReportDirectory);

                        var lastUnsavedReport = db.InventoryReports.GetLastUnsaved((int)reportService.ReportType, api.MarketplaceId);
                        if (lastUnsavedReport == null || (DateTime.UtcNow - lastUnsavedReport.RequestDate) > TimeSpan.FromHours(3))
                        {
                            _log.Info("Request report");
                            reportService.RequestReport();
                        }
                        else
                        {
                            syncInfo.OpenLastSync();
                        }
                    }
                    else
                    {
                        _log.Info("Save report");
                        if (reportService.SaveRequestedReport())
                        {
                            Console.WriteLine("Report Saved");
                            _log.Info("Process report");
                            reportService.ProcessReport(asinList);
                            reportService = null;
                            processed = true;
                            return;
                        }
                        else
                        {
                            var requestStatus = reportService.GetRequestStatus();
                            Console.WriteLine(requestStatus);
                            if (requestStatus.Status == "_CANCELLED_")
                            {
                                db.InventoryReports.MarkProcessedByRequestId(reportService.ReportRequestId,
                                    ReportProcessingResultType.Cancelled);
                                reportService = null;
                            }

                            _log.Info("Could not save report");
                        }
                    }
                    Thread.Sleep(60000);
                }
            }
        }

        public void GetRankByProductApi(AmazonApi api)
        {
            var items = new List<ItemDTO>()
            {
                new ItemDTO()
                {
                    ASIN = "B00SWY44RG"
                }
            };

            var itemsWithError = new List<ItemDTO>();
            api.FillWithAdditionalInfo(_log, 
                _time, 
                items, 
                IdType.ASIN,
                ItemFillMode.Defualt, 
                out itemsWithError);

            Console.WriteLine(items[0].Rank);
        }


        public void GetRatingTest(AmazonApi api)
        {
            using (var db = new UnitOfWork(_log))
            {
                var items = db.Items.GetAll().ToList();
                var index = 0;
                var indexToSleep = 0;
                //var rank = new Dictionary<string, string>();
                //var allNodes = new List<string>();
                while (index < items.Count)
                {
                    var children = items.Skip(index).Take(10).Select(i => i.ASIN).ToList();
                    var resp = api.RetrieveOffers(_log, new List<string>()
                    {
                        "B00RA2Y7UK", 
                        "B00B4TT8M2", 
                        "B00M2OWTWY", 
                        "B00M2OWT1A"
                    }); 
                    
                    // amazonApi.RetrieveOffers(children); //amazonApi.RetrieveOffers(new[] {"B00M4L8DJI", children.First(), children.Last()}); //amazonApi.RetrieveOffers(children);

                    if (resp.Items != null && resp.Items.Any() && resp.Items[0].Item != null)
                    {
                        foreach (var item in items)
                        {
                            var el = resp.Items[0].Item.FirstOrDefault(i => i.ASIN == item.ASIN);
                            if (el != null && el.OfferSummary != null)
                            {
                                var itemId = item.Id;
                                var lowestPrice = el.OfferSummary.LowestNewPrice != null
                                    ? (decimal?)GeneralUtils.GetPrice(el.OfferSummary.LowestNewPrice.FormattedPrice.Replace(".",
                                        CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                    //decimal.GetReportItems(el.OfferSummary.LowestNewPrice.Amount.Replace(".",
                                    //  CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                    : null;
                                var listings = db.Listings.GetFiltered(l => l.ItemId == itemId).ToList();
                                foreach (var listing in listings)
                                {
                                    listing.LowestPrice = lowestPrice;
                                }
                            }
                        }
                        db.Commit();

                    }
                    index += 10;
                    indexToSleep += 1;
                    if (indexToSleep == 9)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        indexToSleep = 0;
                    }
                }

                #region parents
                //var items = db.ParentItems.GetAll().ToList();
                //var index = 0;
                //var indexToSleep = 0;
                //var rank = new Dictionary<string, string>();
                //var allNodes = new List<string>();
                //while (index < items.Count)
                //{
                //    var parents = items.Skip(index).Take(10).Select(i => i.ASIN).ToList();
                //    var resp = amazonApi.RetrieveRanks(parents);

                //    if (resp.Items != null && resp.Items.Any() && resp.Items[0].Item != null)
                //    {
                //        foreach (var item in items)
                //        {
                //            var el = resp.Items[0].Item.FirstOrDefault(i => i.ASIN == item.ASIN);
                //            if (el != null)
                //            {
                //                item.Rank = !string.IsNullOrEmpty(el.SalesRank) ? (int?)int.GetReportItems(el.SalesRank) : null;
                //                if (el.BrowseNodes != null && el.BrowseNodes.BrowseNode.Any())
                //                {
                //                    var nodes = el.BrowseNodes.BrowseNode.Select(n => n.BrowseNodeId);
                //                    foreach (var node in nodes)
                //                    {
                //                        if (!allNodes.Contains(node))
                //                        {
                //                            allNodes.Add(node);
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //    index += 10;
                //    indexToSleep += 1;
                //    if (indexToSleep == 9)
                //    {
                //        Thread.Sleep(TimeSpan.FromSeconds(5));
                //        indexToSleep = 0;
                //    }
                //}
                //var allAsins = items.Select(i => i.ASIN).ToList();
                //var topAsins = new List<Tuple<string, string, string, int>>();
                //foreach (var node in allNodes)
                //{
                //    var nodesRes = amazonApi.BrowseNodes(node);
                //    if (nodesRes.BrowseNodes != null && nodesRes.BrowseNodes.Any() && nodesRes.BrowseNodes[0].BrowseNode != null)
                //    {
                //        var exists = nodesRes.BrowseNodes[0].BrowseNode.Select(n => n.TopItemSet).ToList();
                //        if (exists.Any())
                //        {
                //            var exist = exists[0];
                //            if (exist != null)
                //            {
                //                    var itemSet = exist.First();
                //                    var asins = itemSet.TopItem.Select(iSet => new { iSet.ASIN }).ToList();
                //                for (var i = 0; i < asins.Count; i++)
                //                {
                //                    var asin = asins[i];
                //                    if (allAsins.Contains(asin.ASIN) &&
                //                        !topAsins.Any(t => t.Item1 == asin.ASIN && t.Item3 == node))
                //                    {
                //                        topAsins.Add(new Tuple<string, string, string, int>(
                //                            asin.ASIN,
                //                            nodesRes.BrowseNodes[0].BrowseNode[0].Name,
                //                            node,
                //                            i + 1));
                //                    }
                //                }
                //            }
                //        }
                //    }

                //}
                //foreach (var asin in topAsins)
                //{
                //    db.NodePositions.Add(new NodePosition
                //    {
                //        ASIN = asin.Item1,
                //        NodeName = asin.Item2,
                //        NodeIdentifier = asin.Item3,
                //        Position = asin.Item4,
                //        CreateDate = DateHelper.GetAppNowTime(),
                //        UpdateDate = DateHelper.GetAppNowTime()
                //    });
                //}
                //db.Commit();
                #endregion
            }
        }

        public void UpdateBuyBoxStatus(AmazonApi api)
        {
            using (var db = new UnitOfWork(_log))
            {
                var service = new BuyBoxService(_log, _dbFactory, _time);
                service.Update(api);
            }
        }

        public void ReadPriceInfo(AmazonApi api, string sku)
        {
            var items = api.GetMyPriceBySKU(new List<string>() { sku });
            _log.Info(items.Count.ToString());
        }

        public void ReadAllPriceInfo(AmazonApi api)
        {
            var ratingService = new RatingUpdater(_log, _time);
            var dbFactory = new DbFactory();
            using (var db = dbFactory.GetRWDb())
            {
                ratingService.UpdateMyPrice(api, db);
            }
        }

        public void UpdateAllRatings(AmazonApi api)
        {
            var ratingService = new RatingUpdater(_log, _time);
            var dbFactory = new DbFactory();
            using (var db = dbFactory.GetRWDb())
            {
                ratingService.UpdateRatingByProductApi(api, db);
            }
        }
    }
}
