using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation
{
    public class RateService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private IWeightService _weightService;
        private ISystemMessageService _messageService;
        private ISystemActionService _actionService;
        private CompanyDTO _company;
        private IList<IShipmentApi> _rateProviders;

        public RateService(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            IWeightService weightService,
            ISystemMessageService messageService,
            CompanyDTO company,
            ISystemActionService actionService,
            IList<IShipmentApi> rateProviders)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
            _weightService = weightService;
            _messageService = messageService;
            _actionService = actionService;
            _company = company;
            _rateProviders = rateProviders;
        }

        public void RefreshSuspiciousFedexRates()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);

            using (var db = _dbFactory.GetRWDb())
            {
                var toRefreshOrderIds = (from o in db.Orders.GetAll()
                                         join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                         where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                            && sh.IsActive
                                            && (sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayEnvelope
                                            || sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak)
                                            && sh.StampsShippingCost > 12
                                         select o.Id).ToList();

                _log.Info("Orders to update: " + String.Join(", ", toRefreshOrderIds.Select(o => o).ToList()));

                var orderIdList = toRefreshOrderIds.ToArray();
                if (!orderIdList.Any())
                    return;

                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetSelectedOrdersWithItems(_weightService, orderIdList, includeSourceItems: true).ToList();
                foreach (var dtoOrder in dtoOrders)
                {
                    //Ignore shipped orders
                    if (ShippingUtils.IsOrderShipped(dtoOrder.OrderStatus)
                        || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.LabelPath))
                        || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.TrackingNumber)))
                    {
                        _log.Info("OrderId: " + dtoOrder.Id + " (" + dtoOrder.OrderId + ") - Skipped (has shipped status or has tr#)");
                    }
                    else
                    {
                        _log.Info("Add recalc action for OrderId=" + dtoOrder.OrderId);

                        _actionService.AddAction(db,
                            SystemActionType.UpdateRates,
                            dtoOrder.OrderId,
                            new UpdateRatesInput()
                            {
                                OrderId = dtoOrder.Id
                            },
                            null,
                            null);
                    }
                }
            }
        }

        public void RefreshAmazonRates()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);

            using (var db = _dbFactory.GetRWDb())
            {
                var toRefreshOrderIds = (from o in db.Orders.GetAll()
                                  join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                  where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                     && sh.IsActive
                                     && (o.Market == (int)MarketType.Amazon
                                        || o.Market == (int)MarketType.AmazonAU
                                        || o.Market == (int)MarketType.AmazonEU)
                                     && (sh.ShippingMethodId == (int)ShippingUtils.AmazonFedExExpressSaverShippingMethodId //TEMP: old
                                         || sh.ShippingMethodId == (int)ShippingUtils.AmazonFedExHomeDeliveryShippingMethodId //TEMP: old
                                         || o.SourceShippingService == ShippingUtils.SecondDayServiceName
                                         || o.SourceShippingService == ShippingUtils.NextDayServiceName
                                         || o.OrderType == (int)OrderTypeEnum.Prime)
                                  select o.Id).ToList();

                _log.Info("Orders to update: " + String.Join(", ", toRefreshOrderIds.Select(o => o).ToList()));

                var orderIdList = toRefreshOrderIds.ToArray();
                if (!orderIdList.Any())
                    return;

                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetSelectedOrdersWithItems(_weightService, orderIdList, includeSourceItems: true).ToList();
                foreach (var dtoOrder in dtoOrders)
                {
                    //Ignore shipped orders
                    if (ShippingUtils.IsOrderShipped(dtoOrder.OrderStatus)
                        || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.LabelPath))
                        || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.TrackingNumber)))
                    {
                        _log.Info("OrderId: " + dtoOrder.Id + " (" + dtoOrder.OrderId + ") - Skipped (has shipped status or has tr#)");
                    }
                    else
                    {
                        _log.Info("Add recalc action for OrderId=" + dtoOrder.OrderId);

                        _actionService.AddAction(db,
                            SystemActionType.UpdateRates,
                            dtoOrder.OrderId,
                            new UpdateRatesInput()
                            {
                                OrderId = dtoOrder.Id
                            },
                            null,
                            null);
                    }
                }
            }
        }

        public static decimal GetMarketShippingAmount(MarketType market, string marketplaceId)
        {
            //var usShipping = 4.49M;
            //var caShipping = 9.49M;
            //var wmCAShipping = 4.99M;
            //var ukShipping = 9.49M;
            //var auShipping = 9.49M;
            //var caExtra = 1.00M;
            //var ukExtra = 1.00M;
            //var auExtra = 1.00M;

            if ((market == MarketType.Amazon
                && marketplaceId == MarketplaceKeeper.AmazonComMarketplaceId)
                || market == MarketType.Walmart)
                return 4.49M;

            if (market == MarketType.Amazon
                && marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                return 4.99M;

            if (market == MarketType.AmazonEU)
                return 9.49M;

            if (market == MarketType.AmazonAU)
                return 4.54M;

            if (market == MarketType.WalmartCA)
                return 4.99M;

            return 9.49M;
        }

        public static decimal GetMarketExtraAmount(MarketType market, string marketplaceId)
        {
            //var usShipping = 4.49M;
            //var caShipping = 9.49M;
            //var wmCAShipping = 4.99M;
            //var ukShipping = 9.49M;
            //var auShipping = 9.49M;
            //var caExtra = 1.00M;
            //var ukExtra = 1.00M;
            //var auExtra = 1.00M;

            if (market == MarketType.Amazon
                && marketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId)
                return 1M;

            if (market == MarketType.AmazonEU)
                return 1M;

            if (market == MarketType.AmazonAU)
                return 1M;

            if (market == MarketType.eBay)
                return 3M;
            
            return 0M;
        }


        public void ProcessSystemAction(CancellationToken cancelToken)
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);

            using (var db = _dbFactory.GetRWDb())
            {
                var updateRateActions = _actionService.GetUnprocessedByType(db, SystemActionType.UpdateRates, null, null);

                foreach (var action in updateRateActions)
                {
                    if (cancelToken.IsCancellationRequested)
                        return;

                    var actionStatus = SystemActionStatus.None;
                    try
                    {
                        var inputData = SystemActionHelper.FromStr<UpdateRatesInput>(action.InputData);
                        var orderId = inputData.OrderId;
                        if (!orderId.HasValue && !String.IsNullOrEmpty(inputData.OrderNumber))
                        {
                            var order = db.Orders.GetByOrderNumber(inputData.OrderNumber);
                            orderId = order.Id;
                        }

                        if (orderId.HasValue)
                        {
                            var orderIdList = new long[] {orderId.Value};
                            IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetSelectedOrdersWithItems(_weightService, orderIdList, includeSourceItems: true).ToList();

                            foreach (var dtoOrder in dtoOrders)
                            {
                                //Ignore shipped orders
                                if ((dtoOrder.Market != (int) MarketType.eBay &&
                                     ShippingUtils.IsOrderShipped(dtoOrder.OrderStatus))
                                    || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.LabelPath))

                                    || dtoOrder.BatchId.HasValue) //NOTE: SKip orders in batch
                                {
                                    actionStatus = SystemActionStatus.Skipped;
                                }
                                else
                                {
                                    var markets = new MarketplaceKeeper(_dbFactory, false);
                                    markets.Init();
                                    var companyAddress = new CompanyAddressService(_company, markets.GetAll());
                                    var synchronizer = new AmazonOrdersSynchronizer(_log,
                                        _company,
                                        syncInfo,
                                        _rateProviders,
                                        companyAddress,
                                        _time,
                                        _weightService,
                                        _messageService);
                                    if (!synchronizer.UIUpdate(db, dtoOrder, false, false, keepCustomShipping: false, switchToMethodId: null))
                                    {
                                        actionStatus = SystemActionStatus.Fail;
                                    }
                                    else
                                    {
                                        actionStatus = SystemActionStatus.Done;
                                    }
                                }
                            }
                            
                            _log.Info("Order rates was recalculated, actionId=" + action.Id + ", status=" + actionStatus);
                        }
                        else
                        {
                            actionStatus = SystemActionStatus.NotFoundEntity;
                            _log.Info("Can't find order, actionId=" + action.Id + ", orderId=" + inputData.OrderId + ", orderNumber=" + inputData.OrderNumber);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Fail recalculate order rates action, actionId=" + action.Id, ex);
                        actionStatus = SystemActionStatus.Fail;
                    }

                    _actionService.SetResult(db, 
                        action.Id,
                        actionStatus,
                        null,
                        null);
                }

                db.Commit();
            }
        }
    }
}
