using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Orders;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;

namespace Amazon.Model.Implementation.Labels
{
    public class LabelAutoBuyService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IBatchManager _batchManager;
        private ILabelBatchService _labelBatchService;
        private ISystemActionService _actionService;
        private IEmailService _emailService;
        private IWeightService _weightService;
        private long _companyId;

        public LabelAutoBuyService(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            IBatchManager batchManager,
            ILabelBatchService labelBatchService,
            ISystemActionService actionService,
            IEmailService emailService,
            IWeightService weightService,
            long companyId)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
            _batchManager = batchManager;
            _labelBatchService = labelBatchService;
            _actionService = actionService;
            _emailService = emailService;
            _companyId = companyId;
            _weightService = weightService;
        }

        public class OverdueInfo
        {
            public long OrderId { get; set; } 
            public long? BatchId { get; set; }
        }

        public static IList<OverdueInfo> GetOverdueOrderInfos(IUnitOfWork db, DateTime when)
        {
            var date = when.AddHours(-4).Date;
            var tillDate = date.AddDays(1).AddHours(6);
            
            var overdueOrders = db.Orders.GetAll()
                .Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped
                    && !o.OnHold
                    && o.LatestShipDate < tillDate
                    && o.DropShipperId == DSHelper.DefaultPAId)
                .ToList();

            //Exclude orders from batch in-progress
            var batchInProcessIdList = db.OrderBatches.GetAll().Where(b => b.PrintStatus == (int)BatchPrintStatuses.InProgress)
                    .Select(b => b.Id)
                    .ToList();

            if (batchInProcessIdList.Any())
            {
                overdueOrders = overdueOrders.Where(o => !o.BatchId.HasValue || !batchInProcessIdList.Contains(o.BatchId.Value))
                        .ToList();
            }

            return overdueOrders.Select(o => new OverdueInfo()
            {
                OrderId = o.Id,
                BatchId = o.BatchId
            }).ToList();
        }


        public void PurchaseForOverdue()
        {
            var when = _time.GetAppNowTime();
            var date = when.AddHours(-4).Date;

            using (var db = _dbFactory.GetRWDb())
            {
                var overdueOrderIds = GetOverdueOrderInfos(db, _time.GetAppNowTime()).Select(i => i.OrderId).ToList();

                _log.Info("Overdue orders count=" + overdueOrderIds.Count);

                if (overdueOrderIds.Count > 5) //NOTE: print only when a lot of overdue
                {
                    var orderIdList = overdueOrderIds;
                    var batchName = date.ToString("MM/dd/yyyy") + " Overdue";

                    var batchId = _batchManager.CreateBatch(db,
                        BatchType.AutoBuy,
                        batchName,
                        orderIdList,
                        when,
                        null);

                    _batchManager.LockBatch(db,
                        batchId,
                        when);

                    var actionId = _actionService.AddAction(db,
                        SystemActionType.PrintBatch,
                        batchId.ToString(),
                        new PrintBatchInput()
                        {
                            BatchId = batchId,
                            CompanyId = _companyId,
                            UserId = null
                        },
                        null,
                        null);

                    _log.Info("PrintLabelsForBatch, add print action, id=" + actionId);
                }
            }
        }

        public void PurchaseForSameDay()
        {
            var when = _time.GetAppNowTime();

            using (var db = _dbFactory.GetRWDb())
            {
                var date = when.AddHours(-4).Date;

                var originalSameDaysOrders = db.Orders
                    .GetAll()
                    .Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped
                                && o.InitialServiceType == ShippingUtils.SameDayServiceName)
                    .ToList();

                _log.Info("Same Day orders, original count=" + originalSameDaysOrders.Count);
                
                if (originalSameDaysOrders.Count > 0) //NOTE: print only when a lot of overdue
                {
                    var queryTotalSameDayOrderIdList = from o in db.Orders.GetAll()
                                                       join sh in db.OrderShippingInfos.GetAll()
                                                           on o.Id equals sh.OrderId
                                                       where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                                             && sh.ShippingMethodId == ShippingUtils.DynamexPTPSameShippingMethodId
                                                       select o.Id;


                    var orderIdList = queryTotalSameDayOrderIdList.Distinct().ToList();
                    _log.Info("Same Day orders, total count=" + orderIdList.Count);

                    var batchName = date.ToString("MM/dd/yyyy") + " Same Day";

                    var batchId = _batchManager.CreateBatch(db,
                        BatchType.AutoBuy,
                        batchName,
                        orderIdList,
                        when,
                        null);

                    _batchManager.LockBatch(db,
                        batchId,
                        when);

                    var actionId = _actionService.AddAction(db,
                        SystemActionType.PrintBatch,
                        batchId.ToString(),
                        new PrintBatchInput()
                        {
                            BatchId = batchId,
                            CompanyId = _companyId,
                            UserId = null
                        },
                        null,
                        null);

                    _log.Info("PrintLabelsForBatch, add print action, id=" + actionId);
                }
            }
        }

        public void PurchaseForPrime()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var toPrintPrimeOrderIds = (from o in db.Orders.GetAll()
                    join sh in db.OrderShippingInfos.GetAll()
                        on o.Id equals sh.OrderId
                    where o.OrderStatus == OrderStatusEnumEx.Unshipped
                          && o.IsForceVisible != true
                          && !o.BatchId.HasValue
                          && o.OrderType == (int) OrderTypeEnum.Prime
                          && sh.IsActive
                          && sh.ShippingMethodId == (int) ShippingUtils.AmazonPriorityFlatShippingMethodId
                    select o.Id).ToList();

                _log.Info("PurchaseForPrime. Prime orders, count=" + toPrintPrimeOrderIds.Count);

                if (toPrintPrimeOrderIds.Count > 0) //NOTE: print only when a lot of overdue
                {
                    var orderIdList = toPrintPrimeOrderIds.Distinct().ToList();

                    foreach (var orderId in orderIdList)
                    {
                        _emailService.SendSystemEmailToAdmin("Auto-print prime order " + orderId, "");

                        var result = _labelBatchService.PrintLabel(orderId, _companyId, null);
                        if (result.Success)
                        {
                            _log.Info("IsForceVisible=true");
                            var dbOrder = db.Orders.Get(orderId);
                            dbOrder.IsForceVisible = true;
                            db.Commit();
                        }
                        else
                        {
                            _emailService.SendSystemEmailToAdmin("Issue with auto-print prime order " + orderId, "");
                        }

                        _log.Info("PurchaseForPrime, printed orderId=" + orderId + ", result=" + result.Success);
                    }
                }
            }
        }

        public void PurchaseAmazonNextDay()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var toPrintNextDayOrderIds = (from o in db.Orders.GetAll()
                                            join sh in db.OrderShippingInfos.GetAll()
                                                on o.Id equals sh.OrderId
                                            where o.OrderStatus == OrderStatusEnumEx.Unshipped
                                                  && o.Market == (int)MarketType.Amazon
                                                  && !o.BatchId.HasValue
                                                  && o.InitialServiceType == ShippingUtils.NextDayServiceName
                                                  //&& sh.IsActive
                                                  && sh.ShippingMethodId == (int)ShippingUtils.AmazonExpressFlatShippingMethodId
                                            select o.Id).ToList();

                _log.Info("PurchaseAmazonNextDay. Amazon NextDay orders, count=" + toPrintNextDayOrderIds.Count);

                if (toPrintNextDayOrderIds.Count > 0) //NOTE: print only when a lot of overdue
                {
                    var orderIdList = toPrintNextDayOrderIds.Distinct().ToList();

                    foreach (var orderId in orderIdList)
                    {   
                        var order = db.ItemOrderMappings.GetOrderWithItems(_weightService, orderId, false, false, false);
                        var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.Items);
                        var isSupportFlat = ShippingServiceUtils.IsSupportFlatEnvelope(orderItemInfoes);

                        if (!isSupportFlat)
                        {
                            _log.Info("Not support flat");
                            continue;
                        }

                        var orderShippings = db.OrderShippingInfos.GetAll().Where(o => o.OrderId == orderId).ToList();
                        if (orderShippings.Where(sh => sh.ShippingMethodId == ShippingUtils.AmazonExpressFlatShippingMethodId
                            && sh.IsActive).Count() == 0)
                        {
                            _log.Info("Set active shipping method to USPS Express Flat");
                            orderShippings.ForEach(sh =>
                            {
                                sh.IsActive = sh.ShippingMethodId == ShippingUtils.AmazonExpressFlatShippingMethodId;
                                sh.IsVisible = sh.ShippingMethodId == ShippingUtils.AmazonExpressFlatShippingMethodId;
                            });
                            db.Commit();
                        }

                        _emailService.SendSystemEmailToAdmin("Auto-print Amazon Next Day order " + orderId, "");

                        var result = _labelBatchService.PrintLabel(orderId, _companyId, null);
                        if (result.Success)
                        {
                            _log.Info("IsForceVisible=true");
                            var dbOrder = db.Orders.Get(orderId);
                            dbOrder.IsForceVisible = true;
                            db.Commit();
                        }
                        else
                        {
                            _emailService.SendSystemEmailToAdmin("Issue with auto-print Amazon NextDay order " + orderId, "");
                        }

                        _log.Info("PurchaseAmazonNextDay, printed orderId=" + orderId + ", result=" + result.Success);
                    }
                }
            }
        }
    }
}
