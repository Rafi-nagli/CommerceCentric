using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.Web.Models;

namespace Amazon.Model.Implementation
{
    public class QuantityManager : IQuantityManager
    {
        private ILogService _log;
        private ITime _time;

        public QuantityManager(ILogService log,
            ITime time)
        {
            _log = log;
            _time = time;
        }

        public void Process(IUnitOfWork db,
            long listingId,
            int quantity,
            QuantityChangeSourceType changeType,
            string orderId,
            long? styleId,
            string styleString,
            long? styleItemId,
            string sku,
            string size)
        {
            var historyUpdated = UpdateQuantityHistory(db,
                _log,
                quantity,
                changeType,
                listingId,
                orderId,
                styleId,
                styleString,
                styleItemId,
                sku,
                size,
                _time.GetAppNowTime());
        }

        
        private bool UpdateQuantityHistory(IUnitOfWork db, 
            ILogService log, 
            int quantity, 
            QuantityChangeSourceType changeType, 
            long listingId, 
            string orderId, 
            long? styleId,
            string styleString, 
            long? styleItemId,
            string sku, 
            string size, 
            DateTime when)
        {
            var qtyHistories = db.QuantityHistories.GetByOrderId(orderId);
            var dbQtyHistory = qtyHistories.FirstOrDefault(q => q.ListingId == listingId && q.StyleItemId == styleItemId);

            if (changeType == QuantityChangeSourceType.NewOrder)
            {
                if (dbQtyHistory != null)
                {
                    //Already substracted
                    if (dbQtyHistory.QuantityChanged != quantity)
                    {
                        log.Fatal(String.Format("Quantity not equal for order item in quantity history record, orderId={0}, listing={1}, qty history={2}, new quantity={3}",
                            orderId, listingId, dbQtyHistory.QuantityChanged, quantity));
                    }
                    else
                    {
                        log.Warn(String.Format("History record already exist, orderId={0}, listingId={1}, quantity={2}", orderId, listingId, quantity));
                    }
                    return false;
                }
                else
                {
                    db.QuantityHistories.Add(new QuantityHistory()
                    {
                        ListingId = listingId,
                        Type = (int)QuantityChangeSourceType.NewOrder,
                        QuantityChanged = quantity,
                        SKU = sku,
                        StyleId = styleId,
                        StyleString = styleString,
                        StyleItemId = styleItemId,
                        OrderId = orderId,
                        Size = size,
                        CreateDate = when,
                    });

                    if (styleItemId.HasValue)
                    {
                        LogStyleItemQuantity(db,
                            styleItemId.Value,
                            quantity,
                            null,
                            QuantityChangeSourceType.NewOrder,
                            orderId,
                            null,
                            null,
                            when,
                            null);
                    }
                    return true;
                }
            }

            if (changeType == QuantityChangeSourceType.OrderCancelled)
            {
                if (qtyHistories.Count == 1 && qtyHistories.First().Type == (int)QuantityChangeSourceType.NewOrder)
                {
                    db.QuantityHistories.Add(new QuantityHistory()
                    {
                        ListingId = listingId,
                        Type = (int)QuantityChangeSourceType.OrderCancelled,
                        QuantityChanged = -quantity,
                        SKU = sku,
                        StyleId = styleId,
                        StyleString = styleString,
                        StyleItemId = styleItemId,
                        OrderId = orderId,
                        Size = size,
                        CreateDate = when,
                    });

                    if (styleItemId.HasValue)
                    {
                        LogStyleItemQuantity(db,
                            styleItemId.Value,
                            -quantity,
                            null,
                            QuantityChangeSourceType.OrderCancelled,
                            orderId,
                            null,
                            null,
                            when,
                            null);
                    }
                    return true;
                }
                else
                {
                    //Do nothing
                    //-Canceled info already exist
                    //-Or no new order info
                }
            }

            db.Commit();
            return false;
        }


        public void UpdateListingQuantity(IUnitOfWork db, 
            QuantityChangeSourceType type, 
            Listing listing, 
            int? newQuantity,
            string size,
            string styleString,
            long? styleId,
            long? styleItemId,
            DateTime when, 
            long? by)
        {
            var wasChanged = false;
            var note = String.Empty;

            if (listing.RealQuantity != (newQuantity ?? 0))
            {
                note = listing.RealQuantity.ToString();
                listing.QuantityUpdateRequested = true;
                listing.QuantityUpdateRequestedDate = when;
                listing.RealQuantity = newQuantity ?? 0;
                listing.RealQuantityUpdateDate = when;
                if (type == QuantityChangeSourceType.SetByAutoQuantity)
                    listing.AutoQuantityUpdateDate = when;

                wasChanged = true;
            }

            if (wasChanged)
            {
                db.QuantityHistories.Add(new QuantityHistory()
                {
                    ListingId = listing.Id,
                    Type = (int) type,
                    QuantityChanged = newQuantity ?? 0,
                    SKU = StringHelper.Substring(listing.SKU, 50),

                    StyleId = styleId,
                    StyleString = StringHelper.Substring(styleString, 50),
                    StyleItemId = styleItemId,
                    Size = StringHelper.Substring(size, 50),

                    OrderId = StringHelper.Substring(note, 50),
                    CreateDate = when,
                    CreatedBy = by
                });

                db.Commit();
            }
        }

