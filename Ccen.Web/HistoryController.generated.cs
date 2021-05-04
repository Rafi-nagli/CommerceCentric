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
    public partial class HistoryController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HistoryController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected HistoryController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult GetSyncHistory()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSyncHistory);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetSyncMessages()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSyncMessages);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public HistoryController Actions { get { return MVC.History; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "History";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "History";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string SyncHistory = "SyncHistory";
            public readonly string GetSyncHistory = "GetSyncHistory";
            public readonly string GetSyncMessages = "GetSyncMessages";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string SyncHistory = "SyncHistory";
            public const string GetSyncHistory = "GetSyncHistory";
            public const string GetSyncMessages = "GetSyncMessages";
        }


        static readonly ActionParamsClass_GetSyncHistory s_params_GetSyncHistory = new ActionParamsClass_GetSyncHistory();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetSyncHistory GetSyncHistoryParams { get { return s_params_GetSyncHistory; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetSyncHistory
        {
            public readonly string request = "request";
        }
        static readonly ActionParamsClass_GetSyncMessages s_params_GetSyncMessages = new ActionParamsClass_GetSyncMessages();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetSyncMessages GetSyncMessagesParams { get { return s_params_GetSyncMessages; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetSyncMessages
        {
            public readonly string request = "request";
            public readonly string syncHistoryId = "syncHistoryId";
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
                public readonly string SyncHistory = "SyncHistory";
            }
            public readonly string SyncHistory = "~/Views/History/SyncHistory.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_HistoryController : Amazon.Web.Controllers.HistoryController
    {
        public T4MVC_HistoryController() : base(Dummy.Instance) { }

        [NonAction]
        partial void SyncHistoryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult SyncHistory()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.SyncHistory);
            SyncHistoryOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void GetSyncHistoryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSyncHistory(Kendo.Mvc.UI.DataSourceRequest request)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSyncHistory);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            GetSyncHistoryOverride(callInfo, request);
            return callInfo;
        }

        [NonAction]
        partial void GetSyncMessagesOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, long syncHistoryId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSyncMessages(Kendo.Mvc.UI.DataSourceRequest request, long syncHistoryId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSyncMessages);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "syncHistoryId", syncHistoryId);
            GetSyncMessagesOverride(callInfo, request, syncHistoryId);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
