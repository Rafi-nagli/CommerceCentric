using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Web.Mvc;
using System.Web.Security;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Sync;
using Amazon.Model.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllAdmins)]
    public partial class AdminController : BaseController
    {
        public override string TAG
        {
            get { return "AdminController."; }
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult RestartDWSService()
        {
            LogService.Info("Restart DWS Service Action");

            SystemActions.AddAction(Db,
                SystemActionType.RestartService,
                "",
                null,
                null,
                AccessManager.UserId);

            return JsonGet(new { success = true });
        }


        public virtual ActionResult UpdateCaches()
        {
            var entry = LogService.Info("UpdateCaches begin");
            MessageResult result = null;

            try
            {
                //Note: only for admin (for debugging purposes)
                Settings.SetCacheUpdateInProgress(false);
                var success = Cache.UpdateDbCacheUsingSettings(Db, Settings);

                if (success)
                    result = MessageResult.Success("Success updated");
                else
                    result = MessageResult.Error("Cache updating already in progress");
            }
            catch (Exception ex)
            {
                LogService.Error("UpdateCaches", ex, entry);
                result = MessageResult.Error(ex.Message);
            }

            LogService.Info("UpdateCaches end", entry);

            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult ResetShippings(string orderId)
        {
            LogI("ResetShippings begin, orderid=" + orderId);
            var result = MessageResult.Error("Undefined");

            if (!string.IsNullOrEmpty(orderId))
            {
                var syncInfo = new EmptySyncInformer(LogService, SyncType.Orders);
                var rateProviders = ServiceFactory.GetShipmentProviders(LogService,
                    Time,
                    DbFactory,
                    WeightService,
                    AccessManager.Company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

                DTOOrder dtoOrder = Db.ItemOrderMappings.GetOrderWithItems(WeightService, orderId, unmaskReferenceStyle: false, includeSourceItems: true);
                if (dtoOrder != null)
                {
                    dtoOrder.OrderStatus = OrderStatusEnumEx.Unshipped;

                    var synchronizer = new AmazonOrdersSynchronizer(LogService,
                        AccessManager.Company,
                        syncInfo,
                        rateProviders,
                        CompanyAddress,
                        Time,
                        WeightService,
                        MessageService);

                    if (synchronizer.UIUpdate(Db, dtoOrder, true, false, keepCustomShipping:false, switchToMethodId: null))
                    {
                        var dbOrder = Db.Orders.Get(dtoOrder.Id);
                        dbOrder.OrderStatus = OrderStatusEnum.Unshipped.ToString();
                        dbOrder.UpgradeLevel = null;
                        Db.Commit();

                        result = MessageResult.Success("Success updates");
                    }
                }
                else
                {
                    result = MessageResult.Error("Not found OrderId: " + orderId);
                }
            }
            else
            {
                result = MessageResult.Error("OrderId is empty");
            }
            
            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
