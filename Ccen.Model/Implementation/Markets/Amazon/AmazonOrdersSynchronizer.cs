using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Sync;

namespace Amazon.Model.Implementation.Markets.Amazon
{
    public class AmazonOrdersSynchronizer : BaseOrdersSynchronizer
    {
        /// <summary>
        /// For UIUpdate
        /// </summary>
        public AmazonOrdersSynchronizer(ILogService log,
            CompanyDTO company,
            ISyncInformer syncInfo,
            IList<IShipmentApi> rateProviders,
            ICompanyAddressService companyAddress,
            ITime time,
            IWeightService weightService,
            ISystemMessageService messageService) : base(log,
                null,
                company,
                null,
                syncInfo,
                rateProviders,
                null,
                null,
                null,
                null,
                null,
                null,
                companyAddress,
                time,
                weightService,
                messageService)
        {

        }

        /// <summary>
        /// For Sync
        /// </summary>
        public AmazonOrdersSynchronizer(ILogService log,
            IMarketApi api,
            CompanyDTO company,
            ISettingsService settings,
            ISyncInformer syncInfo,
            IList<IShipmentApi> rateProviders,
            IQuantityManager quantityManager,
            IEmailService emailService,
            IOrderValidatorService validatorService,
            IOrderHistoryService orderHistoryService,
            ICacheService cacheService,
            ISystemActionService systemAction,
            ICompanyAddressService companyAddress,
            ITime time,
            IWeightService weightService,
            ISystemMessageService messageService) : base(log,
                api,
                company,
                settings,
                syncInfo,
                rateProviders,
                quantityManager,
                emailService,
                validatorService,
                orderHistoryService,
                cacheService,
                systemAction,
                companyAddress,
                time,
                weightService,
                messageService)
        {

        }


        public override SyncResult Sync(OrderSyncModes syncMode, CancellationToken? cancel)
        {
            var syncMissedFrom = DateTime.UtcNow -
                     (syncMode == OrderSyncModes.Full
                         ? _settings.GetOrdersSyncForMissedInterval(_api.Market, _api.MarketplaceId)
                         : _settings.GetOrdersSyncForMissedShortInterval(_api.Market, _api.MarketplaceId));

            _log.Info("Sync, syncMode=" + syncMode + ", from date=" + syncMissedFrom);

            var isSuccess = true;
            var processedOrderIdList = new List<SyncResultOrderInfo>();
            var skippedOrderIdList = new List<SyncResultOrderInfo>();

            _syncInfo.PingSync();

            //STEP 1. SyncAllUnshippedOrders
            var stopWatch = Stopwatch.StartNew();
            

            var syncResult = SyncAllUnshippedOrders(
                cancel,
                _log,
                _api,

                GetOrderItemsFromMarket,
                _syncInfo,

                _rateProviders,
                _quantityManager,
                _settings,
                _emailService,
                _validatorService,
                _cacheService,

                _companyAddress,
                syncMissedFrom,
                _time);

            isSuccess = isSuccess && syncResult.IsSuccess;
            if (syncResult.ProcessedOrders != null)
                processedOrderIdList.AddRange(syncResult.ProcessedOrders);
            if (syncResult.SkippedOrders != null)
                skippedOrderIdList.AddRange(syncResult.SkippedOrders);

            stopWatch.Stop();
            _log.Info("STEP. SyncAllUnshippedOrders, duration=" + stopWatch.ElapsedMilliseconds);
            
            _syncInfo.PingSync();

            //STEP 2. UpdateExistingOrders
            stopWatch.Start();

            var excludedOrders = processedOrderIdList.Select(o => o.OrderId).ToList();
            if (syncResult.SkippedOrders != null)
                excludedOrders.AddRange(syncResult.SkippedOrders.Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped).Select(o => o.OrderId).ToList());
            
