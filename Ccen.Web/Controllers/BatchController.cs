using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.SessionState;
using Amazon.Common.Helpers;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Orders;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public partial class BatchController : BaseController
    {
        public override string TAG
        {
            get { return "BatchController."; }
        }

        public virtual ActionResult Batches()
        {
            LogI("Batches begin");
            return View();
        }

        public virtual ActionResult ActiveBatches(long? batchId)
        {
            LogI("ActiveBatches, batchId=" + batchId);

            var model = new BatchCollection
            {
                Batches = OrderBatchViewModel.GetAllForTabs(Db, batchId)
            };
            if (model.Batches.Any() && model.Batches.All(b => !b.Selected))
            {
                model.Batches.First().Selected = true;
            }
            return View(model);
        }

        public virtual ActionResult GetBatches([DataSourceRequest] DataSourceRequest request, bool? showArchive)
        {
            LogI("GetBatches begin, showArchive=" + showArchive);

            try
            {
                var items = OrderBatchViewModel.GetAll(Db.OrderBatches, showArchive ?? false);
                var dataSource = items.ToDataSourceResult(request);
                return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            catch (Exception ex)
            {
                LogE("GetBatches begin", ex);
                return new JsonResult { Data = new List<OrderBatchViewModel>(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public virtual ActionResult CreateBatch(CreateBatchViewModel model)
        {
            LogI("CreateBatch begin, orderIds=" + model.OrderIds + ", batchName=" + model.BatchName);

            var result = OrderBatchViewModel.CreateBatch(Db,
                BatchManager,
                model.OrderIds,
                model.BatchName,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual ActionResult AddOrdersToBatch(CreateBatchViewModel model)
        {
            LogI("CreateBatch begin, orderIds=" + model.OrderIds + ", batchId=" + model.BatchId);

            var result = OrderBatchViewModel.AddOrdersToBatch(Db, 
                OrderHistoryService,
                model.BatchId, 
                model.OrderIds,
                Time.GetAppNowTime(),
                AccessManager.UserId);

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public virtual ActionResult CheckAddOrdersToBatch(CreateBatchViewModel model)
        {
            LogI("CheckAddOrdersToBatch begin, orderIds=" + model.OrderIds + ", batchId=" + model.BatchId);

            var result = OrderBatchViewModel.CheckAddOrdersToBatch(Db,
               model.OrderIds);

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult LockBatch(long batchId)
        {
            LogI("LockBatch, batchId=" + batchId);

            var result = OrderBatchViewModel.LockBatch(Db, BatchManager, batchId, Time.GetAppNowTime());

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult RemoveFromBatch(long batchId, long orderId)
        {
            LogI("RemoveFromBatch begin, batchId=" + batchId + ", orderId=" + orderId);

            var result = OrderBatchViewModel.RemoveFromBatch(Db, 
                LogService, 
                SystemActions, 
                OrderHistoryService,
                BatchManager,
                batchId, 
                orderId, 
                AccessManager.UserId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        

        public virtual ActionResult RemoveMultipleFromBatch(long batchId,
            string orderIds,
            long? toBatchId,
            bool? removeOnHold)
        {
            LogI("RemoveMultipleFromBatch begin, batchId=" + batchId + ", orderIds=" + orderIds);

            var result = OrderBatchViewModel.RemoveMultipleFromBatch(Db,
                LogService,
                SystemActions,
                OrderHistoryService,
                BatchManager,
                batchId,
                orderIds,
                toBatchId,
                removeOnHold);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetDayBatchNumber()
        {
            var now = Time.GetAppNowTime();
            DateTime from;
            if (now.TimeOfDay < TimeSpan.FromHours(21))
            {
                from = new DateTime(now.Year, now.Month, now.Day, 21, 0, 0).AddDays(-1);
            }
            else
            {
                from = new DateTime(now.Year, now.Month, now.Day, 21, 0, 0);
            }
            var batchCount = Db.OrderBatches.GetBatchesToDisplay(true)
                .Where(b => b.Type == (int)BatchType.User)
                .Count(b => b.CreateDate >= from);

            return JsonGet(new ValueResult<int>(true, "", batchCount));
        }

        [HttpPost]
        public virtual ActionResult ReCalcShippingService(long batchId, 
            string orderIds, 
            int? switchToProviderId,
            int? switchToMethodId)
        {
            LogI("ReCalcShippingService begin, orderIds=" + orderIds);

            IList<string> failedUpdate = new List<string>();
            IList<string> successUpdate = new List<string>();
            if (!string.IsNullOrEmpty(orderIds))
            {
                var stringOrderIdList = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var orderIdList = stringOrderIdList.Select(long.Parse).ToArray();
                var rateProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.Company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

                var syncInfo = new EmptySyncInformer(LogService, SyncType.Orders);

                IList<DTOOrder> dtoOrders = Db.ItemOrderMappings.GetSelectedOrdersWithItems(WeightService, orderIdList, includeSourceItems: true).ToList();
                foreach (var dtoOrder in dtoOrders)
                {
                    //Ignore shipped orders
                    if ((dtoOrder.Market != (int)MarketType.eBay && ShippingUtils.IsOrderShipped(dtoOrder.OrderStatus))
                        || dtoOrder.ShippingInfos.Any(s => !String.IsNullOrEmpty(s.LabelPath)))
                    {
                        failedUpdate.Add(dtoOrder.OrderId);
                        continue;
                    }

                    if (switchToProviderId.HasValue
                        && dtoOrder.ShipmentProviderType != switchToProviderId.Value)
                    {
                        var skipChanges = false;
                        if (switchToProviderId == (int)ShipmentProviderType.FedexOneRate)
                        {
                            if (ShippingUtils.IsInternational(dtoOrder.FinalShippingCountry))
                                skipChanges = true;
                        }

                        if (!skipChanges)
                        {
                            var dbOrder = Db.Orders.Get(dtoOrder.Id);
                            dbOrder.ShipmentProviderType = switchToProviderId.Value;
                            Db.Commit();
                            dtoOrder.ShipmentProviderType = switchToProviderId.Value;
                        }
                    }

                    var synchronizer = new AmazonOrdersSynchronizer(LogService,
                        AccessManager.Company,
                        syncInfo,
                        rateProviders, 
                        CompanyAddress,
                        Time,
                        WeightService,
                        MessageService);

                    if (synchronizer.UIUpdate(Db, dtoOrder, false, false, keepCustomShipping: false, switchToMethodId: switchToMethodId))
                    {
                        successUpdate.Add(dtoOrder.OrderId);
                    }
                    else
                    {
                        failedUpdate.Add(dtoOrder.OrderId);
                    }
                }
            }
            LogI("ReCalcShippingService result, failedUpdate=" + String.Join(", ", failedUpdate) 
                + ", successUpdate=" + String.Join(", ", successUpdate));

            return new JsonResult
            {
                Data = ValueResult<IList<string>>.Success("", new List<string>() { String.Join(", ", failedUpdate), String.Join(", ", successUpdate) }), 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult UpgradeShippingService(long batchId, string orderIds)
        {
            LogI("UpgradeShippingService begin, batchId=" + batchId + ", orderId=" + orderIds);

            IList<string> failedUpdate = new List<string>();
            if (!string.IsNullOrEmpty(orderIds))
            {
                var stringOrderIdList = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var orderIdList = stringOrderIdList.Select(long.Parse).ToList();
                var rateProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.Company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

                failedUpdate = OrderViewModel.UpgradeShippingService(Db, WeightService, rateProviders, orderIdList);
            }
            LogI("UpgradeShippingService result, failedUpdate=" + String.Join(", ", failedUpdate));

            return new JsonResult
            {
                Data = MessageResult.Success("", String.Join(", ", failedUpdate)), 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


        public virtual ActionResult DowngradeShippingService(long batchId, string orderIds)
        {
            LogI("DownGradeShippingService begin, batchId=" + batchId + ", orderId=" + orderIds);

            IList<string> failedUpdate = new List<string>();
            if (!string.IsNullOrEmpty(orderIds))
            {
                var stringOrderIdList = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var orderIdList = stringOrderIdList.Select(long.Parse).ToList();
                var rateProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.Company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

                failedUpdate = OrderViewModel.DowngradeShippingService(Db, WeightService, rateProviders, orderIdList);
            }
            LogI("DownGradeShippingService result, failedUpdate=" + String.Join(", ", failedUpdate));

            return new JsonResult
            {
                Data = MessageResult.Success("", String.Join(", ", failedUpdate)), 
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult ToggleArchive(long batchId)
        {
            LogI("ToggleArchive begin, batchId=" + batchId);
            var newStatus = OrderBatchViewModel.ToggleBatchArchive(Db, batchId);

            return Json(newStatus, JsonRequestBehavior.AllowGet);
        }

        //[AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult UpdateBatch([DataSourceRequest] DataSourceRequest request, OrderBatchViewModel batch)
        {
            LogI("UpdateBatch begin, batch=" + batch);

            if (batch != null && ModelState.IsValid)
            {
                OrderBatchViewModel.UpdateBatchName(Db,
                    batch.Id,
                    batch.Name,
                    Time.GetAppNowTime(),
                    AccessManager.UserId);
            }

            return Json(new[] { batch }.ToDataSourceResult(request, ModelState), JsonRequestBehavior.AllowGet);
        }
    }
}
