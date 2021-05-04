using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Amazon.Api.Feeds;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Feeds;
using Amazon.DTO.Listings;
using Amazon.Model.Models;
using Amazon.Utils;
using CsvHelper;


namespace Amazon.Model.Implementation.Markets.Amazon.Feeds
{
    public class ItemPriceRuleUpdater : BaseFeedUpdater
    {
        //protected IList<Listing> Listings { get; set; }

        protected override AmazonFeedType Type
        {
            get { return AmazonFeedType.PriceRule; }
        }

        protected override string AmazonFeedName
        {
            get { return "_MARS_AUTOMATE_PRICING_FEED_"; }
        }

        public ItemPriceRuleUpdater(ILogService log,
            ITime time,
            IDbFactory dbFactory) : base(log, time, dbFactory)
        {

        }


        public class PriceRuleLine
        {
            public int Id { get; set; }
            public string SKU { get; set; }
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public string RuleName { get; set; }
            public string RuleAction { get; set; }
        }

        protected override DocumentInfo ComposeDocument(IUnitOfWork db,
            long companyId,
            MarketType market,
            string marketplaceId,
            IList<string> asinList)
        {
            Log.Info("Get listings for price rule update");

            var toDate = Time.GetAppNowTime().AddHours(-30);
            IList<ItemDTO> dtoItems;


            if (asinList == null || !asinList.Any())
            {
                var requestInfoes = db.SystemActions.GetAllAsDto()
                    .Where(a => a.Type == (int)SystemActionType.UpdateOnMarketProductPriceRule
                                    && a.Status != (int)SystemActionStatus.Done)
                    .ToList();

                var requestedSKUs = requestInfoes.Select(i => i.Tag).ToList();
                dtoItems = (from i in db.Items.GetAllViewAsDto()
                            where requestedSKUs.Contains(i.SKU)
                                && i.PublishedStatus == (int)PublishedStatuses.Published
                                && i.Market == (int)market
                                && i.RealQuantity > 0
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                            select i).ToList();

                foreach (var dtoItem in dtoItems)
                {
                    var requestInfo = requestInfoes.FirstOrDefault(i => i.Tag == dtoItem.SKU);
                    dtoItem.Id = (int)(requestInfo?.Id ?? 0);
                }
            }
            else
            {
                dtoItems = db.Items.GetAllViewAsDto()
                    .Where(i => i.Market == (int)market
                                && (String.IsNullOrEmpty(marketplaceId) || i.MarketplaceId == marketplaceId)
                                && i.StyleItemId.HasValue
                                && i.StyleId.HasValue
                                && asinList.Contains(i.SKU))
                    .ToList();
                dtoItems.ForEach(i => i.Id = 0);
            }

            if (dtoItems.Any())
            {
                var listingIdList = dtoItems.Select(i => i.ListingEntityId).Distinct().ToList();
                var styleItemIdList = dtoItems.Where(i => i.StyleItemId.HasValue).Select(i => i.StyleItemId).Distinct().ToList();
                var saleList = db.StyleItemSaleToListings.GetAllListingSaleAsDTO().Where(s => listingIdList.Contains(s.ListingId)).ToList();
                var styleItems = db.StyleItems.GetAll().Where(si => styleItemIdList.Contains(si.Id)).ToList();
                var listings = db.Listings.GetAll().Where(l => listingIdList.Contains(l.Id)).ToList();

                var priceLines = new List<PriceRuleLine>();
                var index = 0;

                var priceToSend = new Dictionary<string, decimal>();

                foreach (var dtoItem in dtoItems)
                {
                    if (!dtoItem.StyleItemId.HasValue)
                        continue;

                    var listing = listings.FirstOrDefault(l => l.Id == dtoItem.ListingEntityId);
                    var styleItem = styleItems.FirstOrDefault(si => si.Id == dtoItem.StyleItemId.Value);

                    if (styleItem == null)
                        continue;

                    if (priceToSend.ContainsKey(listing.SKU))
                        continue;

                    index++;
                    Log.Info("add listing, index=" + index + ", listing=" + listing);

                    ViewListingSaleDTO sale = saleList.FirstOrDefault(s => s.ListingId == listing.Id);
                    if (sale != null)
                    {
                        sale.SaleStartDate = sale.SaleStartDate ?? sale.ListingSaleCreateDate;
                        sale.SaleEndDate = sale.SaleEndDate ?? Time.GetAppNowTime().Add(SaleManager.DefaultSalesPeriod);
                        Log.Info("sale: salePrice=" + sale.SalePrice
                                 + ", maxPiecesOnSale=" + sale.MaxPiecesOnSale
                                 + ", currentPiecesSaled=" + sale.PiecesSoldOnSale
                                 + ", saleStartDate=" + sale.SaleStartDate
                                 + ", saleEndDate=" + sale.SaleEndDate);
                    }

                    var now = Time.GetAppNowTime();

                    var saleOn = sale != null
                        && sale.SaleStartDate <= now
                        && (sale.SalePrice.HasValue || sale.SalePercent.HasValue);

                    var businessPrice = listing.AutoAdjustedPrice ?? listing.CurrentPrice;
                    var currentPrice = listing.AutoAdjustedPrice ?? listing.CurrentPrice;

                    decimal? salePrice = null;
                    if (sale != null && sale.SalePrice.HasValue)
                        salePrice = sale.SalePrice.Value;
                    if (sale != null && sale.SalePercent.HasValue)
                        salePrice = currentPrice - currentPrice * sale.SalePercent.Value / (decimal)100;

                    if (saleOn)
                        businessPrice = salePrice.Value;
                    Log.Info("Current price=" + listing.CurrentPrice + ", Business price=" + businessPrice + ", Adjusted price=" + listing.AutoAdjustedPrice);

                    if (saleOn)
                    {
                        if (sale.SalePrice > currentPrice)
                        {
                            Log.Info("SalePrice > CurrentPrice, new currentPrice=" + currentPrice + " => " + salePrice + ", sale is OFF");

                            currentPrice = salePrice.Value;
                            saleOn = false;
                            sale = null;
                        }
                    }

                    var ruleName = "M004";
                    var ruleAction = styleItem.MinPrice.HasValue && styleItem.MaxPrice.HasValue ? "START" : "STOP"; //STOP
                    var minPrice = styleItem.MinPrice ?? 0;
                    var maxPrice = styleItem.MaxPrice ?? 0;

                    priceLines.Add(new PriceRuleLine()
                    {
                        Id = dtoItem.Id,
                        SKU = listing.SKU,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        RuleName = ruleName,
                        RuleAction = ruleAction
                    });

                    //listing.MessageIdentifier = index;

                    priceToSend.Add(listing.SKU, listing.CurrentPrice);
                }
                Log.Info("Compose feed");
                //var merchant = db.Companies.Get(companyId).AmazonFeedMerchantIdentifier;
                var document = ComposeFeed(priceLines);


                return new DocumentInfo
                {
                    TextDocument = document,
                    NodesCount = index,
                    FeedItems = priceLines.Select(i => new FeedItemDTO()
                    {
                        MessageId = 0,
                        ItemId = i.Id, 
                    }).ToList()
                };
            }
            return null;
        }