            //NOTE: exclude list contains orders that have unshipped status on Market (no need to update these, except if orders in DB have status Pending)
            syncResult = UpdateExistingOrders(cancel,
                                _log,
                                _api,

                                GetOrderItemsFromMarket,

                                _syncInfo,
                                _rateProviders,
                                _quantityManager,
                                _emailService,
                                _validatorService,

                                syncMode == OrderSyncModes.Full,
                                excludedOrders,

                                _companyAddress,
                                _time);

            isSuccess = isSuccess && syncResult.IsSuccess;
            if (syncResult.ProcessedOrders != null)
                processedOrderIdList.AddRange(syncResult.ProcessedOrders);
            if (syncResult.SkippedOrders != null)
                skippedOrderIdList.AddRange(syncResult.SkippedOrders);

            stopWatch.Stop();
            _log.Info("STEP. UpdateExistingOrders, duration=" + stopWatch.ElapsedMilliseconds);

            _syncInfo.PingSync();

            //STEP 3. StoreNewPendingOrders
            stopWatch.Start();

            syncResult = StoreNewPendingOrders(cancel,
                _log,
                _api,
                GetOrderItemsFromMarket,

                _rateProviders,
                _validatorService,
                _emailService,
                _syncInfo,
                _settings,
                _quantityManager,
                _cacheService,

                _companyAddress,
                _time);

            isSuccess = isSuccess && syncResult.IsSuccess;
            if (syncResult.ProcessedOrders != null)
                processedOrderIdList.AddRange(syncResult.ProcessedOrders);
            if (syncResult.SkippedOrders != null)
                skippedOrderIdList.AddRange(syncResult.SkippedOrders);

            stopWatch.Stop();
            _log.Info("STEP. StoreNewPendingOrders, duration=" + stopWatch.ElapsedMilliseconds);

            _syncInfo.PingSync();

            return new SyncResult()
            {
                IsSuccess = isSuccess,
                ProcessedOrders = processedOrderIdList,
                SkippedOrders = skippedOrderIdList,
            };
        }

        public override SyncResult ProcessSpecifiedOrder(IUnitOfWork db, string orderId)
        {
            return ProcessSpecifiedOrderInner(db, orderId, GetOrderItemsFromMarket);
        }


