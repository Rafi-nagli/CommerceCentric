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
    public partial class LicenseController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public LicenseController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected LicenseController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult GetAllParents()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAllParents);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetChildren()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetChildren);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult AddParent()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddParent);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateParent()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateParent);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult AddChild()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddChild);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateChild()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateChild);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public LicenseController Actions { get { return MVC.License; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "License";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "License";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string GetAllParents = "GetAllParents";
            public readonly string GetChildren = "GetChildren";
            public readonly string AddParent = "AddParent";
            public readonly string UpdateParent = "UpdateParent";
            public readonly string AddChild = "AddChild";
            public readonly string UpdateChild = "UpdateChild";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string GetAllParents = "GetAllParents";
            public const string GetChildren = "GetChildren";
            public const string AddParent = "AddParent";
            public const string UpdateParent = "UpdateParent";
            public const string AddChild = "AddChild";
            public const string UpdateChild = "UpdateChild";
        }


        static readonly ActionParamsClass_GetAllParents s_params_GetAllParents = new ActionParamsClass_GetAllParents();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAllParents GetAllParentsParams { get { return s_params_GetAllParents; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAllParents
        {
            public readonly string request = "request";
        }
        static readonly ActionParamsClass_GetChildren s_params_GetChildren = new ActionParamsClass_GetChildren();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetChildren GetChildrenParams { get { return s_params_GetChildren; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetChildren
        {
            public readonly string request = "request";
            public readonly string Id = "Id";
        }
        static readonly ActionParamsClass_AddParent s_params_AddParent = new ActionParamsClass_AddParent();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AddParent AddParentParams { get { return s_params_AddParent; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AddParent
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_UpdateParent s_params_UpdateParent = new ActionParamsClass_UpdateParent();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateParent UpdateParentParams { get { return s_params_UpdateParent; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateParent
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_AddChild s_params_AddChild = new ActionParamsClass_AddChild();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AddChild AddChildParams { get { return s_params_AddChild; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AddChild
        {
            public readonly string request = "request";
            public readonly string item = "item";
            public readonly string parentId = "parentId";
        }
        static readonly ActionParamsClass_UpdateChild s_params_UpdateChild = new ActionParamsClass_UpdateChild();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateChild UpdateChildParams { get { return s_params_UpdateChild; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateChild
        {
            public readonly string request = "request";
            public readonly string item = "item";
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
            public readonly string Index = "~/Views/License/Index.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_LicenseController : Amazon.Web.Controllers.LicenseController
    {
        public T4MVC_LicenseController() : base(Dummy.Instance) { }

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
        partial void GetAllParentsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAllParents(Kendo.Mvc.UI.DataSourceRequest request)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAllParents);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            GetAllParentsOverride(callInfo, request);
            return callInfo;
        }

        [NonAction]
        partial void GetChildrenOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, int Id);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetChildren(Kendo.Mvc.UI.DataSourceRequest request, int Id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetChildren);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "Id", Id);
            GetChildrenOverride(callInfo, request, Id);
            return callInfo;
        }

        [NonAction]
        partial void AddParentOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult AddParent(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddParent);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            AddParentOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void UpdateParentOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateParent(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateParent);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            UpdateParentOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void AddChildOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item, int parentId);

        [NonAction]
        public override System.Web.Mvc.ActionResult AddChild(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item, int parentId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddChild);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "parentId", parentId);
            AddChildOverride(callInfo, request, item, parentId);
            return callInfo;
        }

        [NonAction]
        partial void UpdateChildOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateChild(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.LicenseViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateChild);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            UpdateChildOverride(callInfo, request, item);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