        public void LogStyleItemQuantity(IUnitOfWork db, 
            long styleItemId,
            int? newQuantity, 
            int? oldQuantity, 
            QuantityChangeSourceType type,
            string tag,
            long? sourceEntityTag,
            string sourceEntityName,
            DateTime when, 
            long? by)
        {
            var styleItemCache = db.StyleItemCaches.GetAllAsDto().FirstOrDefault(si => si.Id == styleItemId);

            db.StyleItemQuantityHistories.Add(new StyleItemQuantityHistory()
            {
                StyleItemId = styleItemId,
                
                Quantity = newQuantity,
                FromQuantity = oldQuantity,

                BeforeRemainingQuantity = styleItemCache != null ? styleItemCache.RemainingQuantity : (int?)null,

                Type = (int)type,
                Tag = tag,                
                SourceEntityTag = sourceEntityTag,
                SourceEntityName = sourceEntityName,

                CreateDate = when,
                CreatedBy = by
            });

            db.Commit();
        }

        #region Quantity Operation

        public long AddQuantityOperation(IUnitOfWork db, 
            QuantityOperationDTO quantityOperation,
            DateTime when,
            long? by)
        {
            var operation = new QuantityOperation();
            operation.Type = (int)quantityOperation.Type;
            operation.OrderId = quantityOperation.OrderId;
            operation.Comment = quantityOperation.Comment;

            operation.CreateDate = when;
            operation.CreatedBy = by;

            db.QuantityOperations.Add(operation);
            db.Commit();

            foreach (var change in quantityOperation.QuantityChanges)
            {
                var dbChange = new QuantityChange();
                dbChange.QuantityOperationId = operation.Id;

                dbChange.StyleId = change.StyleId;
                dbChange.StyleItemId = change.StyleItemId;
                dbChange.Quantity = change.Quantity;

                dbChange.InActive = change.InActive;
                dbChange.ExpiredOn = change.ExpiredOn;
                dbChange.Tag = StringHelper.Substring(change.Tag, 50);

                dbChange.CreateDate = when;
                dbChange.CreatedBy = by;

                db.QuantityChanges.Add(dbChange);

                LogStyleItemQuantity(db,
                    change.StyleItemId,
                    change.Quantity,
                    null,
                    QuantityChangeSourceType.AddSpecialCase,
                    operation.Type.ToString(),
                    dbChange.Id,
                    StringHelper.Substring(StringHelper.GetFirstNotEmpty(operation.OrderId), 50),
                    when,
                    by);
            }
            db.Commit();

            return operation.Id;
        }

        #endregion

        #region FixUp Quantity

        public void FixupListingQuantity(IUnitOfWork db, ISettingsService settingService)
        {
            var listingsWithIssue = db.Listings.GetAll().Where(l => ((!l.DisplayQuantity.HasValue && l.RealQuantity != l.AmazonRealQuantity)
                                            || (l.DisplayQuantity.HasValue && (l.DisplayQuantity > l.RealQuantity ? l.RealQuantity : l.DisplayQuantity) != l.AmazonRealQuantity))
                                            && !l.IsFBA
                                            && !l.IsRemoved).ToList();


            var marketplaces = new List<MarketplaceName>()
            {
                new MarketplaceName()
                {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonComMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.AmazonEU,
                    MarketplaceId = MarketplaceKeeper.AmazonUkMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.AmazonAU,
                    MarketplaceId = MarketplaceKeeper.AmazonAuMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonCaMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.Amazon,
                    MarketplaceId = MarketplaceKeeper.AmazonMxMarketplaceId
                },
                new MarketplaceName()
                {
                    Market = MarketType.Walmart,
                },
                new MarketplaceName()
                {
                    Market = MarketType.WalmartCA,
                },
                new MarketplaceName()
                {
                    Market = MarketType.eBay,
                },
                new MarketplaceName()
                {
                    Market = MarketType.Jet,
                }
            };

            foreach (var market in marketplaces)
            {
                var marketListings = listingsWithIssue.Where(l => l.Market == (int)market.Market
                    && (l.MarketplaceId == market.MarketplaceId || String.IsNullOrEmpty(market.MarketplaceId))).ToList();
                var listingCount = marketListings.Count(l => l.RealQuantity == 0);
                _log.Debug("Checking " + market.Market + "-" + market.MarketplaceId + ", count=" + listingCount);
                settingService.SetListingsQtyAlert(listingCount, market.Market, market.MarketplaceId);
                foreach (var listing in marketListings)
                {
                    _log.Debug("Request update for: " + listing.Id + ", SKU=" + listing.SKU);
                    listing.QuantityUpdateRequested = true;
                    listing.QuantityUpdateRequestedDate = _time.GetAppNowTime();
                }

                db.Commit();
            }
        }

        #endregion
    }
}