        #region Sync Actions
        /// <summary>
        /// Requested new pending orders (from last exist pending order date)
        /// </summary>
        /// <returns></returns>
        protected SyncResult StoreNewPendingOrders(CancellationToken? token,
            ILogService log,
            IMarketApi api,

            Func<ILogService, IMarketApi, ISyncInformer, string, IList<ListingOrderDTO>> getOrderItemsFromMarketFunc,

            IList<IShipmentApi> rateProviders,
            IOrderValidatorService validatorService,
            IEmailService emailService,
            ISyncInformer syncInfo,
            ISettingsService settings,
            IQuantityManager quantityManager,
            ICacheService cacheService,

            ICompanyAddressService companyAddress,
            ITime time)
        {
            try
            {
                using (var db = new UnitOfWork(_log))
                {
                    var lastPending = db.Orders
                        .GetFiltered(o => o.OrderStatus == OrderStatusEnumEx.Pending
                            && o.Market == (int)api.Market
                            && o.MarketplaceId == api.MarketplaceId)
                        .OrderBy(o => o.Id).FirstOrDefault();

                    var date = lastPending != null && lastPending.OrderDate.HasValue ? lastPending.OrderDate : null;
                    log.Info(date != null ? "Last pending order date: " + date.Value : "No pending orders!");

                    var updatedAfter = date ?? DateHelper.GetAmazonNowTime().AddDays(-1);
                    log.Info(string.Format("Process Orders created since {0}", updatedAfter));

                    var statusList = new List<string> { OrderStatusEnum.Pending.Str() };

                    var pendingOrders = api.GetOrders(log, updatedAfter, statusList).ToList();
                    log.Info("Total pending orders:" + pendingOrders.Count());

                    syncInfo.PingSync();

                    var result = ProcessNewOrdersPack(token,
                        log,
                        api,
                        getOrderItemsFromMarketFunc,

                        rateProviders,
                        validatorService,
                        syncInfo,
                        settings,
                        quantityManager,
                        emailService,
                        cacheService,

                        companyAddress,
                        pendingOrders,
                        time);

                    if (!result.IsSuccess)
                    {
                        syncInfo.AddWarning("", "The orders pack processing has failed");
                        return new SyncResult() { IsSuccess = false };
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                syncInfo.AddError(null, "Error when storing new orders", ex);
                log.Error("Error when storing new orders", ex);
                return new SyncResult() { IsSuccess = false };
            }
        }


        /// <summary>
        /// Updating pending orders (!shipped and !canceled)
        /// </summary>
        /// <returns></returns>
        protected SyncResult UpdateExistingOrders(CancellationToken? cancel,
            ILogService log,
            IMarketApi api,

            Func<ILogService, IMarketApi, ISyncInformer, string, IList<ListingOrderDTO>> getOrderItemsFromMarketFunc,

            ISyncInformer syncInfo,
            IList<IShipmentApi> rateProviders,
            IQuantityManager quantityManager,
            IEmailService emailService,
            IOrderValidatorService validatorService,

            bool isFullSync,
            IList<string> excludeOrderIdList,

            ICompanyAddressService companyAddress,
            ITime time)
        {
            IList<Order> ordersToUpdate = new List<Order>();

            try
            {
                using (var db = new UnitOfWork(_log))
                {
                    var orderStatuses = isFullSync ? new[] {
                            OrderStatusEnumEx.Pending,
                            OrderStatusEnumEx.Unshipped,
                            OrderStatusEnumEx.PartiallyShipped
                        } : 
                        new[] {
                            OrderStatusEnumEx.Pending,
                        };
                    
                    var existOrders = db.Orders.GetOrdersByStatus(api.Market, 
                        api.MarketplaceId,
                        orderStatuses
                    ).ToList();

                    var withModifiedStatusOrders = db.Orders.GetOrdersWithModifiedStatus(api.Market,
                        api.MarketplaceId);

                    existOrders.AddRange(withModifiedStatusOrders);

                    if (excludeOrderIdList != null)
                        ordersToUpdate = existOrders.Where(o => 
                            !excludeOrderIdList.Contains(o.AmazonIdentifier)
                            || o.OrderStatus == OrderStatusEnumEx.Pending //NOTE: force include pending orders
                            || o.ShippingCalculationStatus == (int)ShippingCalculationStatusEnum.NoCalculation //NOTE: forse include w/o rates
                            || o.ShippingCalculationStatus == (int)ShippingCalculationStatusEnum.WoWeightCalculation
                            ).ToList();
                    else
                        ordersToUpdate = existOrders;

                    log.Info("Orders to update count:" + ordersToUpdate.Count 
                        + " (existing in DB=" + existOrders.Count 
                        + ", exclude list=" + (excludeOrderIdList != null ? excludeOrderIdList.Count.ToString() : "[null]") + ")");

                    var index = 0;
                    var step = 50;
                    while (index < ordersToUpdate.Count)
                    {
                        var ordersToProcess = ordersToUpdate.Skip(index).Take(step).ToList();
                        log.Info("Start process orders from api, from=" + index + ", count=" + ordersToProcess.Count);
                        
                        if (!ProcessExistOrdersPack(cancel,
                            log,
                            api,

                            getOrderItemsFromMarketFunc,

                            syncInfo,
                            db,

                            rateProviders,
                            quantityManager,
                            emailService,
                            validatorService,
                            
                            companyAddress,
                            ordersToProcess,

                            time))
                        {
                            syncInfo.AddWarning("", "The pending orders pack processing has failed");
                            //Break cycle if pack processing is fail, something wrong (m.b. unavailable Db, m.b. parallel sync)
                            return new SyncResult() { IsSuccess = false };
                        }
                        index += step;
                        syncInfo.PingSync();
                    }

                }
                return new SyncResult()
                {
                    IsSuccess = true,
                    ProcessedOrders = ordersToUpdate.Select(o => new SyncResultOrderInfo()
                    {
                        OrderId = o.AmazonIdentifier,
                        OrderStatus = o.OrderStatus
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                LogError(log, syncInfo, ex, "", "Error when updating existing orders");
                return new SyncResult() { IsSuccess = false };
            }
        }

        protected SyncResult SyncAllUnshippedOrders(CancellationToken? cancel,
            ILogService log,
            IMarketApi api,

            Func<ILogService, IMarketApi, ISyncInformer, string, IList<ListingOrderDTO>> getOrderItemsFromMarketFunc,

            ISyncInformer syncInfo,
            IList<IShipmentApi> rateProviders,
            IQuantityManager quantityManager,
            ISettingsService settings,
            IEmailService emailService,
            IOrderValidatorService validatorService,
            ICacheService cacheService,

            ICompanyAddressService companyAddress,
            DateTime syncMissedFrom,
            ITime time)
        {
            try
            {
                log.Info("Get missed orders");

                SyncResult result = new SyncResult();
                var statusList = new List<string> { OrderStatusEnumEx.Unshipped, OrderStatusEnumEx.PartiallyShipped, OrderStatusEnumEx.Canceled };
                var unshippedOrders = api.GetOrders(log, syncMissedFrom, statusList).ToList();

                if (unshippedOrders.Any())
                {
                    result = ProcessNewOrdersPack(cancel,
                        log,
                        api,

                        getOrderItemsFromMarketFunc,

                        rateProviders,
                        validatorService,
                        syncInfo,
                        settings,
                        quantityManager,
                        emailService,
                        cacheService,

                        companyAddress,
                        unshippedOrders,
                        time);

                    if (!result.IsSuccess)
                    {
                        syncInfo.AddWarning("", "The orders pack processing has failed");
                        return new SyncResult() { IsSuccess = false };
                    }
                }

                return new SyncResult()
                {
                    IsSuccess = true,
                    //NOTE: Return all unshipped orders for correct calculation total orders on Amazon
                    //unprocessed orders actual didn't updated (we can safely skip processing of it)
                    ProcessedOrders = result.ProcessedOrders,
                    SkippedOrders = result.SkippedOrders,
                    /*unshippedOrders.Select(o => new SyncResultOrderInfo()
                    {
                        OrderId = o.OrderId,
                        OrderStatus = o.OrderStatus
                    }).ToList()*/
                };
            }
            catch (Exception ex)
            {
                syncInfo.AddError(null, "Error when storing new unshipped orders", ex);
                log.Error("Error when storing new unshipped orders", ex);
                return new SyncResult() { IsSuccess = false };
            }
        }

        #endregion




        protected IList<ListingOrderDTO> GetOrderItemsFromMarket(ILogService log, IMarketApi api, ISyncInformer syncInfo, string orderId)
        {
            try
            {
                return api.GetOrderItems(orderId).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Error when getting order items", ex);
                syncInfo.AddError(orderId, "Failed getting order items", ex);
                return new List<ListingOrderDTO>();
            }
        }

        protected override void PrepareOrderInfo(DTOOrder marketOrder, IList<ListingOrderDTO> orderItems)
        {
            if (orderItems.Any(oi => oi.OnMarketTemplateName == AmazonTemplateHelper.OversizeTemplate))
            {
                if (marketOrder.Market == (int)MarketType.Amazon
                    && marketOrder.InitialServiceType == ShippingUtils.StandardServiceName)
                    marketOrder.InitialServiceType = ShippingUtils.ExpeditedServiceName;
            }
        }

        protected override bool FillOrderItemsBy(ILogService log,
            IUnitOfWork db,
            IMarketApi api,
            IList<ListingOrderDTO> orderItems) //NOTE: Only using for listings lookup
        {
            //Checking listings (inventory may haven't some listing, ex. not synced new listings)
            //In this case skipping calculation, waiting listing
            var allListingsUpdated = db.Listings.FillOrderItemsBySKU(orderItems,
                api.Market,
                api.MarketplaceId);

            return allListingsUpdated;
        }
    }
}
