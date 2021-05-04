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
    public partial class ReturnOrderController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ReturnOrderController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected ReturnOrderController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult ValidateRefund()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ValidateRefund);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult AcceptReturn()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AcceptReturn);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult MarkRefundAsProcessed()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.MarkRefundAsProcessed);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Generate()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Generate);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetEmailsByOrderId()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetEmailsByOrderId);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetModelByOrderId()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetModelByOrderId);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetShippingOptions()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetShippingOptions);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetStyleItemById()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetStyleItemById);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetStyleSizeInfo()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetStyleSizeInfo);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetExchangeInfo()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetExchangeInfo);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ReturnOrderController Actions { get { return MVC.ReturnOrder; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "ReturnOrder";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "ReturnOrder";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string ValidateRefund = "ValidateRefund";
            public readonly string AcceptReturn = "AcceptReturn";
            public readonly string MarkRefundAsProcessed = "MarkRefundAsProcessed";
            public readonly string Generate = "Generate";
            public readonly string GetEmailsByOrderId = "GetEmailsByOrderId";
            public readonly string GetModelByOrderId = "GetModelByOrderId";
            public readonly string GetShippingOptions = "GetShippingOptions";
            public readonly string GetStyleItemById = "GetStyleItemById";
            public readonly string GetStyleSizeInfo = "GetStyleSizeInfo";
            public readonly string GetExchangeInfo = "GetExchangeInfo";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string ValidateRefund = "ValidateRefund";
            public const string AcceptReturn = "AcceptReturn";
            public const string MarkRefundAsProcessed = "MarkRefundAsProcessed";
            public const string Generate = "Generate";
            public const string GetEmailsByOrderId = "GetEmailsByOrderId";
            public const string GetModelByOrderId = "GetModelByOrderId";
            public const string GetShippingOptions = "GetShippingOptions";
            public const string GetStyleItemById = "GetStyleItemById";
            public const string GetStyleSizeInfo = "GetStyleSizeInfo";
            public const string GetExchangeInfo = "GetExchangeInfo";
        }


        static readonly ActionParamsClass_Index s_params_Index = new ActionParamsClass_Index();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Index IndexParams { get { return s_params_Index; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Index
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_ValidateRefund s_params_ValidateRefund = new ActionParamsClass_ValidateRefund();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ValidateRefund ValidateRefundParams { get { return s_params_ValidateRefund; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ValidateRefund
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_AcceptReturn s_params_AcceptReturn = new ActionParamsClass_AcceptReturn();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AcceptReturn AcceptReturnParams { get { return s_params_AcceptReturn; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AcceptReturn
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_MarkRefundAsProcessed s_params_MarkRefundAsProcessed = new ActionParamsClass_MarkRefundAsProcessed();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_MarkRefundAsProcessed MarkRefundAsProcessedParams { get { return s_params_MarkRefundAsProcessed; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_MarkRefundAsProcessed
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_Generate s_params_Generate = new ActionParamsClass_Generate();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Generate GenerateParams { get { return s_params_Generate; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Generate
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_GetEmailsByOrderId s_params_GetEmailsByOrderId = new ActionParamsClass_GetEmailsByOrderId();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetEmailsByOrderId GetEmailsByOrderIdParams { get { return s_params_GetEmailsByOrderId; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetEmailsByOrderId
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetModelByOrderId s_params_GetModelByOrderId = new ActionParamsClass_GetModelByOrderId();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetModelByOrderId GetModelByOrderIdParams { get { return s_params_GetModelByOrderId; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetModelByOrderId
        {
            public readonly string searchString = "searchString";
        }
        static readonly ActionParamsClass_GetShippingOptions s_params_GetShippingOptions = new ActionParamsClass_GetShippingOptions();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetShippingOptions GetShippingOptionsParams { get { return s_params_GetShippingOptions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetShippingOptions
        {
            public readonly string countryTo = "countryTo";
            public readonly string countryFrom = "countryFrom";
            public readonly string weightLb = "weightLb";
            public readonly string weightOz = "weightOz";
        }
        static readonly ActionParamsClass_GetStyleItemById s_params_GetStyleItemById = new ActionParamsClass_GetStyleItemById();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetStyleItemById GetStyleItemByIdParams { get { return s_params_GetStyleItemById; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetStyleItemById
        {
            public readonly string styleItemId = "styleItemId";
        }
        static readonly ActionParamsClass_GetStyleSizeInfo s_params_GetStyleSizeInfo = new ActionParamsClass_GetStyleSizeInfo();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetStyleSizeInfo GetStyleSizeInfoParams { get { return s_params_GetStyleSizeInfo; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetStyleSizeInfo
        {
            public readonly string styleItemId = "styleItemId";
        }
        static readonly ActionParamsClass_GetExchangeInfo s_params_GetExchangeInfo = new ActionParamsClass_GetExchangeInfo();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetExchangeInfo GetExchangeInfoParams { get { return s_params_GetExchangeInfo; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetExchangeInfo
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
                public readonly string Index = "Index";
            }
            public readonly string Index = "~/Views/ReturnOrder/Index.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_ReturnOrderController : Amazon.Web.Controllers.ReturnOrderController
    {
        public T4MVC_ReturnOrderController() : base(Dummy.Instance) { }

        [NonAction]
        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult Index(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            IndexOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void ValidateRefundOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.ReturnOrderViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult ValidateRefund(Amazon.Web.ViewModels.ReturnOrderViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ValidateRefund);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            ValidateRefundOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void AcceptReturnOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long id);

        [NonAction]
        public override System.Web.Mvc.ActionResult AcceptReturn(long id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AcceptReturn);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            AcceptReturnOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void MarkRefundAsProcessedOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long id);

        [NonAction]
        public override System.Web.Mvc.ActionResult MarkRefundAsProcessed(long id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.MarkRefundAsProcessed);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            MarkRefundAsProcessedOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void GenerateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.ReturnOrderViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Generate(Amazon.Web.ViewModels.ReturnOrderViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Generate);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            GenerateOverride(callInfo, model);
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
        partial void GetModelByOrderIdOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string searchString);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetModelByOrderId(string searchString)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetModelByOrderId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "searchString", searchString);
            GetModelByOrderIdOverride(callInfo, searchString);
            return callInfo;
        }

        [NonAction]
        partial void GetShippingOptionsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string countryTo, string countryFrom, int? weightLb, decimal? weightOz);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetShippingOptions(string countryTo, string countryFrom, int? weightLb, decimal? weightOz)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetShippingOptions);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "countryTo", countryTo);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "countryFrom", countryFrom);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "weightLb", weightLb);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "weightOz", weightOz);
            GetShippingOptionsOverride(callInfo, countryTo, countryFrom, weightLb, weightOz);
            return callInfo;
        }

        [NonAction]
        partial void GetStyleItemByIdOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleItemId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetStyleItemById(long styleItemId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetStyleItemById);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleItemId", styleItemId);
            GetStyleItemByIdOverride(callInfo, styleItemId);
            return callInfo;
        }

        [NonAction]
        partial void GetStyleSizeInfoOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleItemId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetStyleSizeInfo(long styleItemId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetStyleSizeInfo);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleItemId", styleItemId);
            GetStyleSizeInfoOverride(callInfo, styleItemId);
            return callInfo;
        }

        [NonAction]
        partial void GetExchangeInfoOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderNumber);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetExchangeInfo(string orderNumber)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetExchangeInfo);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderNumber", orderNumber);
            GetExchangeInfoOverride(callInfo, orderNumber);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
