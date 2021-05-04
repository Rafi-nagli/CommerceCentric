using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Orders;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateOrdersFromOrderApiThread : ThreadBase
    {
        private readonly IMarketApi _api;
        private MarketType _market;
        private DateTime? _lastFullSync = null;
        private TimeSpan _fullSyncInterval = new TimeSpan(2, 0, 0);

        public UpdateOrdersFromOrderApiThread(string logTag,
            IMarketApi api,
            long companyId,
            ISystemMessageService messageService,
            TimeSpan? callbackInterval) : base(logTag, companyId, messageService, callbackInterval)
        {
            _api = api;
            _market = api.Market;
        }

        protected override void RunCallback()
        {
            _api.Connect();

            var dbFactory = new DbFactory();
            var time = new TimeService(dbFactory);
            var log = GetLogger();

            var syncInfo = new DbSyncInformer(dbFactory,
                    log,
                    time,
                    SyncType.Orders,
                    _api.MarketplaceId,
                    _market,
                    String.Empty);

            using (var db = dbFactory.GetRWDb())
            {
                var serviceFactory = new ServiceFactory();
                
                var settings = new SettingsService(dbFactory);
                var company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
                var companyAddress = new CompanyAddressService(company);
                
                var shipmentProviders = company.ShipmentProviderInfoList;
                var addressProviders = company.AddressProviderInfoList;

                var addressCheckServiceList = serviceFactory.GetAddressCheckServices(log,
                    time,
                    dbFactory,
                    addressProviders);
                var addressService = new AddressService(addressCheckServiceList, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));

                var actionService = new SystemActionService(log, time);
                var priceService = new PriceService(dbFactory);
                var quantityManager = new QuantityManager(log, time);
                var emailService = new EmailService(log, 
                    SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels),
                    addressService);

                var weightService = new WeightService();
                var messageService = new SystemMessageService(log, time, dbFactory);

                var rateProviders = serviceFactory.GetShipmentProviders(log,
                    time,
                    dbFactory,
                    weightService,
                    shipmentProviders,
                    null,
                    null,
                    null,
                    null);

                var htmlScraper = new HtmlScraperService(log, time, dbFactory);
                var orderHistoryService = new OrderHistoryService(log, time, dbFactory);
                var validatorService = new OrderValidatorService(log,
                    dbFactory,
                    emailService,
                    settings,
                    orderHistoryService,
                    actionService,
                    priceService,
                    htmlScraper,
                    addressService,
                    companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                    rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.Stamps),
                    time,
                    company);
                
                var cacheService = new CacheService(log, time, actionService, quantityManager);

                var orderSyncFactory = new OrderSyncFactory();

                if (settings.GetOrdersSyncEnabled() != false)
                {
                    if (!syncInfo.IsSyncInProgress()) //NOTE: for now it a few minutes ~10
                    {
                        if (!IsPrintLabelsInProgress(db, actionService, time))
                        {
                            try
                            {
                                var marketplaceId = _api.MarketplaceId;

                                LogWrite("Set OrderSyncInProgress");
                                syncInfo.SyncBegin(null);

                                var synchronizer = orderSyncFactory.GetForMarket(_api,
                                    GetLogger(),
                                    company,
                                    settings,
                                    syncInfo,
                                    rateProviders,
                                    quantityManager,
                                    emailService,
                                    validatorService,
                                    orderHistoryService,
                                    cacheService,
                                    actionService,
                                    companyAddress,
                                    time,
                                    weightService,
                                    messageService);

                                var isFullSync = !_lastFullSync.HasValue || (time.GetUtcTime() - _lastFullSync) > _fullSyncInterval;

                                var syncResult = synchronizer.Sync(isFullSync ? OrderSyncModes.Full : OrderSyncModes.Fast, CancellationToken);

                                if (isFullSync)
                                    _lastFullSync = time.GetUtcTime();

                                var statusList = new List<string>() {OrderStatusEnum.Unshipped.Str()};
                                if (_market == MarketType.Walmart
                                    || _market == MarketType.WalmartCA)
                                {
                                    statusList.Add(OrderStatusEnum.Pending.Str());
                                }

                                var dbOrderIdList = (from o in db.Orders.GetAll()
                                                     join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                                     where (sh.IsActive || sh.IsVisible)
                                                        && statusList.Contains(o.OrderStatus)
                                                        && o.Market == (int)_market
                                                        && (o.MarketplaceId == marketplaceId || String.IsNullOrEmpty(marketplaceId))
                                                     select o.AmazonIdentifier).Distinct().ToList();
                                //var dbOrders = db.ItemOrderMappings.GetOrdersWithItemsByStatus(weightService, statusList.ToArray(), _market, marketplaceId).ToList();
                                //dbOrders = dbOrders.Where(o => o.ShippingInfos != null && o.ShippingInfos.Any(sh => sh.IsActive || sh.IsVisible)).ToList();

                                
                                var unshippedMarketOrderIdList = syncResult.ProcessedOrders
                                    .Where(o => statusList.Contains(o.OrderStatus))
                                    .Select(o => o.OrderId)
                                    .ToList();

                                if (syncResult.SkippedOrders != null)
                                    unshippedMarketOrderIdList.AddRange(syncResult.SkippedOrders
                                        .Where(o => statusList.Contains(o.OrderStatus))
                                        .Select(o => o.OrderId)
                                        .ToList());

                                unshippedMarketOrderIdList = unshippedMarketOrderIdList.Distinct().ToList();

                                //var dbOrderIdList = dbOrders.Select(o => o.OrderId).Distinct().ToList();
                                LogDiffrents(unshippedMarketOrderIdList, dbOrderIdList, "Missing order: ");

                                if (unshippedMarketOrderIdList.Count != dbOrderIdList.Count
                                        || !syncResult.IsSuccess)
                                {
                                    emailService.SendSystemEmailToAdmin("PA Orders Sync has issue",
                                        "Market: " + _api.Market + " - " + _api.MarketplaceId + "<br/>" +
                                        "Sync message: " + syncResult.Message + "<br/>" +
                                        "Missing orders: " + (unshippedMarketOrderIdList.Count - dbOrderIdList.Count));
                                }

                                //NOTE: otherwise if we have missed order (older than 2 hours that was hidden in next lite iteration)
                                if (isFullSync)
                                {
                                    settings.SetOrderCountOnMarket(unshippedMarketOrderIdList.Count, _market, marketplaceId);
                                    settings.SetOrderCountInDB(dbOrderIdList.Count, _market, marketplaceId);
                                }

                                if (syncResult.IsSuccess)
                                {
                                    settings.SetOrderSyncDate(time.GetUtcTime(), _market, marketplaceId);
                                }
                            }
                            catch (Exception ex)
                            {
                                emailService.SendSystemEmailToAdmin("PA Orders Sync has error",
                                    "Market: " + _api.Market + " - " + _api.MarketplaceId + "<br/>" +
                                    "Sync message: " + ExceptionHelper.GetAllMessages(ex));
                                LogError("RunCallback", ex);
                            }
                            finally
                            {
                                syncInfo.SyncEnd();
                            }
                        }
                        else
                        {
                            LogWrite("Labels printing in-progress");
                        }
                    }
                    else
                    {
                        LogWrite("Order Sync already runned");
                    }
                }
            }
        }

        private bool IsPrintLabelsInProgress(IUnitOfWork db, ISystemActionService actionService, ITime time)
        {
            var after = time.GetUtcTime().AddHours(-1);
            var printInProgressList = actionService.GetInProgressByType(db, SystemActionType.PrintBatch, after);
            return printInProgressList.Any();
        }

        private void LogDiffrents(IList<string> original, IList<string> current, string prefix)
        {
            foreach (var item in original)
            {
                if (!current.Contains(item))
                    LogWrite(prefix + item);
            }
        }
    }
}
