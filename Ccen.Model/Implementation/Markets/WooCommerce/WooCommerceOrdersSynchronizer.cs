using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Orders;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Sync;

namespace Amazon.Model.Implementation.Markets.WooCommerce
{
    public class WooCommerceOrdersSynchronizer : BaseOrdersSynchronizer
    {
        public WooCommerceOrdersSynchronizer(ILogService log,
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

        public WooCommerceOrdersSynchronizer(ILogService log,
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

        public override SyncResult ProcessSpecifiedOrder(IUnitOfWork db, string orderId)
        {
            return ProcessSpecifiedOrderInner(db, orderId, GetOrderItemsFromMarket);
        }


        #region Implement Interface
        public override SyncResult Sync(OrderSyncModes syncMode, CancellationToken? token)
        {
            _log.Info("Sync, syncMode=" + syncMode);
            var processedOrderIdList = new List<SyncResultOrderInfo>();
            var isSuccess = true;

            _syncInfo.PingSync();

            var syncResult = SyncAllNewOrders(token,
                _log,
                _api,
                _api.Market,
                GetOrderItemsFromMarket,
                _syncInfo,

                _rateProviders,
                _quantityManager,
                _settings,
                _emailService,
                _validatorService,
                _cacheService,

                _companyAddress,
                DateTime.UtcNow - _settings.GetOrdersSyncForMissedInterval(_api.Market, _api.MarketplaceId),
                _time);

            isSuccess = isSuccess && syncResult.IsSuccess;
            if (syncResult.ProcessedOrders != null)
                processedOrderIdList.AddRange(syncResult.ProcessedOrders);

            _syncInfo.PingSync();

            syncResult = UpdateExistingUnshippedOrders(token,
                                _log,
                                _api,

                                GetOrderItemsFromMarket,

                                _syncInfo,

                                _rateProviders,
                                _quantityManager,
                                _emailService,
                                _validatorService,

                                processedOrderIdList.Select(o => o.OrderId).ToList(),
                                _companyAddress,

                                _time);

            isSuccess = isSuccess && syncResult.IsSuccess;
            if (syncResult.ProcessedOrders != null)
                processedOrderIdList.AddRange(syncResult.ProcessedOrders);

            _syncInfo.PingSync();

            return new SyncResult()
            {
                ProcessedOrders = processedOrderIdList,
                IsSuccess = isSuccess
            };
        }

        #region Sync Actions


        protected SyncResult UpdateExistingUnshippedOrders(CancellationToken? token,
            ILogService log,
            IMarketApi api,

            Func<ILogService, IMarketApi, ISyncInformer, string, IList<ListingOrderDTO>> getOrderItemsFromMarketFunc,

            ISyncInformer syncInfo,
            IList<IShipmentApi> rateProviders,
            IQuantityManager quantityManager,
            IEmailService emailService,
            IOrderValidatorService validatorService,

            IList<string> excludeOrderIdList,

            ICompanyAddressService companyAddress,
            ITime time)
        {
            IList<Order> ordersToUpdate = new List<Order>();

            try
            {
                using (var db = new UnitOfWork(log))
                {
                    var existOrders = db.Orders.GetOrdersByStatus(api.Market,
                        api.MarketplaceId,
                        new[] {
                            OrderStatusEnumEx.Pending,
                            OrderStatusEnumEx.Unshipped,
                            OrderStatusEnumEx.PartiallyShipped
                        }
                    ).ToList();

                    if (excludeOrderIdList != null)
                        ordersToUpdate = existOrders.Where(o => !excludeOrderIdList.Contains(o.AmazonIdentifier)).ToList();
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

                        if (!ProcessExistOrdersPack(token,
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

        protected SyncResult SyncAllNewOrders(CancellationToken? token,
            ILogService log,
            IMarketApi api,
            MarketType market,

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
                var statusList = new List<string> {
                    OrderStatusEnumEx.Canceled,
                    OrderStatusEnumEx.Shipped,
                    OrderStatusEnumEx.Unshipped,
                    OrderStatusEnumEx.Pending }; //NOTE: On API level there status will be converted on eBay status

                var allOrders = api.GetOrders(log, syncMissedFrom, statusList).ToList();

                if (allOrders.Any())
                {
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
                        allOrders,
                        time);

                    if (!result.IsSuccess)
                    {
                        syncInfo.AddWarning("", "The orders pack processing has failed");
                        return new SyncResult() { IsSuccess = false };
                    }

                    return result;
                }

                return new SyncResult()
                {
                    IsSuccess = true,
                    ProcessedOrders = new List<SyncResultOrderInfo>()
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
            return new List<ListingOrderDTO>();
        }

        protected override bool FillOrderItemsBy(ILogService log,
            IUnitOfWork db,
            IMarketApi api,
            IList<ListingOrderDTO> orderItems) //NOTE: only using for listings lookup (on Amazon)
        {
            //Check order listings
            var allListingsUpdated = db.Listings.FillOrderItemsBySKU(orderItems, api.Market, api.MarketplaceId);
            return allListingsUpdated;
        }

        #endregion
    }
}
