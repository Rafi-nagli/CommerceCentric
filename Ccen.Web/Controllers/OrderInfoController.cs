using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Addresses;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Sync;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Orders;
using Amazon.Web.ViewModels.Pages;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public partial class OrderInfoController : BaseController
    {
        public override string TAG
        {
            get { return "OrderInfoController."; }
        }

        [OutputCache(CacheProfile = "MiddleTimeProfile")]
        public virtual ActionResult Stats()
        {
            LogI("Stats");

            var marketplaceManager = new MarketplaceKeeper(DbFactory, false);
            marketplaceManager.Init();

            var model = OrdersStatViewModel.Get(Db, marketplaceManager);
            return View(model);
        }

        public virtual ActionResult Orders(string orderId)
        {
            LogI("Orders");
            var model = new OrderPageViewModel
            {
                DefaultMarket = MarketHelper.DefaultUIMarket,
                DefaultMarketplaceId = MarketHelper.DefaultUIMarketplaceId,
                SearchOrderId = orderId,
            };
            if (AccessManager.Company.ShortName == "PA")
            {
                model.DefaultDropShipperId = DSHelper.DefaultPAId;
            }
            if (AccessManager.Company.ShortName == "MBG")
            {
                model.DefaultDropShipperId = DSHelper.DefaultMBGId;
            }

            return View(model);
        }

        public virtual ActionResult GetSearchHistory()
        {
            return Json(OrderPageViewModel.GetLastOrderSearchList(Db,
                AccessManager.UserId), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetEmailsByOrderId(string orderId)
        {
            LogI("GetModelByOrderId, orderId=" + orderId);

            IList<EmailViewModel> emails = new List<EmailViewModel>();
            if (!String.IsNullOrEmpty(orderId))
                emails = ReturnOrderViewModel.GetEmailsByOrderId(Db, AddressService, orderId);

            return new JsonResult { Data = emails, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult OrderHistory(string orderId)
        {
            LogI("OrderHistory, orderId=" + orderId);

            var model = new OrderHistoryControlViewModel()
            {
                OrderNumber = orderId,
                IsCollapsed = false
            };
            
            return View("OrderHistory", model);
        }

        public virtual ActionResult GetOrderHistory(string orderId)
        {
            LogI("GetOrderHistory, orderId=" + orderId);

            var model = OrderHistoryViewModel.GetByOrderId(Db, LogService, WeightService, orderId);

            return JsonGet(new ValueResult<OrderHistoryViewModel>()
            {
                IsSuccess = model != null,
                Data = model
            });
        }

        public virtual ActionResult GetOrderQuickSummary(string orderId)
        {
            LogI("GetOrderQuickSummary, orderId=" + orderId);

            var model = OrderQuickSummaryViewModel.GetByOrderId(Db, LogService, WeightService, orderId);

            return JsonGet(new ValueResult<OrderQuickSummaryViewModel>()
            {
                IsSuccess = model != null,
                Data = model
            });
        }

        public virtual ActionResult GetOrderMessagesSummary(string orderNumber)
        {
            LogI("GetOrderMessagesSummary, orderNumber=" + orderNumber);

            var model = OrderMessagesSummaryViewModel.GetByOrderId(Db, LogService, orderNumber);

            return JsonGet(new ValueResult<OrderMessagesSummaryViewModel>()
            {
                IsSuccess = model != null,
                Data = model
            });
        }
    }
}
