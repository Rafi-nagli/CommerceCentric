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
    public partial class FeedController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public FeedController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected FeedController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult GetAllMessage()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAllMessage);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetFeed()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetFeed);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public FeedController Actions { get { return MVC.Feed; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Feed";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Feed";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string GetAll = "GetAll";
            public readonly string GetAllMessage = "GetAllMessage";
            public readonly string GetFeed = "GetFeed";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string GetAll = "GetAll";
            public const string GetAllMessage = "GetAllMessage";
            public const string GetFeed = "GetFeed";
        }


        static readonly ActionParamsClass_GetAll s_params_GetAll = new ActionParamsClass_GetAll();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAll GetAllParams { get { return s_params_GetAll; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAll
        {
            public readonly string request = "request";
            public readonly string market = "market";
            public readonly string marketplaceId = "marketplaceId";
            public readonly string dateFrom = "dateFrom";
            public readonly string dateTo = "dateTo";
            public readonly string type = "type";
        }
        static readonly ActionParamsClass_GetAllMessage s_params_GetAllMessage = new ActionParamsClass_GetAllMessage();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAllMessage GetAllMessageParams { get { return s_params_GetAllMessage; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAllMessage
        {
            public readonly string request = "request";
            public readonly string feedId = "feedId";
        }
        static readonly ActionParamsClass_GetFeed s_params_GetFeed = new ActionParamsClass_GetFeed();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetFeed GetFeedParams { get { return s_params_GetFeed; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetFeed
        {
            public readonly string feedId = "feedId";
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
            public readonly string Index = "~/Views/Feed/Index.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_FeedController : Amazon.Web.Controllers.FeedController
    {
        public T4MVC_FeedController() : base(Dummy.Instance) { }

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
        partial void GetAllOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Core.Models.MarketType market, string marketplaceId, System.DateTime? dateFrom, System.DateTime? dateTo, int? type);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAll(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Core.Models.MarketType market, string marketplaceId, System.DateTime? dateFrom, System.DateTime? dateTo, int? type)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "market", market);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "marketplaceId", marketplaceId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "dateFrom", dateFrom);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "dateTo", dateTo);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "type", type);
            GetAllOverride(callInfo, request, market, marketplaceId, dateFrom, dateTo, type);
            return callInfo;
        }

        [NonAction]
        partial void GetAllMessageOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, long feedId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAllMessage(Kendo.Mvc.UI.DataSourceRequest request, long feedId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAllMessage);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "feedId", feedId);
            GetAllMessageOverride(callInfo, request, feedId);
            return callInfo;
        }

        [NonAction]
        partial void GetFeedOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long feedId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetFeed(long feedId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetFeed);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "feedId", feedId);
            GetFeedOverride(callInfo, feedId);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
