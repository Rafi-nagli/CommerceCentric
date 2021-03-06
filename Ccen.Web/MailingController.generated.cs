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
    public partial class MailingController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public MailingController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected MailingController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult Generate()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Generate);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetQuickReturnLabelCost()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetQuickReturnLabelCost);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult QuickPrintReturnLabel()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.QuickPrintReturnLabel);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult CheckCanceledLabel()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CheckCanceledLabel);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult CheckReasonCode()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CheckReasonCode);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult CheckAddress()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CheckAddress);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetModelByOrderId()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetModelByOrderId);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetOrderById()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderById);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetShippingOptions()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetShippingOptions);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public MailingController Actions { get { return MVC.Mailing; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Mailing";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Mailing";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string Generate = "Generate";
            public readonly string GetQuickReturnLabelCost = "GetQuickReturnLabelCost";
            public readonly string QuickPrintReturnLabel = "QuickPrintReturnLabel";
            public readonly string CheckCanceledLabel = "CheckCanceledLabel";
            public readonly string CheckReasonCode = "CheckReasonCode";
            public readonly string CheckAddress = "CheckAddress";
            public readonly string GetModelByOrderId = "GetModelByOrderId";
            public readonly string GetOrderById = "GetOrderById";
            public readonly string GetShippingOptions = "GetShippingOptions";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string Generate = "Generate";
            public const string GetQuickReturnLabelCost = "GetQuickReturnLabelCost";
            public const string QuickPrintReturnLabel = "QuickPrintReturnLabel";
            public const string CheckCanceledLabel = "CheckCanceledLabel";
            public const string CheckReasonCode = "CheckReasonCode";
            public const string CheckAddress = "CheckAddress";
            public const string GetModelByOrderId = "GetModelByOrderId";
            public const string GetOrderById = "GetOrderById";
            public const string GetShippingOptions = "GetShippingOptions";
        }


        static readonly ActionParamsClass_Index s_params_Index = new ActionParamsClass_Index();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Index IndexParams { get { return s_params_Index; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Index
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_Generate s_params_Generate = new ActionParamsClass_Generate();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Generate GenerateParams { get { return s_params_Generate; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Generate
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_GetQuickReturnLabelCost s_params_GetQuickReturnLabelCost = new ActionParamsClass_GetQuickReturnLabelCost();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetQuickReturnLabelCost GetQuickReturnLabelCostParams { get { return s_params_GetQuickReturnLabelCost; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetQuickReturnLabelCost
        {
            public readonly string orderNumber = "orderNumber";
        }
        static readonly ActionParamsClass_QuickPrintReturnLabel s_params_QuickPrintReturnLabel = new ActionParamsClass_QuickPrintReturnLabel();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_QuickPrintReturnLabel QuickPrintReturnLabelParams { get { return s_params_QuickPrintReturnLabel; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_QuickPrintReturnLabel
        {
            public readonly string orderNumber = "orderNumber";
            public readonly string shippingMethodId = "shippingMethodId";
        }
        static readonly ActionParamsClass_CheckCanceledLabel s_params_CheckCanceledLabel = new ActionParamsClass_CheckCanceledLabel();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CheckCanceledLabel CheckCanceledLabelParams { get { return s_params_CheckCanceledLabel; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CheckCanceledLabel
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_CheckReasonCode s_params_CheckReasonCode = new ActionParamsClass_CheckReasonCode();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CheckReasonCode CheckReasonCodeParams { get { return s_params_CheckReasonCode; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CheckReasonCode
        {
            public readonly string orderId = "orderId";
            public readonly string reasonCode = "reasonCode";
        }
        static readonly ActionParamsClass_CheckAddress s_params_CheckAddress = new ActionParamsClass_CheckAddress();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_CheckAddress CheckAddressParams { get { return s_params_CheckAddress; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_CheckAddress
        {
            public readonly string model = "model";
            public readonly string onlyCheck = "onlyCheck";
        }
        static readonly ActionParamsClass_GetModelByOrderId s_params_GetModelByOrderId = new ActionParamsClass_GetModelByOrderId();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetModelByOrderId GetModelByOrderIdParams { get { return s_params_GetModelByOrderId; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetModelByOrderId
        {
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetOrderById s_params_GetOrderById = new ActionParamsClass_GetOrderById();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetOrderById GetOrderByIdParams { get { return s_params_GetOrderById; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetOrderById
        {
            public readonly string request = "request";
            public readonly string orderId = "orderId";
        }
        static readonly ActionParamsClass_GetShippingOptions s_params_GetShippingOptions = new ActionParamsClass_GetShippingOptions();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetShippingOptions GetShippingOptionsParams { get { return s_params_GetShippingOptions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetShippingOptions
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
            }
            public readonly string Index = "~/Views/Mailing/Index.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_MailingController : Amazon.Web.Controllers.MailingController
    {
        public T4MVC_MailingController() : base(Dummy.Instance) { }

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
        partial void GenerateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.MailViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Generate(Amazon.Web.ViewModels.MailViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Generate);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            GenerateOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void GetQuickReturnLabelCostOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderNumber);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetQuickReturnLabelCost(string orderNumber)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetQuickReturnLabelCost);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderNumber", orderNumber);
            GetQuickReturnLabelCostOverride(callInfo, orderNumber);
            return callInfo;
        }

        [NonAction]
        partial void QuickPrintReturnLabelOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderNumber, int shippingMethodId);

        [NonAction]
        public override System.Web.Mvc.ActionResult QuickPrintReturnLabel(string orderNumber, int shippingMethodId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.QuickPrintReturnLabel);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderNumber", orderNumber);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "shippingMethodId", shippingMethodId);
            QuickPrintReturnLabelOverride(callInfo, orderNumber, shippingMethodId);
            return callInfo;
        }

        [NonAction]
        partial void CheckCanceledLabelOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult CheckCanceledLabel(long orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CheckCanceledLabel);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            CheckCanceledLabelOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void CheckReasonCodeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long orderId, int reasonCode);

        [NonAction]
        public override System.Web.Mvc.ActionResult CheckReasonCode(long orderId, int reasonCode)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CheckReasonCode);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "reasonCode", reasonCode);
            CheckReasonCodeOverride(callInfo, orderId, reasonCode);
            return callInfo;
        }

        [NonAction]
        partial void CheckAddressOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.AddressViewModel model, bool onlyCheck);

        [NonAction]
        public override System.Web.Mvc.ActionResult CheckAddress(Amazon.Web.ViewModels.AddressViewModel model, bool onlyCheck)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.CheckAddress);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "onlyCheck", onlyCheck);
            CheckAddressOverride(callInfo, model, onlyCheck);
            return callInfo;
        }

        [NonAction]
        partial void GetModelByOrderIdOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetModelByOrderId(string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetModelByOrderId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            GetModelByOrderIdOverride(callInfo, orderId);
            return callInfo;
        }

        [NonAction]
        partial void GetOrderByIdOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, string orderId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetOrderById(Kendo.Mvc.UI.DataSourceRequest request, string orderId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOrderById);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "orderId", orderId);
            GetOrderByIdOverride(callInfo, request, orderId);
            return callInfo;
        }

        [NonAction]
        partial void GetShippingOptionsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.MailViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetShippingOptions(Amazon.Web.ViewModels.MailViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetShippingOptions);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            GetShippingOptionsOverride(callInfo, model);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
