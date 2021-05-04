using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO.Categories;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets.Magento;
using Amazon.Model.Implementation.Markets.Walmart.Feeds;
using CsvHelper;
using CsvHelper.Configuration;
using Jet.Api;
using Magento.Api.Wrapper;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallMagentoProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private ICacheService _cacheService;
        private IEmailService _emailService;
        private IBarcodeService _barcodeService;
        private IAutoCreateListingService _autoCreateListingService;

        public CallMagentoProcessing(ILogService log,
            ITime time,
            IEmailService emailService,
            ICacheService cacheService,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _cacheService = cacheService;
            _emailService = emailService;

            var itemHistoryService = new ItemHistoryService(_log, _time, _dbFactory);
            _barcodeService = new BarcodeService(log, time, dbFactory);
            _autoCreateListingService = new AutoCreateNonameListingService(_log, _time, dbFactory, cacheService, _barcodeService, _emailService, itemHistoryService, AppSettings.IsDebug);
        }

        public void LoadCustomCategories(string filePath)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                StreamReader streamReader = new StreamReader(filePath);
                CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    TrimFields = true,
                });
                //Weight,A,B,C,D,E,F,G,H1,H2,I,L,M,J,K,N,P
                //Product, Package, Weight, A, B, ...
                var filename = Path.GetFileName(filePath);

                while (reader.Read())
                {
                    var category1Name = reader.GetField<string>("Category");
                    var category2Name = reader.GetField<string>("Sub Category 1");
                    var category3Name = reader.GetField<string>("Sub Category 2");
                    var gender = reader.GetField<string>("Gender");
                    var itemStyle = reader.GetField<string>("Item Style");

                    if (!String.IsNullOrEmpty(category1Name))
                    {
                        var categoryPath = category1Name + " / " + category2Name + " / " + category3Name;
                        var categoryName = category1Name + " - " + category2Name + " - " + category3Name;
                        var customCategory = db.CustomCategories.GetAllAsDto().FirstOrDefault(c => c.CategoryPath == categoryPath);
                        if (customCategory == null)
                        {
                            var newCustomCategory = new Core.Entities.Categories.CustomCategory()
                            {
                                Name = categoryName,
                                CategoryPath = categoryPath,
                                Market = (int)MarketType.Magento,
                                Mode = 0,
                                CreateDate = _time.GetAppNowTime(),
                            };
                            db.CustomCategories.Add(newCustomCategory);
                            db.Commit();

                            db.CustomCategoryFilters.Add(new Core.Entities.Categories.CustomCategoryFilter()
                            {
                                CustomCategoryId = newCustomCategory.Id,
                                AttributeName = "Gender",
                                Operation = "Equals",
                                AttributeValues = gender,
                                CreateDate = _time.GetAppNowTime()
                            });
                            db.CustomCategoryFilters.Add(new Core.Entities.Categories.CustomCategoryFilter()
                            {
                                CustomCategoryId = newCustomCategory.Id,
                                AttributeName = "Item style",
                                Operation = "Equals",
                                AttributeValues = itemStyle,
                                CreateDate = _time.GetAppNowTime()
                            });
                            db.Commit();
                        }
                    }
                }

                db.Commit();
            }
        }

        public void CreateMagentoListings()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var styleIds = (from i in db.Items.GetAll()
                               join l in db.Listings.GetAll() on i.Id equals l.ItemId
                               join sic in db.StyleItemCaches.GetAll() on i.StyleItemId equals sic.Id
                               where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId                               
                                && !l.IsRemoved
                               select new {
                                    i.ParentASIN,
                                    i.StyleId,
                                    i.Market,
                                    i.MarketplaceId,
                                    sic.RemainingQuantity
                                })
                                .Distinct()
                                .OrderByDescending(i => i.RemainingQuantity)
                                .Take(1000)                          
                                .ToList();

                var existMarketStyleIds = db.Items.GetAll()
                    .Where(i => i.Market == (int)MarketType.Magento
                        && i.StyleId.HasValue)
                    .Select(i => i.StyleId)
                    .ToList();

                IList<MessageString> messages;
                foreach (var styleInfo in styleIds)
                {
                    if (existMarketStyleIds.Any(i => i == styleInfo.StyleId))
                        continue;

                    var model = _autoCreateListingService.CreateFromParentASIN(db,
                        styleInfo.ParentASIN,
                        styleInfo.Market,
                        styleInfo.MarketplaceId,
                        false,
                        false,
                        0,
                        out messages);

                    if (model == null)
                    {
                        _log.Info("Empty model");
                        continue;
                    }

                    model.Market = (int)MarketType.Magento;
                    model.MarketplaceId = "";

                    //if (model.Variations.Select(i => i.StyleId).Distinct().Count() != 1
                    //    && model.Variations.Select(i => i.StyleId).Distinct().Count() != 2)
                    //{
                    //    _log.Info("Parent ASIN is multilisting");
                    //    continue;
                    //}

                    _autoCreateListingService.PrepareData(model);
                    _autoCreateListingService.Save(model, null, db, _time.GetAppNowTime(), null);

                    var newItems = db.Items.GetAll().Where(i => i.Market == (int)MarketType.Magento
                        && i.ParentASIN == model.ASIN).ToList();
                    existMarketStyleIds.AddRange(newItems.Where(i => i.StyleId.HasValue).Select(i => i.StyleId).Distinct());
                }
            }
        }

        public void SyncAttributeOptions(Magento20MarketApi api)
        {
            var sync = new MagentoItemsSync(api,
                _dbFactory,
                _log,
                _time);

            sync.SyncAttributeOptions();
        }

        public void SubmitParentItems(Magento20MarketApi api, IList<string> skuList)
        {
            var sync = new MagentoItemsSync(api,
                _dbFactory,
                _log,
                _time);

            IList<int> itemIdList = null;
            
            if (skuList != null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    var query = from pi in db.ParentItems.GetAll()                                
                                where pi.Market == (int)MarketType.Magento
                                    && skuList.Contains(pi.ASIN)
                                select pi;
                    
                    itemIdList = query.Select(i => i.Id).ToList();
                }
            }
            else
            {
                using (var db = _dbFactory.GetRDb())
                {
                    itemIdList = db.ParentItems.GetAll()
                        .Where(i => i.Market == (int)MarketType.Magento)
                        .OrderBy(i => i.Id)
                        .Select(i => i.Id)
                        .ToList();
                }
            }


            _log.Info("ParentItems to update=" + itemIdList?.Count);
            //itemIdList = itemIdList.Skip(400).ToList();

            sync.SendParentItemUpdates(itemIdList);
        }

        public void SubmitItems(Magento20MarketApi api, IList<string> styleList)
        {
            var sync = new MagentoItemsSync(api,
                _dbFactory,
                _log,
                _time);

            IList<int> itemIdList = null;
            //using (var db = _dbFactory.GetRDb())
            //{
            //    //var styleIdList = db.StyleFeatureValues.GetAll().Select(fv => fv.StyleId).ToList();
            //    var fromDate = DateTime.Today.AddHours(13);
            //    var styleIdList = db.Styles.GetAll().Where(st => st.StyleID == "Citizen EM0420-54D")
            //        .Select(st => st.Id)
            //        .ToList();
            //        //db.Styles.GetAll().Where(st => st.LiteCountingDate > fromDate).Select(st => st.Id).ToList();
            //    itemIdList = db.Items.GetAll()
            //        .Where(i => i.StyleId.HasValue &&
            //            styleIdList.Contains(i.StyleId.Value) &&
            //            (i.ItemPublishedStatus == (int)PublishedStatuses.Published
            //            || i.ItemPublishedStatus == (int)PublishedStatuses.New
            //            || i.ItemPublishedStatus == (int)PublishedStatuses.PublishingErrors
            //            || i.ItemPublishedStatus == (int)PublishedStatuses.HasChanges) &&
            //            //i.ItemPublishedStatusDate < fromDate &&
            //            i.Market == (int)MarketType.Magento)
            //        .Select(i => i.Id).ToList();
            //}

            if (styleList != null)
            {
                using (var db = _dbFactory.GetRWDb())
                {
                    var query = from i in db.Items.GetAll()
                                join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                join st in db.Styles.GetAll() on i.StyleId equals st.Id
                                where l.Market == (int)MarketType.Magento
                                    && styleList.Contains(st.StyleID)
                                select i;

                    //var query = from i in db.Items.GetAll()
                    //            join st in db.Styles.GetAll() on i.StyleId equals st.Id
                    //            where i.Market == (int)MarketType.Magento
                    //                && st.DropShipperId == DSHelper.UltraluxId
                    //            select i;

                    itemIdList = query.Select(i => i.Id).ToList();
                }
            }
            else
            {
                using (var db = _dbFactory.GetRDb())
                {
                    itemIdList = db.Items.GetAll()
                        .Where(i => i.Market == (int)MarketType.Magento)
                        .OrderBy(i => i.Id)
                        .Select(i => i.Id)
                        .ToList();
                }
            }


            _log.Info("Items to update=" + itemIdList?.Count);
            //itemIdList = itemIdList.Skip(400).ToList();

            sync.SendItemUpdates(itemIdList);
        }

        public void SubmitInventory(Magento20MarketApi api)
        {
            var sync = new MagentoItemsSync(api,
                _dbFactory,
                _log,
                _time);

            sync.SendInventoryUpdates();
        }

        public void SubmitPrice(Magento20MarketApi api)
        {
            var sync = new MagentoItemsSync(api,
                _dbFactory,
                _log,
                _time);

            sync.SendPriceUpdates();
        }


        //public void SubmitItems(MagentoMarketApi api)
        //{
        //    var sync = new MagentoItemsSync(api,
        //        _dbFactory,
        //        _log,
        //        _time);

        //    sync.SendItemUpdates();
        //}

        //public void SubmitInventory(MagentoMarketApi api)
        //{
        //    var sync = new MagentoItemsSync(api,
        //        _dbFactory,
        //        _log,
        //        _time);

        //    sync.SendInventoryUpdates();
        //}

        //public void SubmitPrice(MagentoMarketApi api)
        //{
        //    var sync = new MagentoItemsSync(api,
        //        _dbFactory,
        //        _log,
        //        _time);

        //    sync.SendPriceUpdates();
        //}
    }
}
