using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Web.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace Amazon.Web.ViewModels.Groupon
{
    public class ActualizeGrouponQtyFeedViewModel
    {
        public string FileName { get; set; }
        public string MarketplaceId { get; set; }

        private class GrouponItemInfo
        {
            public int ItemId { get; set; }
            public long? ListingId { get; set; }
            public string SKU { get; set; }
            public string Barcode { get; set; }

            public decimal Price { get; set; }
            public decimal OldPrice { get; set; }
            public decimal? OldShippingPrice { get; set; }

            public int? OldItemPublishedStatus { get; set; }

            public int Qty { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }

            public bool ExistOnMarket { get; set; }
        }

        public IList<MessageString> Validate()
        {
            return new List<MessageString>();
        }

        private int? FindIndex(string[] values, string value)
        {
            var index = Array.IndexOf(values, value);
            if (index >= 0)
                return index;

            return null;
        }

        public CallMessagesResult<string> UpdateFeed(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IAutoCreateListingService autoCreateListingService)
        {
            var marketplaceId = MarketplaceId;
            var sourceFeed = Path.Combine(UrlHelper.GetActualizeGrouponQtyFeedPath(), FileName);
            var qtyThreshold = 10;

            //Read prices
            var allDSQtyItems = new List<GrouponItemInfo>();
            using (var db = dbFactory.GetRWDb())
            {
                var allDSQtyItemsQuery = (from st in db.Styles.GetAll()
                                 join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                 join sic in db.StyleItemCaches.GetAll() on si.Id equals sic.Id
                                 join vi in db.Items.GetAllViewAsDto() on si.Id equals vi.StyleItemId into withItems
                                 from vi in withItems.DefaultIfEmpty()
                                 join i in db.Listings.GetAll() on vi.Id equals i.Id into withListings
                                 from i in withListings.DefaultIfEmpty()
                                 join pi in db.ParentItems.GetAll() on new { vi.ParentASIN, vi.Market, vi.MarketplaceId } equals new { ParentASIN = pi.ASIN, pi.Market, pi.MarketplaceId } into withParent
                                 from pi in withParent.DefaultIfEmpty()
                                 where vi.Market == (int)MarketType.Groupon
                                    && (vi.MarketplaceId == marketplaceId
                                    || String.IsNullOrEmpty(marketplaceId))
                                    && !st.Deleted
                                 select new GrouponItemInfo()
                                 {
                                     ItemId = vi.Id,
                                     ListingId = vi.ListingEntityId,
                                     SKU = vi.SKU,
                                     Barcode = vi.Barcode,

                                     OldPrice = vi.CurrentPrice,
                                     OldShippingPrice = i.ShippingPriceFromMarket,
                                     OldItemPublishedStatus = vi.PublishedStatus,

                                     Qty = (st.OnHold
                                         || st.SystemOnHold
                                         || vi.OnHold
                                         || pi.OnHold
                                         || sic.RemainingQuantity <= qtyThreshold) ? 0 : (vi.RealQuantity),
                                 });
                allDSQtyItems = allDSQtyItemsQuery.ToList();
            }

            //Read prices dest file
            var dataList = new List<string[]>();
            using (StreamReader streamReader = new StreamReader(sourceFeed))
            {
                using (CsvReader reader = new CsvReader(streamReader, new CsvConfiguration
                {
                    HasHeaderRecord = false,
                    Delimiter = ",",
                    TrimFields = true,
                }))
                {
                    while (reader.Read())
                    {
                        dataList.Add(reader.CurrentRecord);
                    }
                }
            }

            var headers = dataList[0];
            var skuIndex = FindIndex(headers, "Vendor SKU") ?? FindIndex(headers, "Merchant SKU");
            var barcodeIndex = FindIndex(headers, "Product Identifier");
            var priceIndex = FindIndex(headers, "Unit Price") ?? FindIndex(headers, "Sell Price");
            var shippingPriceIndex = FindIndex(headers, "Shipping Cost") ?? FindIndex(headers, "Customer Ship Fee");
            var qtyIndex = FindIndex(headers, "Quantity");
            
            //Create Groupon Listings
            foreach (var dataItem in dataList.Skip(1))
            {
                var sku = skuIndex.HasValue ? PrepareValue(dataItem[skuIndex.Value]) : null;
                var barcode = barcodeIndex.HasValue ?  PrepareValue(dataItem[barcodeIndex.Value]) : null;
                var price = priceIndex.HasValue ? StringHelper.TryGetDecimal(PrepareValue(dataItem[priceIndex.Value])) : null;
                var shippingPrice = shippingPriceIndex.HasValue ? StringHelper.TryGetDecimal(PrepareValue(dataItem[shippingPriceIndex.Value])) : null;

                var qtyInfo = allDSQtyItems.FirstOrDefault(q => q.SKU == sku || q.SKU.TrimStart('0') == sku);
                if (qtyInfo == null)
                    qtyInfo = allDSQtyItems.FirstOrDefault(q => q.Barcode == barcode);

                if (qtyInfo == null)
                {
                    //Create listing
                    if (!String.IsNullOrEmpty(sku) && price.HasValue)
                    {
                        CreateListing(dbFactory, 
                            log, 
                            autoCreateListingService, 
                            marketplaceId,
                            sku, 
                            barcode, 
                            price.Value, 
                            shippingPrice, 
                            time.GetAppNowTime());
                    }
                }
                else
                {
                    if (price.HasValue)
                        UpdateListing(dbFactory, log, qtyInfo, price.Value, shippingPriceIndex.HasValue, shippingPrice, time.GetAppNowTime());
                    qtyInfo.ExistOnMarket = true;
                }
            }

            //Mark not exists as deleted
            var notUpdatedItemIds = allDSQtyItems.Where(i => !i.ExistOnMarket).Select(i => i.ItemId).ToList();

            using (var db = dbFactory.GetRWDb())
            {
                var dbListingsToRemove = db.Listings
                    .GetAll()
                    .Where(l => notUpdatedItemIds.Contains(l.ItemId))
                    .ToList();

                var dbItemsToRemove = db.Items
                    .GetAll()
                    .Where(l => notUpdatedItemIds.Contains(l.Id)
                        && (l.ItemPublishedStatus == (int)PublishedStatuses.Published
                            || l.ItemPublishedStatus == (int)PublishedStatuses.None)
                        && l.IsExistOnAmazon == true)
                    .ToList();
                log.Info("Mark as unpublished Listings: " + dbListingsToRemove.Count() + ", Items: " + dbItemsToRemove.Count());
                foreach (var toRemove in dbListingsToRemove)
                {
                    toRemove.AmazonRealQuantity = 0;
                    toRemove.AmazonRealQuantityUpdateDate = time.GetAppNowTime();
                }
                foreach (var toRemove in dbItemsToRemove)
                {
                    toRemove.IsExistOnAmazon = false;
                    toRemove.ItemPublishedStatusBeforeRepublishing = toRemove.ItemPublishedStatus;
                    toRemove.ItemPublishedStatus = (int)PublishedStatuses.PublishedInactive;
                    toRemove.ItemPublishedStatusReason = "From Cost&Quantity feed";
                    toRemove.ItemPublishedStatusDate = time.GetAppNowTime();
                }
                db.Commit();
            }

            //Add pices
            foreach (var dataItem in dataList.Skip(1))
            {
                var sku = PrepareValue(dataItem[skuIndex.Value]);
                var barcode = PrepareValue(dataItem[barcodeIndex.Value]);
                var qtyInfo = allDSQtyItems.FirstOrDefault(q => q.SKU == sku || q.SKU.TrimStart('0') == sku);
                if (qtyInfo == null)
                    qtyInfo = allDSQtyItems.FirstOrDefault(q => q.Barcode == barcode);
                
                if (qtyInfo != null)
                {
                    dataItem[qtyIndex.Value] = (Math.Max(0, qtyInfo.Qty)).ToString();
                }
                else
                {
                    dataItem[qtyIndex.Value] = "0";
                }
            }

            //Write updates
            var newFilePath = Path.Combine(Path.GetDirectoryName(sourceFeed), Path.GetFileNameWithoutExtension(sourceFeed)
                + "-updated-at-" + DateHelper.GetAppNowTime().ToString("yyyyMMddHHmm") + ".csv");
            using (var output = File.Create(newFilePath))
            {
                using (TextWriter writer = new StreamWriter(output))
                {
                    var csv = new CsvWriter(writer);
                    csv.Configuration.Encoding = Encoding.UTF8;

                    foreach (var item in dataList)
                    {
                        foreach (var field in item)
                            csv.WriteField(field);
                        csv.NextRecord();
                    }

                    writer.Flush();
                }
            }

            var newFileName = Path.GetFileName(newFilePath);

            return new CallMessagesResult<string>()
            {
                Data = UrlHelper.GetActualizeGrouponQtyUrl(newFileName).Trim("~".ToCharArray())
            };
        }

        private void UpdateListing(IDbFactory dbFactory,
            ILogService log,
            GrouponItemInfo info,
            decimal newPrice,
            bool hasShippingPrice,
            decimal? newShippingPrice,
            DateTime when)
        {
            using (var db = dbFactory.GetRWDb())
            {
                if (info.OldPrice != newPrice 
                    || (hasShippingPrice && info.OldShippingPrice != newShippingPrice))
                {
                    log.Info("Price changed: SKU=" + info.SKU + ", price=" + info.OldPrice + "=>" + newPrice 
                        + (hasShippingPrice ? ", shipping price=" + info.OldShippingPrice + "=>" + newShippingPrice : ""));
                    db.Listings.TrackItem(new Listing()
                        {
                            Id = info.ListingId.Value,
                            CurrentPrice = newPrice,
                            ShippingPriceFromMarket = newShippingPrice,
                            AmazonCurrentPrice = newPrice,
                            AmazonCurrentPriceUpdateDate = when,
                        },
                        hasShippingPrice ? 
                            new List<Expression<Func<Listing, object>>>()
                            {
                                l => l.CurrentPrice,
                                l => l.ShippingPriceFromMarket,
                                l => l.AmazonCurrentPrice,
                                l => l.AmazonCurrentPriceUpdateDate
                            } 
                            : new List<Expression<Func<Listing, object>>>()
                            {
                                l => l.CurrentPrice,                     
                                l => l.AmazonCurrentPrice,
                                l => l.AmazonCurrentPriceUpdateDate
                            });
                }

                if (info.OldItemPublishedStatus != (int)PublishedStatuses.Published)
                {
                    log.Info("ItemStatus changed: SKU=" + info.SKU + ", " + info.OldItemPublishedStatus + "=>" + (int)PublishedStatuses.Published);
                    db.Items.TrackItem(new Item()
                    {
                        Id = info.ItemId,
                        ItemPublishedStatus = (int)PublishedStatuses.Published,
                        ItemPublishedStatusDate = when
                    },
                    new List<Expression<Func<Item, object>>>()
                    {
                        i => i.ItemPublishedStatus,
                        i => i.ItemPublishedStatusDate
                    });
                }
                db.Commit();
            }
        }

        private void CreateListing(IDbFactory dbFactory,
            ILogService log,
            IAutoCreateListingService listingCreateService,
            string marketplaceId,
            string sku,
            string barcode,
            decimal price,
            decimal? shippingPrice,
            DateTime when)
        {
            var market = MarketType.Groupon;

            using (var db = dbFactory.GetRWDb())
            {
                var dbStyle = db.Styles.GetAll().FirstOrDefault(st => st.StyleID == sku && !st.Deleted);

                if (dbStyle == null)
                {
                    var dbStyleCandidates = (from st in db.Styles.GetAll()
                                             join si in db.StyleItems.GetAll() on st.Id equals si.StyleId
                                             join sib in db.StyleItemBarcodes.GetAll() on si.Id equals sib.StyleItemId
                                             where sib.Barcode == barcode
                                             && !st.Deleted
                                             select st).ToList();

                    if (dbStyleCandidates.Count() > 1)
                    {
                        log.Error("Multiple style by barcode: " + barcode + ", sku=" + sku);
                    }
                    if (dbStyleCandidates.Count() == 1)
                    {
                        dbStyle = dbStyleCandidates[0];
                    }
                }

                //Check other marketplaces
                if (dbStyle == null)
                {
                    var dbListing = (from l in db.Listings.GetAll()
                                     join i in db.Items.GetAll() on l.ItemId equals i.Id
                                     where l.SKU == sku
                                         && !l.IsRemoved
                                         && i.StyleItemId.HasValue
                                         && i.StyleId.HasValue
                                     orderby l.Market ascending
                                     select i)
                                    .FirstOrDefault();

                    if (dbListing != null)
                    {
                        dbStyle = db.Styles.Get(dbListing.StyleId.Value);
                    }
                }

                if (dbStyle != null)
                {
                    IList<MessageString> messages = new List<MessageString>();
                    log.Info("Create listing for style: " + dbStyle.StyleID);

                    var model = listingCreateService.CreateFromStyle(db,
                            dbStyle.StyleID,
                            price,
                            MarketType.Groupon,
                            marketplaceId,
                            out messages);

                    model.Variations.ForEach(v => v.CurrentPrice = price);
                    model.Variations.ForEach(v => v.ShippingPriceFromMarket = shippingPrice);

                    listingCreateService.Save(model,
                        "",
                        db,
                        when,
                        null);
                }
                else
                {
                    log.Info("Missing SKU=" + sku + ", barcode=" + barcode);
                }
            }
        }

        private string PrepareValue(string sku)
        {
            return StringHelper.Trim(sku, "=/\\\"\t ".ToCharArray());
        }
    }
}