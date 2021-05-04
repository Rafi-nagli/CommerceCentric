using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api.Exports;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Items;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.Exports;
using Amazon.Web.ViewModels.ExcelToAmazon;
using CsvHelper;
using CsvHelper.Configuration;

namespace Amazon.InventoryUpdateManual.Models
{
    public class BuildAmazonMultiListingExcel
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private IMarketCategoryService _categoryService;
        private CompanyDTO _company;
         
        public BuildAmazonMultiListingExcel(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            IMarketCategoryService categoryService,
            CompanyDTO company)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
            _company = company;
            _categoryService = categoryService;
        }
        
        public void BuildMultilistingUSFBAExcel(IList<FBAItemInfo> fbaItems)
        {
            var resultItems = new List<ExcelProductUSViewModel>();

            var marketplaceManager = new MarketplaceKeeper(_dbFactory, false);
            marketplaceManager.Init();
            
            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), _time, _log, _dbFactory, null)
                .GetApi(AccessManager.Company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);

            var filename = String.Empty;

            using (var db = _dbFactory.GetRWDb())
            {
                var parentASINList = fbaItems.Select(i => i.ParentASIN).Distinct().ToList();

                foreach (var parentASIN in parentASINList)
                {
                    var childFBAItems = fbaItems.Where(f => f.ParentASIN == parentASIN).ToList();

                    var newItems = ExcelProductUSViewModel.GetItemsFor(_log,
                        _time,
                        _categoryService,
                        htmlScraper,
                        api,
                        db,
                        _company,
                        parentASIN,
                        ExportToExcelMode.FBA,
                        childFBAItems,
                        MarketType.Amazon,
                        MarketplaceKeeper.AmazonComMarketplaceId,
                        UseStyleImageModes.Auto, 
                        out filename);
                    
                    resultItems.AddRange(newItems);
                }
            }

            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Files/Templates/Flat.File.Clothing-full.OneSheet.US.xls");
            var stream = ExcelHelper.ExportIntoFile(templateFile,
                "Template",
                resultItems);

            var outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("USFBAListings_{0}.xls", _time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss")));
            stream.Seek(0, SeekOrigin.Begin);
            using (FileStream file = new FileStream(outputFile, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                file.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }


        public void BuildMultilistingUSPrimeExcelByChildASIN(IList<string> usChildASINList)
        {
            var resultItems = new List<ExcelProductUSViewModel>();

            var marketplaceManager = new MarketplaceKeeper(_dbFactory, false);
            marketplaceManager.Init();

            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), _time, _log, _dbFactory, null)
                .GetApi(AccessManager.Company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId);

            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);

            var filename = String.Empty;

            using (var db = _dbFactory.GetRWDb())
            {
                var parentASINs = db.Items.GetAll().Where(i => usChildASINList.Contains(i.ASIN)
                                                               && i.Market == (int) MarketType.Amazon
                                                               &&
                                                               i.MarketplaceId ==
                                                               MarketplaceKeeper.AmazonComMarketplaceId)
                    .Select(i => i.ParentASIN)
                    .ToList();

                var usASINList = parentASINs.Distinct().ToList();

                foreach (var asin in usASINList)
                {
                    var newItems = ExcelProductUSViewModel.GetItemsFor(_log,
                        _time,
                        _categoryService,
                        htmlScraper,
                        api,
                        db,
                        _company,
                        asin,
                        ExportToExcelMode.FBP, 
                        null,
                        MarketType.Amazon,
                        MarketplaceKeeper.AmazonComMarketplaceId,
                        UseStyleImageModes.Auto,
                        out filename);

                    var resultList = new List<ExcelProductUSViewModel>();
                    foreach (var newItem in newItems)
                    {
                        if (newItem.StyleItemId.HasValue)
                        {
                            var styleItem =
                                db.StyleItemCaches.GetForStyleItemId(newItem.StyleItemId.Value).FirstOrDefault();
                            if (styleItem != null && styleItem.RemainingQuantity > 0)
                                resultList.Add(newItem);
                        }
                        else
                        {
                            //Parent record w/o StyleItemId
                            resultList.Add(newItem);
                        }
                    }

                    resultItems.AddRange(resultList);
                }
            }

            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Files/Templates/Flat.File.Clothing-full.OneSheet.US.xls");
            var stream = ExcelHelper.ExportIntoFile(templateFile,
                "Template",
                resultItems);

            var outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("USPrimeListings_{0}.xls", _time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss")));
            stream.Seek(0, SeekOrigin.Begin);
            using (FileStream file = new FileStream(outputFile, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                file.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }


        public void BuildMultilistingCAExcel(IList<string> usParentASINList)
        {
            var resultItems = new List<ExcelProductCAViewModel>();

            var marketplaceManager = new MarketplaceKeeper(_dbFactory, false);
            marketplaceManager.Init();
            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), _time, _log, _dbFactory, null)
                .GetApi(AccessManager.Company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonCaMarketplaceId);

            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);

            var filename = String.Empty;

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var parentAsin in usParentASINList)
                {
                    var newItems = ExcelProductCAViewModel.GetItemsFor(_log,
                        _time,
                        _categoryService,
                        htmlScraper,
                        api,
                        db,
                        _company,
                        parentAsin,
                        MarketType.Amazon,
                        MarketplaceKeeper.AmazonCaMarketplaceId,
                        UseStyleImageModes.Auto,
                        out filename);

                    var resultList = new List<ExcelProductCAViewModel>();
                    foreach (var newItem in newItems)
                    {
                        if (newItem.StyleItemId.HasValue)
                        {
                            var styleItem =
                                db.StyleItemCaches.GetForStyleItemId(newItem.StyleItemId.Value).FirstOrDefault();
                            if (styleItem != null && styleItem.RemainingQuantity > 0)
                                resultList.Add(newItem);
                        }
                        else
                        {
                            //Parent record w/o StyleItemId
                            resultList.Add(newItem);
                        }
                    }

                    resultItems.AddRange(resultList);
                }
            }

            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Files/Templates/Flat.File.Clothing.OneSheet.CA.xls");
            var stream = ExcelHelper.ExportIntoFile(templateFile,
                "Template",
                resultItems);

            var outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("CAListings_{0}.xls", _time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss")));
            stream.Seek(0, SeekOrigin.Begin);
            using (FileStream file = new FileStream(outputFile, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                file.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }

        public void BuildMultilistingUKExcel(IList<string> usASINList)
        {
            var resultItems = new List<ExcelProductUKViewModel>();

            var marketplaceManager = new MarketplaceKeeper(_dbFactory, false);
            marketplaceManager.Init();
            IMarketApi api = new MarketFactory(marketplaceManager.GetAll(), _time, _log, _dbFactory, null)
                .GetApi(AccessManager.Company.Id, MarketType.Amazon, MarketplaceKeeper.AmazonUkMarketplaceId);

            var htmlScraper = new HtmlScraperService(_log, _time, _dbFactory);

            var filename = String.Empty;

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var asin in usASINList)
                {
                    var newItems = ExcelProductUKViewModel.GetItemsFor(_log,
                        _time,
                        _categoryService,
                        htmlScraper,
                        api,
                        db,
                        _company,
                        asin,
                        MarketType.Amazon,
                        MarketplaceKeeper.AmazonComMarketplaceId,
                        UseStyleImageModes.Auto,
                        out filename);

                    var resultList = new List<ExcelProductUKViewModel>();
                    foreach (var newItem in newItems)
                    {
                        if (newItem.StyleItemId.HasValue)
                        {
                            var styleItem =
                                db.StyleItemCaches.GetForStyleItemId(newItem.StyleItemId.Value).FirstOrDefault();
                            if (styleItem != null && styleItem.RemainingQuantity > 0)
                                resultList.Add(newItem);
                        }
                        else
                        {
                            //Parent record w/o StyleItemId
                            resultList.Add(newItem);
                        }
                    }

                    resultItems.AddRange(resultList);
                }
            }

            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Files/Templates/Flat.File.Clothing.OneSheet.UK.xls");
            var stream = ExcelHelper.ExportIntoFile(templateFile,
                "Template",
                resultItems);

            var outputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Format("UKListings_{0}.xls", _time.GetAppNowTime().ToString("MM_dd_yyyy_hh_mm_ss")));
            stream.Seek(0, SeekOrigin.Begin);
            using (FileStream file = new FileStream(outputFile, FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                file.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }

        public IList<FBAItemInfo> ReadFBAInfo(string filePath)
        {
            StreamReader streamReader = new StreamReader(filePath);
            CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimFields = true,
            });

            var results = new List<FBAItemInfo>();
            while (reader.Read())
            {
                var isCreateFBA = reader.GetField("Create FBA listing") == "Y";
                if (!isCreateFBA)
                    continue;

                var qtyField = reader.GetField("Qty");
                var lengthField = reader.GetField("Lengt");

                if (String.IsNullOrEmpty(qtyField) 
                    || String.IsNullOrEmpty(lengthField))
                    continue;

                results.Add(new FBAItemInfo()
                {
                    ParentASIN = reader.GetField("Parent ASIN"),
                    Quantity = Int32.Parse(qtyField),
                    SKU = reader.GetField("SKU"),
                    PackageLength = decimal.Parse(lengthField),
                    PackageWidth = decimal.Parse(reader.GetField("Width")),
                    PackageHeight = decimal.Parse(reader.GetField("Depth")),
                });
            }

            return results;
        }
    }
}
