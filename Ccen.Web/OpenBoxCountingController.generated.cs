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
    public partial class OpenBoxCountingController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public OpenBoxCountingController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected OpenBoxCountingController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult OnCreateItem()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.OnCreateItem);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult DeleteBox()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.DeleteBox);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult OnUpdateItem()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.OnUpdateItem);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Submit()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetOpenBox()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOpenBox);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public OpenBoxCountingController Actions { get { return MVC.OpenBoxCounting; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "OpenBoxCounting";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "OpenBoxCounting";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string OnCreateItem = "OnCreateItem";
            public readonly string DeleteBox = "DeleteBox";
            public readonly string OnUpdateItem = "OnUpdateItem";
            public readonly string Submit = "Submit";
            public readonly string GetOpenBox = "GetOpenBox";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string OnCreateItem = "OnCreateItem";
            public const string DeleteBox = "DeleteBox";
            public const string OnUpdateItem = "OnUpdateItem";
            public const string Submit = "Submit";
            public const string GetOpenBox = "GetOpenBox";
        }


        static readonly ActionParamsClass_OnCreateItem s_params_OnCreateItem = new ActionParamsClass_OnCreateItem();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_OnCreateItem OnCreateItemParams { get { return s_params_OnCreateItem; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_OnCreateItem
        {
            public readonly string styleId = "styleId";
            public readonly string isMobileMode = "isMobileMode";
        }
        static readonly ActionParamsClass_DeleteBox s_params_DeleteBox = new ActionParamsClass_DeleteBox();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_DeleteBox DeleteBoxParams { get { return s_params_DeleteBox; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_DeleteBox
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_OnUpdateItem s_params_OnUpdateItem = new ActionParamsClass_OnUpdateItem();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_OnUpdateItem OnUpdateItemParams { get { return s_params_OnUpdateItem; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_OnUpdateItem
        {
            public readonly string openBoxId = "openBoxId";
            public readonly string isMobileMode = "isMobileMode";
        }
        static readonly ActionParamsClass_Submit s_params_Submit = new ActionParamsClass_Submit();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Submit SubmitParams { get { return s_params_Submit; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Submit
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_GetOpenBox s_params_GetOpenBox = new ActionParamsClass_GetOpenBox();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetOpenBox GetOpenBoxParams { get { return s_params_GetOpenBox; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetOpenBox
        {
            public readonly string request = "request";
            public readonly string styleId = "styleId";
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
                public readonly string OpenBoxViewModel = "OpenBoxViewModel";
            }
            public readonly string OpenBoxViewModel = "~/Views/OpenBoxCounting/OpenBoxViewModel.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_OpenBoxCountingController : Amazon.Web.Controllers.OpenBoxCountingController
    {
        public T4MVC_OpenBoxCountingController() : base(Dummy.Instance) { }

        [NonAction]
        partial void OnCreateItemOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleId, bool? isMobileMode);

        [NonAction]
        public override System.Web.Mvc.ActionResult OnCreateItem(long styleId, bool? isMobileMode)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.OnCreateItem);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "isMobileMode", isMobileMode);
            OnCreateItemOverride(callInfo, styleId, isMobileMode);
            return callInfo;
        }

        [NonAction]
        partial void DeleteBoxOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id);

        [NonAction]
        public override System.Web.Mvc.ActionResult DeleteBox(int id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.DeleteBox);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            DeleteBoxOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void OnUpdateItemOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long openBoxId, bool? isMobileMode);

        [NonAction]
        public override System.Web.Mvc.ActionResult OnUpdateItem(long openBoxId, bool? isMobileMode)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.OnUpdateItem);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "openBoxId", openBoxId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "isMobileMode", isMobileMode);
            OnUpdateItemOverride(callInfo, openBoxId, isMobileMode);
            return callInfo;
        }

        [NonAction]
        partial void SubmitOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.Inventory.Counting.OpenBoxCountingViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Submit(Amazon.Web.ViewModels.Inventory.Counting.OpenBoxCountingViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            SubmitOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void GetOpenBoxOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, long styleId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetOpenBox(Kendo.Mvc.UI.DataSourceRequest request, long styleId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetOpenBox);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            GetOpenBoxOverride(callInfo, request, styleId);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
