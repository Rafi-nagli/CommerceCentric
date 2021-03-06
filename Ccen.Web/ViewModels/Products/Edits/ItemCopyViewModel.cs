using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.DTO;
using Amazon.Model.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Products.Edits
{
    public class ItemCopyViewModel
    {
        public int Id { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string ParentASIN { get; set; }

        public IList<ItemMarketViewModel> Marketplaces { get; set; }

        public ItemCopyViewModel()
        {

        }

        public void Init(IUnitOfWork db,
            int id)
        {
            var parentItem = db.ParentItems.GetAll().FirstOrDefault(pi => pi.Id == id);
            Id = id;
            ParentASIN = parentItem.ASIN;
            Market = parentItem.Market;
            MarketplaceId = parentItem.MarketplaceId;
            
            var marketplaces = db.Marketplaces.GetAll().ToList().OrderBy(m => m.SortOrder).ToList();

            var items = db.Items.GetAll()
                .Where(i => i.ParentASIN == ParentASIN
                    && i.Market == (int)Market
                    && (i.MarketplaceId == MarketplaceId || String.IsNullOrEmpty(MarketplaceId)))
                .ToList();

            var styleIdList = items.Where(i => i.StyleId.HasValue).Select(i => i.StyleId.Value).ToList();
            var allItems = db.Items.GetAll().Where(i => i.StyleId.HasValue && styleIdList.Contains(i.StyleId.Value)).ToList();

            var results = new List<ItemMarketViewModel>();
            foreach (var marketplace in marketplaces)
            {
                if (marketplace.Market == (int)Market
                    && (marketplace.MarketplaceId == MarketplaceId || String.IsNullOrEmpty(MarketplaceId)))
                    continue;

                results.Add(new ItemMarketViewModel()
                {
                    Market = marketplace.Market,
                    MarketplaceId = marketplace.MarketplaceId,
                    IsAlreadyExists = allItems.Any(i => i.Market == marketplace.Market
                        && (i.MarketplaceId == marketplace.MarketplaceId || String.IsNullOrEmpty(marketplace.MarketplaceId))),
                    IsSelected = false
                });
            }

            ParentASIN = ParentASIN;
            Market = (int)Market;
            MarketplaceId = MarketplaceId;
            Marketplaces = results;
        }


        public static void CopyToMarketplaces(IUnitOfWork db,
           ICacheService cache,
           IBarcodeService barcodeService,
           ISystemActionService actionService,
           IAutoCreateListingService autoCreateListingService,
           IItemHistoryService itemHistoryService,
           int id,
           DateTime when,
           long? by,
           IList<ItemMarketViewModel> toMarketplaces,
           out IList<MessageString> messages)
        {
            var parent = db.ParentItems.GetAsDTO(id);
            messages = new List<MessageString>();

            foreach (var toMarketplace in toMarketplaces)
            {
                var model = ItemEditViewModel.CreateFromParentASIN(db,
                    autoCreateListingService,
                    parent.ASIN,
                    parent.Market,
                    parent.MarketplaceId,
                    false, //NOTE: false - ex.: exclude to copy FBP to Walmart
                    out messages);

                model.Id = null;
                model.Market = toMarketplace.Market;
                model.MarketplaceId = toMarketplace.MarketplaceId;
                //model.OnHold = true;

                var parentBaseASIN = SkuHelper.RemoveSKULastIndex(model.ASIN);
                var parentIndex = 0;
                while (db.ParentItems.GetAsDTO(parentBaseASIN + ((parentIndex == 0) ? "" : "-" + parentIndex), (MarketType)toMarketplace.Market, toMarketplace.MarketplaceId) != null)
                    parentIndex++;
                var parentSKU = parentBaseASIN + ((parentIndex == 0) ? "" : "-" + parentIndex);

                var forceReplace = model.VariationList.Any(s => (s.Size ?? "").Contains("/"));

                model.ASIN = parentSKU;

                foreach (var item in model.VariationList)
                {
                    item.Id = null;

                    if (model.Market == (int)MarketType.Walmart
                        || model.Market == (int)MarketType.WalmartCA)
                    {
                        item.Barcode = null;
                        item.AutoGeneratedBarcode = true;
                    }

                    if (item.StyleItemId.HasValue)
                    {
                        var sourceUSDPrice = item.Price;
                        var fromCurrency = PriceHelper.GetCurrencyAbbr((MarketType)parent.Market, parent.MarketplaceId);
                        if (fromCurrency != PriceHelper.USDSymbol)
                            sourceUSDPrice = PriceHelper.ConvertToUSD(item.Price, fromCurrency);

                        var rateForMarketplace = RateHelper.GetRatesByStyleItemId(db, item.StyleItemId.Value);
                        var newPrice = RateHelper.CalculateForMarket((MarketType)toMarketplace.Market,
                            toMarketplace.MarketplaceId,

                            sourceUSDPrice,
                            rateForMarketplace[MarketplaceKeeper.AmazonComMarketplaceId],
                            rateForMarketplace[MarketplaceKeeper.AmazonCaMarketplaceId],
                            rateForMarketplace[MarketplaceKeeper.AmazonUkMarketplaceId],
                            rateForMarketplace[MarketplaceKeeper.AmazonAuMarketplaceId],
                            RateService.GetMarketShippingAmount(MarketType.Amazon, MarketplaceKeeper.AmazonComMarketplaceId),
                            RateService.GetMarketShippingAmount((MarketType)toMarketplace.Market, toMarketplace.MarketplaceId),
                            RateService.GetMarketExtraAmount((MarketType)toMarketplace.Market, toMarketplace.MarketplaceId));

                        if (newPrice.HasValue)
                            item.Price = newPrice.Value;
                    }

                    if (db.Listings.CheckForExistenceSKU(item.SKU,
                        (MarketType)toMarketplace.Market,
                        toMarketplace.MarketplaceId))
                    {
                        var baseSKU = item.StyleString + "-" + SizeHelper.PrepareSizeForSKU(item.StyleSize, forceReplace);
                        var index = parentIndex;

                        while (db.Listings.CheckForExistenceSKU(SkuHelper.SetSKUMiddleIndex(baseSKU, index),
                            (MarketType)toMarketplace.Market,
                            toMarketplace.MarketplaceId))
                            index++;

                        item.SKU = SkuHelper.SetSKUMiddleIndex(baseSKU, index);                        
                    }
                }

                model.Save(db,
                            cache,
                            barcodeService,
                            actionService,
                            itemHistoryService,
                            when,
                            by);
            }
        }
    }
}