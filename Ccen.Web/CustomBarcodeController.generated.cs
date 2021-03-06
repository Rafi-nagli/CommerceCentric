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
    public partial class CustomBarcodeController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public CustomBarcodeController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected CustomBarcodeController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult GetAll()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult RemoveAttachedSKU()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RemoveAttachedSKU);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult RequestBarcode()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RequestBarcode);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public CustomBarcodeController Actions { get { return MVC.CustomBarcode; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "CustomBarcode";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "CustomBarcode";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string GetAll = "GetAll";
            public readonly string RemoveAttachedSKU = "RemoveAttachedSKU";
            public readonly string OnRequestBarcode = "OnRequestBarcode";
            public readonly string RequestBarcode = "RequestBarcode";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string GetAll = "GetAll";
            public const string RemoveAttachedSKU = "RemoveAttachedSKU";
            public const string OnRequestBarcode = "OnRequestBarcode";
            public const string RequestBarcode = "RequestBarcode";
        }


        static readonly ActionParamsClass_GetAll s_params_GetAll = new ActionParamsClass_GetAll();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAll GetAllParams { get { return s_params_GetAll; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAll
        {
            public readonly string request = "request";
            public readonly string barcode = "barcode";
            public readonly string sku = "sku";
        }
        static readonly ActionParamsClass_RemoveAttachedSKU s_params_RemoveAttachedSKU = new ActionParamsClass_RemoveAttachedSKU();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RemoveAttachedSKU RemoveAttachedSKUParams { get { return s_params_RemoveAttachedSKU; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RemoveAttachedSKU
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_RequestBarcode s_params_RequestBarcode = new ActionParamsClass_RequestBarcode();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RequestBarcode RequestBarcodeParams { get { return s_params_RequestBarcode; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RequestBarcode
        {
            public readonly string skuText = "skuText";
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
                public readonly string RequestBarcodePopupContent = "RequestBarcodePopupContent";
            }
            public readonly string Index = "~/Views/CustomBarcode/Index.cshtml";
            public readonly string RequestBarcodePopupContent = "~/Views/CustomBarcode/RequestBarcodePopupContent.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_CustomBarcodeController : Amazon.Web.Controllers.CustomBarcodeController
    {
        public T4MVC_CustomBarcodeController() : base(Dummy.Instance) { }

        [NonAction]
        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult Index()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            IndexOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void GetAllOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, string barcode, string sku);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAll(Kendo.Mvc.UI.DataSourceRequest request, string barcode, string sku)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "barcode", barcode);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "sku", sku);
            GetAllOverride(callInfo, request, barcode, sku);
            return callInfo;
        }

        [NonAction]
        partial void RemoveAttachedSKUOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long id);

        [NonAction]
        public override System.Web.Mvc.ActionResult RemoveAttachedSKU(long id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RemoveAttachedSKU);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            RemoveAttachedSKUOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void OnRequestBarcodeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult OnRequestBarcode()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.OnRequestBarcode);
            OnRequestBarcodeOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void RequestBarcodeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string skuText);

        [NonAction]
        public override System.Web.Mvc.ActionResult RequestBarcode(string skuText)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RequestBarcode);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "skuText", skuText);
            RequestBarcodeOverride(callInfo, skuText);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
