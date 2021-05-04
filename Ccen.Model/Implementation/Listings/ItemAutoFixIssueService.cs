using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Listings.Rules;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation
{
    //public class ItemAutoFixIssueService
    //{
    //    private ILogService _log;
    //    private ITime _time;
    //    private IDbFactory _dbFactory;
    //    private ISystemActionService _actionService;
    //    private IItemHistoryService _itemHistoryService;

    //    public ItemAutoFixIssueService(ILogService log,
    //        ITime time,
    //        IDbFactory dbFactory,
    //        ISystemActionService actionService,
    //        IItemHistoryService itemHistoryService)
    //    {
    //        _log = log;
    //        _time = time;
    //        _dbFactory = dbFactory;
    //        _actionService = actionService;
    //        _itemHistoryService = itemHistoryService;
    //    }

    //    //public void ProcessRules(IList<IItemAutoFixRule> rules)
    //    //{
    //    //    using (var db = _dbFactory.GetRWDb())
    //    //    {
    //    //        var items = (from i in db.Items.GetAll()
    //    //                    join l in db.Listings.GetAll() on i.Id equals l.ItemId
    //    //                    where !l.IsRemoved
    //    //                        && i.Market == (int)MarketType.Amazon
    //    //                        && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
    //    //                    select new ItemDTO()
    //    //                    {
    //    //                        Id = i.Id,
    //    //                        SKU = l.SKU,
    //    //                        PublishedStatus = i.ItemPublishedStatus,
    //    //                        Color = i.Color,
    //    //                        BrandName = i.BrandName,
    //    //                        ParentASIN = i.ParentASIN,
    //    //                        Market = i.Market,
    //    //                        MarketplaceId = i.MarketplaceId,
    //    //                        IsAmazonParentASIN = i.IsAmazonParentASIN,
    //    //                        IsExistOnAmazon = i.IsExistOnAmazon
    //    //                    }).ToList();

    //    //        var additionalItems = (from i in db.ItemAdditions.GetAllAsDTO()
    //    //                               where i.Value.Contains("The SKU data provided is different from what's already in the Amazon catalog.")
    //    //                               select i).ToList();

    //    //        foreach (var item in items)
    //    //        {
    //    //            foreach (var rule in rules)
    //    //            {
    //    //                rule.Apply(item, additionalItems.Where(i => i.ItemId == item.Id).ToList());
    //    //            }
    //    //        }
    //    //    }   
    //    //}

    //    //public void ProcessDefectsRules()
    //    //{
    //    //    var imageDefectRule = new FixImageDefectIssueRule(_dbFactory, _actionService, _log, _time);
    //    //    using (var db = _dbFactory.GetRWDb())
    //    //    {
    //    //        var imageDefects = db.ListingDefects.GetAllAsDto().Where(d => d.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
    //    //            && d.FieldName == "Image"
    //    //            && d.AlertType == "Missing")
    //    //            .ToList();

    //    //        imageDefectRule.Apply(imageDefects);
    //    //    }                
    //    //}

    //    //public void RequestUnpublishWithUPCIssueListings()
    //    //{
    //    //    _log.Info("RequestUnpublishWithUPCIssueListings");
    //    //    using (var db = _dbFactory.GetRWDb())
    //    //    {
    //    //        var lastDate = _time.GetAppNowTime().AddDays(-1);

    //    //        //You are using UPCs, EANs, ISBNs, ASINs, or JAN codes that do not match the products you are trying to list. Using incorrect UPCs, EANs, ISBNs, ASINs or JAN codes is prohibited and it can result in your ASIN creation privileges being suspended or permanently removed. Please ensure you are always using the appropriate UPC, EAN, ISBN, ASIN or JAN code when listing a product. If you have reached this message in error, please contact Seller Support using the following link: https://sellercentral.amazon.com/hz/contact-us. For more information, refer to the ASIN Creation Policy Help Page - https://sellercentral.amazon.com/gp/help/201844590
    //    //        var itemsToDisable = db.ItemAdditions.GetAll().Where(i => i.Value.Contains("You are using UPCs, EANs, ISBNs, ASINs, or JAN codes that do not match")).Select(i => i.ItemId).Distinct().ToList();
    //    //        var items = db.Items.GetAll().Where(i => itemsToDisable.Contains(i.Id)).ToList();
    //    //        _log.Info("Items to unpublish: " + items.Count());
    //    //        foreach (var item in items)
    //    //        {
    //    //            _log.Info("Mark as unpublished, itemId=" + item.Id + ", ASIN=" + item.ASIN);
    //    //            item.ItemPublishedStatusDate = _time.GetAppNowTime();
    //    //            item.ItemPublishedStatus = (int)PublishedStatuses.HasUnpublishRequest;
    //    //        }
    //    //        db.Commit();
    //    //    }
    //    //}

    //    //public void RequestUpdatesForPublishingInProgressListings()
    //    //{
    //        //_log.Info("RequestUpdatesForPublishingInProgressListings");
    //        //using (var db = _dbFactory.GetRWDb())
    //        //{
    //        //    var lastDate = _time.GetAppNowTime().AddDays(-1);

    //        //    var items = (from i in db.Items.GetAll()
    //        //                 join l in db.Listings.GetAll() on i.Id equals l.ItemId
    //        //                 where i.Market == (int)MarketType.Amazon
    //        //                            && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
    //        //                            && i.ItemPublishedStatus == (int)PublishedStatuses.PublishedInProgress
    //        //                 select new DTO.ItemDTO()
    //        //                 {
    //        //                     SKU = l.SKU,
    //        //                     Id = i.Id
    //        //                 }).ToList();

    //        //    _log.Info("SKU count: " + items.Count);
    //        //    _log.Info("SKUs to update: " + String.Join(", ", items.Select(i => i.SKU)));

    //        //    foreach (var item in items)
    //        //    {
    //        //        var dbListing = db.Listings.GetAll().FirstOrDefault(l => l.ItemId == item.Id && !l.IsRemoved);
    //        //        _log.Info("Request full update for SKU: " + dbListing?.SKU);
    //        //        dbListing.PriceUpdateRequested = true;
    //        //        dbListing.PriceUpdateRequestedDate = _time.GetAppNowTime();
    //        //        dbListing.QuantityUpdateRequested = true;
    //        //        dbListing.QuantityUpdateRequestedDate = _time.GetAppNowTime();
    //        //        SystemActionHelper.RequestImageUpdates(db, _actionService, item.Id, null);
    //        //        SystemActionHelper.RequestRelationshipUpdates(db, _actionService, item.Id, null);
    //        //    }

    //        //    db.Commit();
    //        //}

    //    //}

    //    //public void RequestUpdatesForUngroupedListings()
    //    //{
    //        //_log.Info("RequestUpdatesForUngroupedListings");
    //        //using (var db = _dbFactory.GetRWDb())
    //        //{
    //        //    var lastDate = _time.GetAppNowTime().AddDays(-2);

    //        //    var items = (from i in db.Items.GetAll()
    //        //         join l in db.Listings.GetAll() on i.Id equals l.ItemId
    //        //         where i.Market == (int)MarketType.Amazon
    //        //                    && i.MarketplaceId == MarketplaceKeeper.AmazonComMarketplaceId
    //        //                    && i.ItemPublishedStatus == (int)PublishedStatuses.Published
    //        //                    && i.IsAmazonParentASIN == false
    //        //                    && (!i.LastForceRepublishedDate.HasValue || i.LastForceRepublishedDate.Value < lastDate)
    //        //         select new DTO.ItemDTO()
    //        //         {
    //        //             SKU = l.SKU,
    //        //             Id = i.Id
    //        //         }).ToList();

    //        //    _log.Info("SKU count: " + items.Count);
    //        //    _log.Info("SKUs to update: " + String.Join(", ", items.Select(i => i.SKU)));

    //        //    foreach (var item in items)
    //        //    {
    //        //        var newAction = new DTO.SystemActionDTO()
    //        //        {
    //        //            ParentId = null,
    //        //            Status = (int)SystemActionStatus.None,
    //        //            Type = (int)SystemActionType.UpdateOnMarketProductRelationship,
    //        //            Tag = item.Id.ToString(),
    //        //            InputData = null,

    //        //            CreateDate = _time.GetUtcTime(),
    //        //            CreatedBy = null,
    //        //        };
    //        //        db.SystemActions.AddAction(newAction);
    //        //    }

    //        //    db.Commit();
    //        //}
    //    //}
    //}
}
