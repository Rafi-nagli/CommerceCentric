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
    public partial class GrouponController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public GrouponController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected GrouponController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult GetFeed()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetFeed);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Submit()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UploadFeed()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UploadFeed);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public GrouponController Actions { get { return MVC.Groupon; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Groupon";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Groupon";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string GetFeed = "GetFeed";
            public readonly string Submit = "Submit";
            public readonly string UploadFeed = "UploadFeed";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string GetFeed = "GetFeed";
            public const string Submit = "Submit";
            public const string UploadFeed = "UploadFeed";
        }


        static readonly ActionParamsClass_GetFeed s_params_GetFeed = new ActionParamsClass_GetFeed();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetFeed GetFeedParams { get { return s_params_GetFeed; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetFeed
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_Submit s_params_Submit = new ActionParamsClass_Submit();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Submit SubmitParams { get { return s_params_Submit; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Submit
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_UploadFeed s_params_UploadFeed = new ActionParamsClass_UploadFeed();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UploadFeed UploadFeedParams { get { return s_params_UploadFeed; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UploadFeed
        {
            public readonly string request = "request";
            public readonly string files = "files";
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
    public partial class T4MVC_GrouponController : Amazon.Web.Controllers.GrouponController
    {
        public T4MVC_GrouponController() : base(Dummy.Instance) { }

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
        partial void GetFeedOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetFeed(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetFeed);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            GetFeedOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void SubmitOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.Groupon.ActualizeGrouponQtyFeedViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Submit(Amazon.Web.ViewModels.Groupon.ActualizeGrouponQtyFeedViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            SubmitOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void UploadFeedOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, System.Collections.Generic.IEnumerable<System.Web.HttpPostedFileBase> files);

        [NonAction]
        public override System.Web.Mvc.ActionResult UploadFeed(Kendo.Mvc.UI.DataSourceRequest request, System.Collections.Generic.IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UploadFeed);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "files", files);
            UploadFeedOverride(callInfo, request, files);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
