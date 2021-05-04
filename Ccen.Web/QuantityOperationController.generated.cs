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
    public partial class QuantityOperationController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public QuantityOperationController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected QuantityOperationController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult Index()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult AddOperation()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddOperation);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.JsonResult GetOrderItems()
        {
            return new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.GetOrderItems);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetAll()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.JsonResult Delete()
        {
            return new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.Delete);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Submit()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public QuantityOperationController Actions { get { return MVC.QuantityOperation; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "QuantityOperation";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "QuantityOperation";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string AddOperation = "AddOperation";
            public readonly string GetOrderItems = "GetOrderItems";
            public readonly string GetAll = "GetAll";
            public readonly string Delete = "Delete";
            public readonly string Submit = "Submit";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string AddOperation = "AddOperation";
            public const string GetOrderItems = "GetOrderItems";
            public const string GetAll = "GetAll";
            public const string Delete = "Delete";
            public const string Submit = "Submit";
        }


        static readonly ActionParamsClass_Index s_params_Index = new ActionParamsClass_Index();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Index IndexParams { get { return s_params_Index; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Index
        {
            public readonly string type = "type";
            public readonly string styleId = "styleId";
        }
        static readonly ActionParamsClass_AddOperation s_params_AddOperation = new ActionParamsClass_AddOperation();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AddOperation AddOperationParams { get { return s_params_AddOperation; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AddOperation
        {
            public readonly string type = "type";
            public readonly string styleId = "styleId";
        }
        static readonly ActionParamsClass_GetOrderItems s_params_GetOrderItems = new ActionParamsClass_GetOrderItems();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetOrderItems GetOrderItemsParams { get { return s_params_GetOrderItems; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetOrderItems
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetAll s_params_GetAll = new ActionParamsClass_GetAll();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAll GetAllParams { get { return s_params_GetAll; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAll
        {
            public readonly string request = "request";
            public readonly string orderNumber = "orderNumber";
            public readonly string styleString = "styleString";
            public readonly string styleItemId = "styleItemId";
            public readonly string userId = "userId";
            public readonly string typeId = "typeId";
            public readonly string dateFrom = "dateFrom";
            public readonly string dateTo = "dateTo";
        }
        static readonly ActionParamsClass_Delete s_params_Delete = new ActionParamsClass_Delete();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Delete DeleteParams { get { return s_params_Delete; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Delete
        {
            public readonly string operationId = "operationId";
        }
        static readonly ActionParamsClass_Submit s_params_Submit = new ActionParamsClass_Submit();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Submit SubmitParams { get { return s_params_Submit; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Submit
        {
            public readonly string model = "model";
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
                public readonly string Index = "Index";
                public readonly string QuantityOperationViewModel = "QuantityOperationViewModel";
            }
            public readonly string Index = "~/Views/QuantityOperation/Index.cshtml";
            public readonly string QuantityOperationViewModel = "~/Views/QuantityOperation/QuantityOperationViewModel.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_QuantityOperationController : Amazon.Web.Controllers.QuantityOperationController
    {
        public T4MVC_QuantityOperationController() : base(Dummy.Instance) { }

        [NonAction]
        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string type, string styleId);

        [NonAction]
        public override System.Web.Mvc.ActionResult Index(string type, string styleId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "type", type);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            IndexOverride(callInfo, type, styleId);
            return callInfo;
        }

        [NonAction]
        partial void AddOperationOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string type, string styleId);

        [NonAction]
        public override System.Web.Mvc.ActionResult AddOperation(string type, string styleId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddOperation);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "type", type);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            AddOperationOverride(callInfo, type, styleId);
            return callInfo;
        }

        [NonAction]
        partial void GetOrderItemsOverride(T4MVC_System_Web_Mvc_JsonResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.JsonResult GetOrderItems(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.GetOrderItems);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            GetOrderItemsOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void GetAllOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, string orderNumber, string styleString, long? styleItemId, long? userId, long? typeId, System.DateTime? dateFrom, System.DateTime? dateTo);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAll(Kendo.Mvc.UI.DataSourceRequest request, string orderNumber, string styleString, long? styleItemId, long? userId, long? typeId, System.DateTime? dateFrom, System.DateTime? dateTo)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderNumber", orderNumber);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleString", styleString);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleItemId", styleItemId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "userId", userId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "typeId", typeId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "dateFrom", dateFrom);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "dateTo", dateTo);
            GetAllOverride(callInfo, request, orderNumber, styleString, styleItemId, userId, typeId, dateFrom, dateTo);
            return callInfo;
        }

        [NonAction]
        partial void DeleteOverride(T4MVC_System_Web_Mvc_JsonResult callInfo, long operationId);

        [NonAction]
        public override System.Web.Mvc.JsonResult Delete(long operationId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.Delete);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "operationId", operationId);
            DeleteOverride(callInfo, operationId);
            return callInfo;
        }

        [NonAction]
        partial void SubmitOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.Inventory.QuantityOperationViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Submit(Amazon.Web.ViewModels.Inventory.QuantityOperationViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            SubmitOverride(callInfo, model);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
