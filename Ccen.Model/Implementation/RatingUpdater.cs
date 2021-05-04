using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DAL;
using log4net;

namespace Amazon.Model.Implementation
{
    public class RatingUpdater
    {
        private ILogService _log;
        private ITime _time;

        public RatingUpdater(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        private class TopNodeInfo
        {
            public string ASIN { get; set; }
            public string BrowseNodeId { get; set; }
            public string Name { get; set; }
            public int Index { get; set; }
        }

        public void UpdateMyPrice(AmazonApi api, IUnitOfWork db)
        {
            var lastUpdate = _time.GetUtcTime().AddHours(-12);

            var allItems = db.Listings
                    .GetAll()
                    .Where(p => p.Market == (int)api.Market
                        && p.MarketplaceId == api.MarketplaceId
                        && !p.IsRemoved
                        && (!p.PriceFromMarketUpdatedDate.HasValue
                        || p.PriceFromMarketUpdatedDate < lastUpdate))
                    .ToList();
            var index = 0;
            var stepSize = 20;

            var stepSleeper = new StepSleeper(TimeSpan.FromSeconds(1), 5);
            while (index < allItems.Count)
            {
                try
                {
                    var itemsToUpdate = allItems.Skip(index).Take(stepSize).ToList();
                    var skuList = itemsToUpdate.Select(i => i.SKU).ToList();
                    var items = api.GetMyPriceBySKU(skuList);

                    foreach (var item in items)
                    {
                        var toUpdate = itemsToUpdate.FirstOrDefault(pi => pi.SKU == item.SKU);
                        if (toUpdate != null)
                        {
                            toUpdate.ListingPriceFromMarket = item.ListingPriceFromMarket;
                            toUpdate.ShippingPriceFromMarket = item.ShippingPriceFromMarket;
                            toUpdate.ReqularPriceFromMarket = item.ReqularPriceFromMarket;
                            toUpdate.PriceFromMarketUpdatedDate = _time.GetUtcTime();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Error when updating rank for index: " + index, ex);
                }
                index += stepSize;

                db.Commit();

                stepSleeper.NextStep();
            }
        }

        public void UpdateLowestPrice(AmazonApi api, IUnitOfWork db)
        {
            var allItems = db.Listings.GetAll().Where(p => p.Market == (int)api.Market
                && p.MarketplaceId == api.MarketplaceId
                && !p.IsRemoved).ToList();
            var index = 0;
            var stepSize = 20;

            var stepSleeper = new StepSleeper(TimeSpan.FromSeconds(1), 10);
            while (index < allItems.Count)
            {
                try
                {
                    var itemsToUpdate = allItems.Skip(index).Take(stepSize).ToList();
                    var skuList = itemsToUpdate.Select(i => i.SKU).ToList();
                    var items = api.GetCompetitivePricingForSKU(skuList);

                    foreach (var item in items)
                    {
                        var toUpdate = itemsToUpdate.FirstOrDefault(pi => pi.SKU == item.SKU);
                        if (toUpdate != null && item.LowestPrice.HasValue)
                        {
                            toUpdate.LowestPrice = item.LowestPrice;
                            toUpdate.LowestPriceUpdateDate = _time.GetAppNowTime();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Error when updating rank for index: " + index, ex);
                }
                index += stepSize;

                stepSleeper.NextStep();
            }

            db.Commit();
        }

        public void UpdateRatingByProductApi(AmazonApi api, IUnitOfWork db)
        {
            var allParentItems = db.ParentItems.GetAll().Where(p => p.Market == (int)api.Market
                && p.MarketplaceId == api.MarketplaceId).ToList();
            var index = 0;
            var stepSize = 20;

            var stepSleeper = new StepSleeper(TimeSpan.FromSeconds(1), 10);
            while (index < allParentItems.Count)
            {
                try
                {
                    var parentItems = allParentItems.Skip(index).Take(stepSize).ToList();
                    var asinList = parentItems.Select(i => i.ASIN).ToList();
                    var items = api.GetCompetitivePricingForSKU(asinList);

                    foreach (var item in items)
                    {
                        var toUpdate = parentItems.FirstOrDefault(pi => pi.ASIN == item.ASIN);
                        if (toUpdate != null)
                        {
                            toUpdate.Rank = (int?)item.Rank;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Error when updating rank for index: " + index, ex);
                }
                index += stepSize;

                stepSleeper.NextStep();
            }

            db.Commit();
        }

        //public void UpdateRatingByECommerceApi(AmazonApi api, IUnitOfWork db)
        //{
        //    var allParentItems = db.ParentItems.GetAll().Where(p => p.Market == (int)api.Market
        //        && p.MarketplaceId == api.MarketplaceId).ToList();
        //    var index = 0;
            
        //    var stepSleeper = new StepSleeper(TimeSpan.FromSeconds(5), 10);
            
        //    //STEP 1. Get Rating
        //    var allNodeIds = new List<string>();
        //    while (index < allParentItems.Count)
        //    {
        //        try
        //        {
        //            var parentItems = allParentItems.Skip(index).Take(10).ToList();
        //            var asinList = parentItems.Select(i => i.ASIN).ToList();
        //            var resp = api.RetrieveRanks(_log, asinList);

        //            if (resp.Items != null && resp.Items.Any() && resp.Items[0].Item != null)
        //            {
        //                foreach (var item in parentItems)
        //                {
        //                    var el = resp.Items[0].Item.FirstOrDefault(i => i.ASIN == item.ASIN);
        //                    if (el != null)
        //                    {
        //                        item.Rank = !string.IsNullOrEmpty(el.SalesRank) ? (int?)int.Parse(el.SalesRank) : null;
        //                        if (el.BrowseNodes != null && el.BrowseNodes.BrowseNode.Any())
        //                        {
        //                            var nodeIds = el.BrowseNodes.BrowseNode.Select(n => n.BrowseNodeId);
        //                            foreach (var nodeId in nodeIds)
        //                            {
        //                                if (!allNodeIds.Contains(nodeId))
        //                                {
        //                                    allNodeIds.Add(nodeId);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _log.Error("Error when updating rank for index: " + index, ex);
        //        }
        //        index += 10;

        //        stepSleeper.NextStep();
        //    }

        //    //STEP 2. Get Position Info
        //    var allAsinList = allParentItems.Select(i => i.ASIN).ToList();
        //    var topItems = new List<TopNodeInfo>();
        //    foreach (var nodeId in allNodeIds)
        //    {
        //        try
        //        {
        //            Thread.Sleep(TimeSpan.FromSeconds(5));
        //            var nodesRes = api.BrowseNodes(_log, nodeId);
        //            if (nodesRes.BrowseNodes != null && nodesRes.BrowseNodes.Any() && nodesRes.BrowseNodes[0].BrowseNode != null)
        //            {
        //                var exists = nodesRes.BrowseNodes[0].BrowseNode.Select(n => n.TopItemSet).ToList();
        //                if (exists.Any())
        //                {
        //                    var exist = exists[0];
        //                    if (exist != null)
        //                    {
        //                        var itemSet = exist.First();
        //                        var asins = itemSet.TopItem.Select(iSet => new { iSet.ASIN }).ToList();
        //                        for (var i = 0; i < asins.Count; i++)
        //                        {
        //                            var asin = asins[i];
        //                            if (allAsinList.Contains(asin.ASIN) &&
        //                                !topItems.Any(t => t.ASIN == asin.ASIN && t.BrowseNodeId == nodeId))
        //                            {
        //                                topItems.Add(new TopNodeInfo()
        //                                {
        //                                    ASIN = asin.ASIN,
        //                                    Name = nodesRes.BrowseNodes[0].BrowseNode[0].Name,
        //                                    BrowseNodeId = nodeId,
        //                                    Index = i + 1
        //                                });
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _log.Error("Error when updating nodes for node: " + nodeId, ex);
        //        }
        //    }

            
        //    //STEP 3. Send updates to DB
        //    foreach (var topItem in topItems)
        //    {
        //        var asin = topItem.ASIN;
        //        var nodeId = topItem.BrowseNodeId;
        //        var dbNode = db.NodePositions.GetFiltered(p => p.ASIN == asin 
        //                && p.NodeIdentifier == nodeId
        //                && p.Market == (int)api.Market
        //                && p.MarketplaceId == api.MarketplaceId)
        //            .FirstOrDefault();

        //        if (dbNode != null)
        //        {
        //            dbNode.Position = topItem.Index;
        //            dbNode.NodeName = topItem.Name;
        //            dbNode.UpdateDate = _time.GetAppNowTime();
        //        }
        //        else
        //        {
        //            db.NodePositions.Add(new NodePosition
        //            {
        //                ASIN = topItem.ASIN,
        //                NodeName = topItem.Name,
        //                NodeIdentifier = topItem.BrowseNodeId,
        //                Position = topItem.Index,
        //                CreateDate = _time.GetAppNowTime(),
        //                UpdateDate = _time.GetAppNowTime()
        //            });
        //        }
        //    }
        //    db.Commit();
        //}

        //public void UpdateLowestPriceByProductApi(AmazonApi api, IUnitOfWork db)
        //{
        //    var items = db.Items.GetAll().Where(p => p.Market == (int)api.Market
        //        && p.MarketplaceId == api.MarketplaceId).ToList();
        //    var index = 0;
        //    var stepSize = 20;
        //    var stepSleeper = new StepSleeper(TimeSpan.FromSeconds(1), 10);

        //    while (index < items.Count)
        //    {
        //        var childItems = items.Skip(index).Take(stepSize).ToList();
        //        var asinList = childItems.Select(i => i.ASIN).ToList();

        //        var results = api.GetProductForASIN(asinList);
        //        foreach (var result in results)
        //        {
        //            var toUpdate = childItems.FirstOrDefault(i => i.ASIN == result.ASIN);

        //            if (toUpdate != null)
        //            {
        //                var listings = db.Listings.GetFiltered(l => l.ItemId == toUpdate.Id).ToList();
        //                foreach (var listing in listings)
        //                {
        //                    listing.LowestPrice = result.LowestPrice;
        //                }
        //            }
        //        }

        //        db.Commit();

        //        index += stepSize;

        //        stepSleeper.NextStep();
        //    }
        //}


        //public void UpdateLowestPriceByECommerceApi(AmazonApi api, IUnitOfWork db)
        //{
        //    var items = db.Items.GetAll().Where(p => p.Market == (int)api.Market
        //        && p.MarketplaceId == api.MarketplaceId).ToList();
        //    var index = 0;
        //    var stepSleeper = new StepSleeper(TimeSpan.FromSeconds(5), 10);

        //    while (index < items.Count)
        //    {
        //        var childItems = items.Skip(index).Take(10).ToList();
        //        var asinList = childItems.Select(i => i.ASIN).ToList();

        //        var resp = api.RetrieveOffers(_log, asinList);

        //        if (resp.Items != null && resp.Items.Any() && resp.Items[0].Item != null)
        //        {
        //            foreach (var item in childItems)
        //            {
        //                var el = resp.Items[0].Item.FirstOrDefault(i => i.ASIN == item.ASIN);
        //                if (el != null && el.OfferSummary != null)
        //                {
        //                    var itemId = item.Id;
        //                    var lowestPrice = el.OfferSummary.LowestNewPrice != null
        //                        ? (decimal?)GeneralUtils.GetPrice(el.OfferSummary.LowestNewPrice.FormattedPrice.Replace(".",
        //                            CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
        //                        : null;
        //                    var listings = db.Listings.GetFiltered(l => l.ItemId == itemId).ToList();
        //                    foreach (var listing in listings)
        //                    {
        //                        listing.LowestPrice = lowestPrice;
        //                    }
        //                }
        //            }
        //            db.Commit();

        //        }
        //        index += 10;

        //        stepSleeper.NextStep();
        //    }
        //}
    }
}