        protected string ComposeFeed(IList<PriceRuleLine> lines)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.Configuration.Delimiter = "\t";
                        //csv.Configuration.QuoteAllFields = true;

                        csv.WriteField("AutomatePricing-1.0");
                        csv.WriteField("The top 3 rows are for Amazon use only, Do not modify or delete the top 3 rows");
                        csv.WriteField("");
                        csv.WriteField("");
                        csv.WriteField("");
                        csv.NextRecord();

                        csv.WriteField("SKU");
                        csv.WriteField("Minimum Seller Allowed Price");
                        csv.WriteField("Maximum Seller Allowed Price");
                        csv.WriteField("Rule Name");
                        csv.WriteField("Rule Action");
                        csv.NextRecord();
                        
                        csv.WriteField("sku");
                        csv.WriteField("minimum-seller-allowed-price");
                        csv.WriteField("maximum-seller-allowed-price");
                        csv.WriteField("rule-name");
                        csv.WriteField("rule-action");
                        csv.NextRecord();

                        foreach (var line in lines)
                        {
                            csv.WriteField(line.SKU);
                            csv.WriteField(line.MinPrice);
                            csv.WriteField(line.MaxPrice);
                            csv.WriteField(line.RuleName);
                            csv.WriteField(line.RuleAction);
                            csv.NextRecord();
                        }
                    }
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        protected override void UpdateEntitiesBeforeSubmitFeed(IUnitOfWork db, long feedId)
        {

        }

        protected override void UpdateEntitiesAfterResponse(long feedId,
            IList<FeedResultMessage> errorList)
        {
            DefaultActionUpdateEntitiesAfterResponse(feedId, errorList, ItemAdditionFields.UpdatePriceRuleError, 3);
        }
    }
}
