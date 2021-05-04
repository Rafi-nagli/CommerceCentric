using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Api;
using Amazon.Api.AmazonECommerceService;
using Amazon.Api.Models;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Listings;
using Amazon.Core.Models;
using Amazon.DAL.Repositories.Listings;
using Amazon.DTO;

namespace Amazon.Model.Implementation
{
    public class CompetitorQuantityCheckerService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public CompetitorQuantityCheckerService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }

        public const int MaxQuantity = 999;

        public class CartInfo
        {
            public string CartId { get; set; }
            public string HMAC { get; set; }
        }


        public void ProcessAllMarketASINs(AmazonApi advApi)
        {
            var asinToProcess = new List<string>();

            var fromDate = _time.GetAppNowTime().AddHours(-12);
            using (var db = _dbFactory.GetRWDb())
            {
                var freshBBQuery = from bb in db.BuyBoxQuantities.GetAll() 
                                   where bb.CreateDate > fromDate 
                                    && bb.Market == (int)advApi.Market
                                    && bb.MarketplaceId == advApi.MarketplaceId
                                   select bb;

                var asinToProcessQuery = from i in db.Items.GetAllViewAsDto(advApi.Market, advApi.MarketplaceId)
                    join bb in freshBBQuery on new {i.ASIN, i.Market, i.MarketplaceId} equals
                        new {bb.ASIN, bb.Market, bb.MarketplaceId} into withBB
                    from bb in withBB.DefaultIfEmpty()
                    where bb == null
                    select i.ASIN;

                asinToProcess = asinToProcessQuery.ToList();
            }
            _log.Info("Total asin to process=" + asinToProcess.Count + ", market=" + advApi.Market + ", marketplaceId=" + advApi.MarketplaceId);


            CartInfo currentCartInfo = null;
            var index = 0;
            var step = 50;
            while (index < asinToProcess.Count)
            {
                var stepAsinList = asinToProcess.Skip(index).Take(step).ToList();
                _log.Info("ASIN to process: " + String.Join(", ", stepAsinList));

                var results = RequestQuantities(advApi, stepAsinList.ToList(), ref currentCartInfo);

                using (var db = _dbFactory.GetRWDb())
                {
                    foreach (var item in results)
                    {
                        _log.Info("Processed ASIN=" + item.ASIN + ", qty=" + item.Quantity + ", seller=" + item.SellerNickname);
                        db.BuyBoxQuantities.Add(new BuyBoxQuantity()
                        {
                            ASIN = item.ASIN,
                            Market = (int) advApi.Market,
                            MarketplaceId = advApi.MarketplaceId,
                            Quantity = item.Quantity,
                            SellerNickname = item.SellerNickname,
                            CreateDate = _time.GetAppNowTime()
                        });
                    }
                    _log.Info("Before commit");
                    db.Commit();
                    _log.Info("After commit");
                }

                index += step;
            }
        }

        public List<CartItemInfo> RequestQuantities(AmazonApi advApi,
            IList<string> asinList,
            ref CartInfo initialCartInfo)
        {
            var currentCartInfo = initialCartInfo;
            var results = new List<CartItemInfo>();

            //if (asinList.Count == 0)
            //    return results;

            //var index = 0;
            //var stepSize = 50;
            //var stepSleeper = new StepSleeper(TimeSpan.FromSeconds(10), 1);

            //while (index < asinList.Count)
            //{
            //    var asinToProcess = asinList.Skip(index).Take(stepSize).ToList();

            //    _log.Info("Processing AsinList=" + String.Join(", ", asinToProcess));

            //    CartItems responseItems = null;
            //    if (currentCartInfo == null)
            //    {
            //        _log.Info("Before CartCreate request");
            //        var createCartResult = advApi.CartCreate(_log, 
            //            asinToProcess.Select(a => new CartItemInfo()
            //            {
            //                ASIN = a,
            //                Quantity = MaxQuantity
            //            }).ToList());
            //        _log.Info("After CartCreate request");

            //        if (createCartResult != null && createCartResult.Cart != null && createCartResult.Cart.Any())
            //        {
            //            var cart = createCartResult.Cart[0];
            //            currentCartInfo = new CartInfo()
            //            {
            //                CartId = cart.CartId,
            //                HMAC = cart.HMAC,
            //            };

            //            responseItems = cart.CartItems;
            //        }
            //        else
            //        {
            //            if (createCartResult != null)
            //                _log.Info("Received empty cart: " + GetErrors(createCartResult.OperationRequest));
            //            else
            //                _log.Info("Received empty response");
            //        }
            //    }
            //    else
            //    {
            //        _log.Info("Before CartClear request");
            //        var cartClearResponse = advApi.CartClear(_log, currentCartInfo.CartId, currentCartInfo.HMAC);
            //        _log.Info("After CartClear request");

            //        if (cartClearResponse != null)
            //        {
            //            _log.Info("Before CartAdd request");
            //            var cartAddResponse = advApi.CartAdd(_log,
            //                currentCartInfo.CartId,
            //                currentCartInfo.HMAC,
            //                asinToProcess.Select(a => new CartItemInfo()
            //                {
            //                    ASIN = a,
            //                    Quantity = MaxQuantity
            //                }).ToList());
            //            _log.Info("After CartAdd request");

            //            if (cartAddResponse != null && cartAddResponse.Cart != null && cartAddResponse.Cart.Any())
            //            {
            //                responseItems = cartAddResponse.Cart[0].CartItems;
            //            }
            //        }
            //    }

            //    if (responseItems != null)
            //    {
            //        var newItems = responseItems.CartItem.Select(i => new CartItemInfo()
            //        {
            //            ASIN = i.ASIN,
            //            Quantity = StringHelper.TryGetInt(i.Quantity) ?? 0,
            //            CartItemId = i.CartItemId,
            //            SellerNickname = i.SellerNickname
            //        }).ToList();
            //        foreach (var newItem in newItems)
            //        {
            //            _log.Info("Received Quantity for: asin=" + newItem.ASIN + ", qty=" + newItem.Quantity);
            //        }
            //        foreach (var requestItem in asinToProcess)
            //        {
            //            if (newItems.All(i => i.ASIN != requestItem))
            //            {
            //                _log.Info("Not received Quantity for: asin=" + requestItem);
            //                newItems.Add(new CartItemInfo()
            //                {
            //                    ASIN = requestItem,
            //                    Quantity = null,
            //                    SellerNickname = null,
            //                });
            //            }
            //        }
            //        AddCartItemsTo(results, newItems);
            //    }
                
            //    index += stepSize;

            //    stepSleeper.NextStep();
            //}
            //initialCartInfo = currentCartInfo;

            return results;
        }

        private string GetErrors(OperationRequest response)
        {
            if (response != null)
            {
                if (response.Errors != null)
                {
                    return String.Join(", ", response.Errors.Select(e => e.Code + "-" + e.Message));
                }
            }
            return "";
        }

        private void AddCartItemsTo(IList<CartItemInfo> items, IList<CartItemInfo> newCartItems)
        {
            foreach (var newItem in newCartItems)
            {
                if (items.All(i => i.ASIN != newItem.ASIN))
                {
                    items.Add(newItem);
                }
            }
        }
    }
}
