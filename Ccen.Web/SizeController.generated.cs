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
    public partial class SizeController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public SizeController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected SizeController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult GetAllGroups()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAllGroups);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult AddSizeGroup()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddSizeGroup);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateSizeGroup()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateSizeGroup);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult RemoveSizeGroup()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RemoveSizeGroup);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetSizes()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSizes);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult AddSize()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddSize);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdateSize()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateSize);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult RemoveSize()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RemoveSize);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public SizeController Actions { get { return MVC.Size; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Size";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Size";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string GetAllGroups = "GetAllGroups";
            public readonly string AddSizeGroup = "AddSizeGroup";
            public readonly string UpdateSizeGroup = "UpdateSizeGroup";
            public readonly string RemoveSizeGroup = "RemoveSizeGroup";
            public readonly string GetSizes = "GetSizes";
            public readonly string AddSize = "AddSize";
            public readonly string UpdateSize = "UpdateSize";
            public readonly string RemoveSize = "RemoveSize";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string GetAllGroups = "GetAllGroups";
            public const string AddSizeGroup = "AddSizeGroup";
            public const string UpdateSizeGroup = "UpdateSizeGroup";
            public const string RemoveSizeGroup = "RemoveSizeGroup";
            public const string GetSizes = "GetSizes";
            public const string AddSize = "AddSize";
            public const string UpdateSize = "UpdateSize";
            public const string RemoveSize = "RemoveSize";
        }


        static readonly ActionParamsClass_GetAllGroups s_params_GetAllGroups = new ActionParamsClass_GetAllGroups();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAllGroups GetAllGroupsParams { get { return s_params_GetAllGroups; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAllGroups
        {
            public readonly string request = "request";
        }
        static readonly ActionParamsClass_AddSizeGroup s_params_AddSizeGroup = new ActionParamsClass_AddSizeGroup();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AddSizeGroup AddSizeGroupParams { get { return s_params_AddSizeGroup; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AddSizeGroup
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_UpdateSizeGroup s_params_UpdateSizeGroup = new ActionParamsClass_UpdateSizeGroup();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateSizeGroup UpdateSizeGroupParams { get { return s_params_UpdateSizeGroup; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateSizeGroup
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_RemoveSizeGroup s_params_RemoveSizeGroup = new ActionParamsClass_RemoveSizeGroup();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RemoveSizeGroup RemoveSizeGroupParams { get { return s_params_RemoveSizeGroup; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RemoveSizeGroup
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_GetSizes s_params_GetSizes = new ActionParamsClass_GetSizes();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetSizes GetSizesParams { get { return s_params_GetSizes; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetSizes
        {
            public readonly string request = "request";
            public readonly string groupId = "groupId";
        }
        static readonly ActionParamsClass_AddSize s_params_AddSize = new ActionParamsClass_AddSize();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AddSize AddSizeParams { get { return s_params_AddSize; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AddSize
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_UpdateSize s_params_UpdateSize = new ActionParamsClass_UpdateSize();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdateSize UpdateSizeParams { get { return s_params_UpdateSize; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdateSize
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_RemoveSize s_params_RemoveSize = new ActionParamsClass_RemoveSize();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_RemoveSize RemoveSizeParams { get { return s_params_RemoveSize; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_RemoveSize
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
            public readonly string Index = "~/Views/Size/Index.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_SizeController : Amazon.Web.Controllers.SizeController
    {
        public T4MVC_SizeController() : base(Dummy.Instance) { }

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
        partial void GetAllGroupsOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAllGroups(Kendo.Mvc.UI.DataSourceRequest request)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAllGroups);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            GetAllGroupsOverride(callInfo, request);
            return callInfo;
        }

        [NonAction]
        partial void AddSizeGroupOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeGroupViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult AddSizeGroup(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeGroupViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddSizeGroup);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            AddSizeGroupOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void UpdateSizeGroupOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeGroupViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateSizeGroup(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeGroupViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateSizeGroup);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            UpdateSizeGroupOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void RemoveSizeGroupOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeGroupViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult RemoveSizeGroup(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeGroupViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RemoveSizeGroup);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            RemoveSizeGroupOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void GetSizesOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, int groupId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSizes(Kendo.Mvc.UI.DataSourceRequest request, int groupId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSizes);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "groupId", groupId);
            GetSizesOverride(callInfo, request, groupId);
            return callInfo;
        }

        [NonAction]
        partial void AddSizeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult AddSize(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddSize);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            AddSizeOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void UpdateSizeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdateSize(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdateSize);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            UpdateSizeOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void RemoveSizeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult RemoveSize(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.SizeViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.RemoveSize);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            RemoveSizeOverride(callInfo, request, item);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
