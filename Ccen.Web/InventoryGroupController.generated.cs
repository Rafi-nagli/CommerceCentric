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
    public partial class InventoryGroupController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryGroupController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected InventoryGroupController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult AddGroup()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddGroup);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult DeleteGroup()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.DeleteGroup);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult EditGroup()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.EditGroup);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetAll()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetChildren()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetChildren);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult DeleteChild()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.DeleteChild);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UpdatePrice()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdatePrice);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Submit()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryGroupController Actions { get { return MVC.InventoryGroup; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "InventoryGroup";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "InventoryGroup";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string AddGroup = "AddGroup";
            public readonly string DeleteGroup = "DeleteGroup";
            public readonly string EditGroup = "EditGroup";
            public readonly string GetAll = "GetAll";
            public readonly string GetChildren = "GetChildren";
            public readonly string DeleteChild = "DeleteChild";
            public readonly string UpdatePrice = "UpdatePrice";
            public readonly string AddFeed = "AddFeed";
            public readonly string Submit = "Submit";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string AddGroup = "AddGroup";
            public const string DeleteGroup = "DeleteGroup";
            public const string EditGroup = "EditGroup";
            public const string GetAll = "GetAll";
            public const string GetChildren = "GetChildren";
            public const string DeleteChild = "DeleteChild";
            public const string UpdatePrice = "UpdatePrice";
            public const string AddFeed = "AddFeed";
            public const string Submit = "Submit";
        }


        static readonly ActionParamsClass_AddGroup s_params_AddGroup = new ActionParamsClass_AddGroup();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_AddGroup AddGroupParams { get { return s_params_AddGroup; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_AddGroup
        {
            public readonly string styleIds = "styleIds";
        }
        static readonly ActionParamsClass_DeleteGroup s_params_DeleteGroup = new ActionParamsClass_DeleteGroup();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_DeleteGroup DeleteGroupParams { get { return s_params_DeleteGroup; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_DeleteGroup
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_EditGroup s_params_EditGroup = new ActionParamsClass_EditGroup();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_EditGroup EditGroupParams { get { return s_params_EditGroup; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_EditGroup
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_GetAll s_params_GetAll = new ActionParamsClass_GetAll();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetAll GetAllParams { get { return s_params_GetAll; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetAll
        {
            public readonly string request = "request";
            public readonly string dateFrom = "dateFrom";
            public readonly string dateTo = "dateTo";
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
        static readonly ActionParamsClass_DeleteChild s_params_DeleteChild = new ActionParamsClass_DeleteChild();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_DeleteChild DeleteChildParams { get { return s_params_DeleteChild; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_DeleteChild
        {
            public readonly string request = "request";
            public readonly string item = "item";
        }
        static readonly ActionParamsClass_UpdatePrice s_params_UpdatePrice = new ActionParamsClass_UpdatePrice();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UpdatePrice UpdatePriceParams { get { return s_params_UpdatePrice; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UpdatePrice
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_Submit s_params_Submit = new ActionParamsClass_Submit();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Submit SubmitParams { get { return s_params_Submit; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Submit
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
                public readonly string _ManagePrice = "_ManagePrice";
                public readonly string Index = "Index";
                public readonly string InventoryGroupViewModel = "InventoryGroupViewModel";
            }
            public readonly string _ManagePrice = "~/Views/InventoryGroup/_ManagePrice.cshtml";
            public readonly string Index = "~/Views/InventoryGroup/Index.cshtml";
            public readonly string InventoryGroupViewModel = "~/Views/InventoryGroup/InventoryGroupViewModel.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_InventoryGroupController : Amazon.Web.Controllers.InventoryGroupController
    {
        public T4MVC_InventoryGroupController() : base(Dummy.Instance) { }

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
        partial void AddGroupOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, System.Collections.Generic.IList<long> styleIds);

        [NonAction]
        public override System.Web.Mvc.ActionResult AddGroup(System.Collections.Generic.IList<long> styleIds)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddGroup);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleIds", styleIds);
            AddGroupOverride(callInfo, styleIds);
            return callInfo;
        }

        [NonAction]
        partial void DeleteGroupOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int id);

        [NonAction]
        public override System.Web.Mvc.ActionResult DeleteGroup(int id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.DeleteGroup);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            DeleteGroupOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void EditGroupOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long id);

        [NonAction]
        public override System.Web.Mvc.ActionResult EditGroup(long id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.EditGroup);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            EditGroupOverride(callInfo, id);
            return callInfo;
        }

        [NonAction]
        partial void GetAllOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, System.DateTime? dateFrom, System.DateTime? dateTo);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetAll(Kendo.Mvc.UI.DataSourceRequest request, System.DateTime? dateFrom, System.DateTime? dateTo)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetAll);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "dateFrom", dateFrom);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "dateTo", dateTo);
            GetAllOverride(callInfo, request, dateFrom, dateTo);
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
        partial void DeleteChildOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.InventoryGroup.InventoryGroupItemViewModel item);

        [NonAction]
        public override System.Web.Mvc.ActionResult DeleteChild(Kendo.Mvc.UI.DataSourceRequest request, Amazon.Web.ViewModels.InventoryGroup.InventoryGroupItemViewModel item)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.DeleteChild);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "request", request);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "item", item);
            DeleteChildOverride(callInfo, request, item);
            return callInfo;
        }

        [NonAction]
        partial void UpdatePriceOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.InventoryGroup.InventoryGroupPriceViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult UpdatePrice(Amazon.Web.ViewModels.InventoryGroup.InventoryGroupPriceViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UpdatePrice);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            UpdatePriceOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void AddFeedOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult AddFeed()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.AddFeed);
            AddFeedOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void SubmitOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.InventoryGroup.InventoryGroupViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Submit(Amazon.Web.ViewModels.InventoryGroup.InventoryGroupViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            SubmitOverride(callInfo, model);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
