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
    public partial class DashboardController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public DashboardController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected DashboardController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult GetSalesByPeriod()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSalesByPeriod);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetSalesByMarketplace()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSalesByMarketplace);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetSalesByProductType()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSalesByProductType);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetShippingByCarrier()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetShippingByCarrier);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetInventoryByFeature()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetInventoryByFeature);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetListingErrorsByPeriod()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetListingErrorsByPeriod);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public DashboardController Actions { get { return MVC.Dashboard; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "Dashboard";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "Dashboard";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string GetSystemStatus = "GetSystemStatus";
            public readonly string GetSystemMessages = "GetSystemMessages";
            public readonly string GetSalesByPeriod = "GetSalesByPeriod";
            public readonly string GetSalesByMarketplace = "GetSalesByMarketplace";
            public readonly string GetSalesByProductType = "GetSalesByProductType";
            public readonly string GetShippingByCarrier = "GetShippingByCarrier";
            public readonly string GetInventoryByFeature = "GetInventoryByFeature";
            public readonly string GetListingErrorsByPeriod = "GetListingErrorsByPeriod";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string GetSystemStatus = "GetSystemStatus";
            public const string GetSystemMessages = "GetSystemMessages";
            public const string GetSalesByPeriod = "GetSalesByPeriod";
            public const string GetSalesByMarketplace = "GetSalesByMarketplace";
            public const string GetSalesByProductType = "GetSalesByProductType";
            public const string GetShippingByCarrier = "GetShippingByCarrier";
            public const string GetInventoryByFeature = "GetInventoryByFeature";
            public const string GetListingErrorsByPeriod = "GetListingErrorsByPeriod";
        }


        static readonly ActionParamsClass_GetSalesByPeriod s_params_GetSalesByPeriod = new ActionParamsClass_GetSalesByPeriod();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetSalesByPeriod GetSalesByPeriodParams { get { return s_params_GetSalesByPeriod; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetSalesByPeriod
        {
            public readonly string periodType = "periodType";
            public readonly string valueType = "valueType";
        }
        static readonly ActionParamsClass_GetSalesByMarketplace s_params_GetSalesByMarketplace = new ActionParamsClass_GetSalesByMarketplace();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetSalesByMarketplace GetSalesByMarketplaceParams { get { return s_params_GetSalesByMarketplace; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetSalesByMarketplace
        {
            public readonly string periodType = "periodType";
            public readonly string valueType = "valueType";
        }
        static readonly ActionParamsClass_GetSalesByProductType s_params_GetSalesByProductType = new ActionParamsClass_GetSalesByProductType();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetSalesByProductType GetSalesByProductTypeParams { get { return s_params_GetSalesByProductType; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetSalesByProductType
        {
            public readonly string periodType = "periodType";
            public readonly string valueType = "valueType";
        }
        static readonly ActionParamsClass_GetShippingByCarrier s_params_GetShippingByCarrier = new ActionParamsClass_GetShippingByCarrier();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetShippingByCarrier GetShippingByCarrierParams { get { return s_params_GetShippingByCarrier; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetShippingByCarrier
        {
            public readonly string periodType = "periodType";
            public readonly string selectedCarrier = "selectedCarrier";
        }
        static readonly ActionParamsClass_GetInventoryByFeature s_params_GetInventoryByFeature = new ActionParamsClass_GetInventoryByFeature();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetInventoryByFeature GetInventoryByFeatureParams { get { return s_params_GetInventoryByFeature; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetInventoryByFeature
        {
            public readonly string featureId = "featureId";
            public readonly string valueType = "valueType";
            public readonly string selectedFeatureId = "selectedFeatureId";
        }
        static readonly ActionParamsClass_GetListingErrorsByPeriod s_params_GetListingErrorsByPeriod = new ActionParamsClass_GetListingErrorsByPeriod();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetListingErrorsByPeriod GetListingErrorsByPeriodParams { get { return s_params_GetListingErrorsByPeriod; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetListingErrorsByPeriod
        {
            public readonly string periodType = "periodType";
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
                public readonly string _InventoryByFeature = "_InventoryByFeature";
                public readonly string _InventoryBySeason = "_InventoryBySeason";
                public readonly string _InventoryStatus = "_InventoryStatus";
                public readonly string _ListingErrorsByDate = "_ListingErrorsByDate";
                public readonly string _News = "_News";
                public readonly string _SalesByDate = "_SalesByDate";
                public readonly string _SalesByMarketplace = "_SalesByMarketplace";
                public readonly string _SalesByProductType = "_SalesByProductType";
                public readonly string _ShippingByCarrier = "_ShippingByCarrier";
                public readonly string _SystemMessages = "_SystemMessages";
                public readonly string _SystemStatus = "_SystemStatus";
                public readonly string Index = "Index";
            }
            public readonly string _InventoryByFeature = "~/Views/Dashboard/_InventoryByFeature.cshtml";
            public readonly string _InventoryBySeason = "~/Views/Dashboard/_InventoryBySeason.cshtml";
            public readonly string _InventoryStatus = "~/Views/Dashboard/_InventoryStatus.cshtml";
            public readonly string _ListingErrorsByDate = "~/Views/Dashboard/_ListingErrorsByDate.cshtml";
            public readonly string _News = "~/Views/Dashboard/_News.cshtml";
            public readonly string _SalesByDate = "~/Views/Dashboard/_SalesByDate.cshtml";
            public readonly string _SalesByMarketplace = "~/Views/Dashboard/_SalesByMarketplace.cshtml";
            public readonly string _SalesByProductType = "~/Views/Dashboard/_SalesByProductType.cshtml";
            public readonly string _ShippingByCarrier = "~/Views/Dashboard/_ShippingByCarrier.cshtml";
            public readonly string _SystemMessages = "~/Views/Dashboard/_SystemMessages.cshtml";
            public readonly string _SystemStatus = "~/Views/Dashboard/_SystemStatus.cshtml";
            public readonly string Index = "~/Views/Dashboard/Index.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_DashboardController : Amazon.Web.Controllers.DashboardController
    {
        public T4MVC_DashboardController() : base(Dummy.Instance) { }

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
        partial void GetSystemStatusOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSystemStatus()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSystemStatus);
            GetSystemStatusOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void GetSystemMessagesOverride(T4MVC_System_Web_Mvc_ActionResult callInfo);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSystemMessages()
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSystemMessages);
            GetSystemMessagesOverride(callInfo);
            return callInfo;
        }

        [NonAction]
        partial void GetSalesByPeriodOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int periodType, int valueType);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSalesByPeriod(int periodType, int valueType)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSalesByPeriod);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "periodType", periodType);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "valueType", valueType);
            GetSalesByPeriodOverride(callInfo, periodType, valueType);
            return callInfo;
        }

        [NonAction]
        partial void GetSalesByMarketplaceOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int periodType, int valueType);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSalesByMarketplace(int periodType, int valueType)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSalesByMarketplace);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "periodType", periodType);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "valueType", valueType);
            GetSalesByMarketplaceOverride(callInfo, periodType, valueType);
            return callInfo;
        }

        [NonAction]
        partial void GetSalesByProductTypeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int periodType, int valueType);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetSalesByProductType(int periodType, int valueType)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetSalesByProductType);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "periodType", periodType);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "valueType", valueType);
            GetSalesByProductTypeOverride(callInfo, periodType, valueType);
            return callInfo;
        }

        [NonAction]
        partial void GetShippingByCarrierOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int periodType, string selectedCarrier);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetShippingByCarrier(int periodType, string selectedCarrier)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetShippingByCarrier);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "periodType", periodType);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "selectedCarrier", selectedCarrier);
            GetShippingByCarrierOverride(callInfo, periodType, selectedCarrier);
            return callInfo;
        }

        [NonAction]
        partial void GetInventoryByFeatureOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int featureId, int valueType, int? selectedFeatureId);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetInventoryByFeature(int featureId, int valueType, int? selectedFeatureId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetInventoryByFeature);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "featureId", featureId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "valueType", valueType);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "selectedFeatureId", selectedFeatureId);
            GetInventoryByFeatureOverride(callInfo, featureId, valueType, selectedFeatureId);
            return callInfo;
        }

        [NonAction]
        partial void GetListingErrorsByPeriodOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, int periodType);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetListingErrorsByPeriod(int periodType)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetListingErrorsByPeriod);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "periodType", periodType);
            GetListingErrorsByPeriodOverride(callInfo, periodType);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114
