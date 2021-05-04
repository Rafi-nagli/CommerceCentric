using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.AutoCreateListings
{
    public class AutoCreateAmazonUSPrimeListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IItemHistoryService _itemHistoryService;

        public AutoCreateAmazonUSPrimeListingService(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IItemHistoryService itemHistoryService)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _itemHistoryService = itemHistoryService;

        }

        public void CreateListings()
        {
            var now = _time.GetAppNowTime();
            using (var db = _dbFactory.GetRWDb())
            {
                db.DisableValidation();
                db.DisableAutoDetectChanges();
                db.DisableProxyCreation();

                var existPrimeItems = (from i in db.Items.GetAll()
                                       join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                       where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                         && (l.IsPrime || l.SKU.Contains("-FBP"))
                                         && !l.IsRemoved
                                       select new
                                       {
                                           StyleItemId = i.StyleItemId,
                                           ParentASIN = i.ParentASIN,
                                           Quantity = l.RealQuantity
                                       }).ToList();

                var topStyles = (from sic in db.StyleItemCaches.GetAll()
                                 join st in db.Styles.GetAll() on sic.StyleId equals st.Id
                                 group sic by sic.StyleId into byStyle
                                 select new
                                 {
                                     StyleId = byStyle.Key,
                                     Quantity = byStyle.Sum(si => si.RemainingQuantity)
                                 }).ToList();

                //topStyles = topStyles.Where(st => st.Quantity > 100).ToList();

                topStyles = topStyles.OrderByDescending(st => st.Quantity)
                    .ToList();

                var styleIdToCopy = topStyles.Select(st => st.StyleId).ToList();

                var toCopyItems = (from i in db.Items.GetAll()
                                   join l in db.Listings.GetAll() on i.Id equals l.ItemId
                                   where i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                         && styleIdToCopy.Contains(i.StyleId)
                                         && !l.IsRemoved
                                         && !l.IsPrime
                                         && !l.IsFBA
                                         && !l.SKU.Contains("-FBP")
                                         && !l.SKU.Contains("-FBA")
                                         && l.RealQuantity > 5
                                   select new ItemDTO()
                                   {
                                       Id = i.Id,
                                       SKU = l.SKU,
                                       StyleId = i.StyleId,
                                       StyleItemId = i.StyleItemId,
                                       ParentASIN = i.ParentASIN
                                   })
                                .ToList();

                foreach (var itemToCopy in toCopyItems)
                {
                    var existPrime = existPrimeItems.FirstOrDefault(i => i.ParentASIN == itemToCopy.ParentASIN
                        && i.StyleItemId == itemToCopy.StyleItemId);
                    if (existPrime == null)
                    {
                        var dbItemToCopy = db.Items.GetAll().FirstOrDefault(i => i.Id == itemToCopy.Id);
                        var dbListingToCopy = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == itemToCopy.Id
                            && !l.IsRemoved);

                        var salePrice = (from sl in db.StyleItemSaleToListings.GetAll()
                                         join sm in db.StyleItemSaleToMarkets.GetAll() on sl.SaleToMarketId equals sm.Id
                                         join s in db.StyleItemSales.GetAll() on sm.SaleId equals s.Id
                                         where sl.ListingId == dbListingToCopy.Id
                                          && sm.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
                                          && !s.IsDeleted
                                          && (s.SaleStartDate.HasValue && s.SaleStartDate < now)
                                          && sm.SalePrice.HasValue
                                         select sm.SalePrice).FirstOrDefault();

                        //var dbViewItem = db.Items.GetAllViewActual().FirstOrDefault(i => i.Id == dbItemToCopy.Id);
                        //if (dbViewItem == null)
                        //{
                        //    _log.Info("dbViewItem = null");
                        //    continue;
                        //}
                        _log.Info("Copy listing, SKU=" + itemToCopy.SKU + ", price=" + dbListingToCopy.CurrentPrice + ", salePrice=" + salePrice);

                        var isOversizeTemplate = dbItemToCopy.OnMarketTemplateName == AmazonTemplateHelper.OversizeTemplate;

                        dbItemToCopy.Id = 0;
                        dbItemToCopy.ItemPublishedStatus = (int)PublishedStatuses.New;
                        dbItemToCopy.ItemPublishedStatusDate = null;
                        dbItemToCopy.ItemPublishedStatusBeforeRepublishing = null;
                        dbItemToCopy.ItemPublishedStatusFromMarket = null;
                        dbItemToCopy.ItemPublishedStatusFromMarketDate = null;
                        dbItemToCopy.ItemPublishedStatusReason = null;
                        dbItemToCopy.OnMarketTemplateName = AmazonTemplateHelper.PrimeTemplate;
                        dbItemToCopy.CreateDate = _time.GetAppNowTime();
                        dbItemToCopy.UpdateDate = _time.GetAppNowTime();
                        dbItemToCopy.CreatedBy = null;
                        dbItemToCopy.UpdatedBy = null;
                        db.Items.Add(dbItemToCopy);
                        db.Commit();

                        _itemHistoryService.AddRecord(dbItemToCopy.Id,
                            ItemHistoryHelper.SizeKey,
                            null,
                            "AutoCreateAmazonUSPrimeListingService.CreateListing",
                            dbItemToCopy.Size,
                            null,
                            null);

                        var newSKU = SkuHelper.AddPrimePostFix(dbListingToCopy.SKU);
                        var price = salePrice ?? dbListingToCopy.CurrentPrice;
                        dbListingToCopy.ItemId = dbItemToCopy.Id;
                        dbListingToCopy.Id = 0;
                        dbListingToCopy.ListingId = newSKU;
                        dbListingToCopy.IsPrime = true;
                        dbListingToCopy.CurrentPrice = PriceHelper.RoundToFloor99(isOversizeTemplate ? (price + 9.49M) : (price + 7.49M));
                        dbListingToCopy.ListingPriceFromMarket = null;
                        dbListingToCopy.AmazonCurrentPrice = null;
                        dbListingToCopy.AmazonCurrentPriceUpdateDate = null;
                        dbListingToCopy.AmazonRealQuantity = null;
                        dbListingToCopy.AmazonRealQuantityUpdateDate = null;
                        dbListingToCopy.PriceFromMarketUpdatedDate = null;
                        dbListingToCopy.SKU = newSKU;
                        dbListingToCopy.OpenDate = _time.GetAppNowTime();
                        dbListingToCopy.CreateDate = _time.GetAppNowTime();
                        dbListingToCopy.UpdateDate = _time.GetAppNowTime();
                        dbListingToCopy.CreatedBy = null;
                        dbListingToCopy.UpdatedBy = null;
                        db.Listings.Add(dbListingToCopy);
                        db.Commit();
                        _log.Info("Copyied to SKU=" + dbListingToCopy.SKU + ", newPrice=" + dbListingToCopy.CurrentPrice);
                    }
                }
            }
        }
    }
}
