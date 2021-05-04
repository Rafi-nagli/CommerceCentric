// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments and CLS compliance
// 0108: suppress "Foo hides inherited member Foo. Use the new keyword if hiding was intended." when a controller and its abstract parent are both processed
// 0114: suppress "Foo.BarController.Baz()' hides inherited member 'Qux.BarController.Baz()'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword." when an action (with an argument) overrides an action in a parent controller
#pragma warning disable 1591, 3008, 3009, 0108, 0114
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;
namespace Amazon.Web.Controllers
{
    public partial class OrderInfoController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public OrderInfoController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected OrderInfoController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(Task<ActionResult> taskResult)
        {
            return RedirectToAction(taskResult.Result);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(Task<ActionResult> taskResult)
        {
            return RedirectToActionPermanent(taskResult.Result);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Orders()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Orders);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetEmailsByOrderId()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetEmailsByOrderId);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult OrderHistory()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.OrderHistory);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetOrderHistory()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderHistory);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetOrderQuickSummary()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderQuickSummary);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetOrderMessagesSummary()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderMessagesSummary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public OrderInfoController Actions { get { return MVC.OrderInfo; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "OrderInfo";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "OrderInfo";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Stats = "Stats";
            public readonly string Orders = "Orders";
            public readonly string GetSearchHistory = "GetSearchHistory";
            public readonly string GetEmailsByOrderId = "GetEmailsByOrderId";
            public readonly string OrderHistory = "OrderHistory";
            public readonly string GetOrderHistory = "GetOrderHistory";
            public readonly string GetOrderQuickSummary = "GetOrderQuickSummary";
            public readonly string GetOrderMessagesSummary = "GetOrderMessagesSummary";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Stats = "Stats";
            public const string Orders = "Orders";
            public const string GetSearchHistory = "GetSearchHistory";
            public const string GetEmailsByOrderId = "GetEmailsByOrderId";
            public const string OrderHistory = "OrderHistory";
            public const string GetOrderHistory = "GetOrderHistory";
            public const string GetOrderQuickSummary = "GetOrderQuickSummary";
            public const string GetOrderMessagesSummary = "GetOrderMessagesSummary";
        }


        static readonly ActionParamsClass_Orders s_params_Orders = new ActionParamsClass_Orders();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Orders OrdersParams { get { return s_params_Orders; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Orders
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetEmailsByOrderId s_params_GetEmailsByOrderId = new ActionParamsClass_GetEmailsByOrderId();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetEmailsByOrderId GetEmailsByOrderIdParams { get { return s_params_GetEmailsByOrderId; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetEmailsByOrderId
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_OrderHistory s_params_OrderHistory = new ActionParamsClass_OrderHistory();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_OrderHistory OrderHistoryParams { get { return s_params_OrderHistory; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_OrderHistory
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetOrderHistory s_params_GetOrderHistory = new ActionParamsClass_GetOrderHistory();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetOrderHistory GetOrderHistoryParams { get { return s_params_GetOrderHistory; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetOrderHistory
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetOrderQuickSummary s_params_GetOrderQuickSummary = new ActionParamsClass_GetOrderQuickSummary();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetOrderQuickSummary GetOrderQuickSummaryParams { get { return s_params_GetOrderQuickSummary; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetOrderQuickSummary
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetOrderMessagesSummary s_params_GetOrderMessagesSummary = new ActionParamsClass_GetOrderMessagesSummary();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetOrderMessagesSummary GetOrderMessagesSummaryParams { get { return s_params_GetOrderMessagesSummary; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetOrderMessagesSummary
        {
            public readonly string orderNumber = "orderNumber";
        }
        static readonly ViewsClass s_views = new ViewsClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewsClass Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewsClass
        {
            static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
            public _ViewNamesClass ViewNames { get { return s_ViewNames; } }
            public class _ViewNamesClass
            {
            }
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_OrderInfoController : Amazon.Web.Controllers.OrderInfoController
    {
        public T4MVC_OrderInfoController() : base(Dummy.Instance) { }

        [NonAction]
        partial void StatsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult Stats()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Stats);
            StatsOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void OrdersOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult Orders(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Orders);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            OrdersOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void GetSearchHistoryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSearchHistory()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSearchHistory);
            GetSearchHistoryOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void GetEmailsByOrderIdOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetEmailsByOrderId(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetEmailsByOrderId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            GetEmailsByOrderIdOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void OrderHistoryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult OrderHistory(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.OrderHistory);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            OrderHistoryOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void GetOrderHistoryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetOrderHistory(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderHistory);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            GetOrderHistoryOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void GetOrderQuickSummaryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetOrderQuickSummary(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderQuickSummary);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            GetOrderQuickSummaryOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void GetOrderMessagesSummaryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderNumber);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetOrderMessagesSummary(string orderNumber)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderMessagesSummary);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderNumber", orderNumber);
            GetOrderMessagesSummaryOverride(callInfo, orderNumber);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114