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
    public partial class InventoryHistoryController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryHistoryController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected InventoryHistoryController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult ViewInventoryHistory()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ViewInventoryHistory);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetAll()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryHistoryController Actions { get { return MVC.InventoryHistory; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "InventoryHistory";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "InventoryHistory";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string ViewInventoryHistory = "ViewInventoryHistory";
            public readonly string GetAll = "GetAll";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string ViewInventoryHistory = "ViewInventoryHistory";
            public const string GetAll = "GetAll";
        }


        static readonly ActionParamsClass_ViewInventoryHistory s_params_ViewInventoryHistory = new ActionParamsClass_ViewInventoryHistory();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ViewInventoryHistory ViewInventoryHistoryParams { get { return s_params_ViewInventoryHistory; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ViewInventoryHistory
        {
            public readonly string styleItemId = "styleItemId";
        }
        static readonly ActionParamsClass_GetAll s_params_GetAll = new ActionParamsClass_GetAll();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAll GetAllParams { get { return s_params_GetAll; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAll
        {
            public readonly string styleItemId = "styleItemId";
            public readonly string includeSnapshoot = "includeSnapshoot";
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
                public readonly string InventoryHistoryPopupContent = "InventoryHistoryPopupContent";
            }
            public readonly string InventoryHistoryPopupContent = "~/Views/InventoryHistory/InventoryHistoryPopupContent.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_InventoryHistoryController : Amazon.Web.Controllers.InventoryHistoryController
    {
        public T4MVC_InventoryHistoryController() : base(Dummy.Instance) { }

        [NonAction]
        partial void ViewInventoryHistoryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleItemId);

        [NonAction]
        public override System.Web.Mvc.ActionResult ViewInventoryHistory(long styleItemId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ViewInventoryHistory);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleItemId", styleItemId);
            ViewInventoryHistoryOverride(callInfo, styleItemId);
            return callInfo;
        }

        [NonAction]
        partial void GetAllOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleItemId, bool includeSnapshoot);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAll(long styleItemId, bool includeSnapshoot)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleItemId", styleItemId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "includeSnapshoot", includeSnapshoot);
            GetAllOverride(callInfo, styleItemId, includeSnapshoot);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114