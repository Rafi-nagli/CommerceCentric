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
    public partial class InventoryPriceController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryPriceController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected InventoryPriceController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult EditStylePrice()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.EditStylePrice);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Submit()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetListingsByStyleSize()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetListingsByStyleSize);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult SetListingsForStyleSize()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.SetListingsForStyleSize);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryPriceController Actions { get { return MVC.InventoryPrice; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "InventoryPrice";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "InventoryPrice";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string EditStylePrice = "EditStylePrice";
            public readonly string Submit = "Submit";
            public readonly string GetListingsByStyleSize = "GetListingsByStyleSize";
            public readonly string SetListingsForStyleSize = "SetListingsForStyleSize";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string EditStylePrice = "EditStylePrice";
            public const string Submit = "Submit";
            public const string GetListingsByStyleSize = "GetListingsByStyleSize";
            public const string SetListingsForStyleSize = "SetListingsForStyleSize";
        }


        static readonly ActionParamsClass_EditStylePrice s_params_EditStylePrice = new ActionParamsClass_EditStylePrice();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_EditStylePrice EditStylePriceParams { get { return s_params_EditStylePrice; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_EditStylePrice
        {
            public readonly string styleId = "styleId";
        }
        static readonly ActionParamsClass_Submit s_params_Submit = new ActionParamsClass_Submit();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Submit SubmitParams { get { return s_params_Submit; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Submit
        {
            public readonly string model = "model";
        }
        static readonly ActionParamsClass_GetListingsByStyleSize s_params_GetListingsByStyleSize = new ActionParamsClass_GetListingsByStyleSize();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetListingsByStyleSize GetListingsByStyleSizeParams { get { return s_params_GetListingsByStyleSize; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetListingsByStyleSize
        {
            public readonly string styleItemId = "styleItemId";
            public readonly string initSalePrice = "initSalePrice";
            public readonly string initSFPSalePrice = "initSFPSalePrice";
        }
        static readonly ActionParamsClass_SetListingsForStyleSize s_params_SetListingsForStyleSize = new ActionParamsClass_SetListingsForStyleSize();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_SetListingsForStyleSize SetListingsForStyleSizeParams { get { return s_params_SetListingsForStyleSize; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_SetListingsForStyleSize
        {
            public readonly string styleItemId = "styleItemId";
            public readonly string markets = "markets";
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
                public readonly string _ListingSelectPopup = "_ListingSelectPopup";
                public readonly string StylePriceViewModel = "StylePriceViewModel";
            }
            public readonly string _ListingSelectPopup = "~/Views/InventoryPrice/_ListingSelectPopup.cshtml";
            public readonly string StylePriceViewModel = "~/Views/InventoryPrice/StylePriceViewModel.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_InventoryPriceController : Amazon.Web.Controllers.InventoryPriceController
    {
        public T4MVC_InventoryPriceController() : base(Dummy.Instance) { }

        [NonAction]
        partial void EditStylePriceOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleId);

        [NonAction]
        public override System.Web.Mvc.ActionResult EditStylePrice(long styleId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.EditStylePrice);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            EditStylePriceOverride(callInfo, styleId);
            return callInfo;
        }

        [NonAction]
        partial void SubmitOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.Inventory.StylePriceViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Submit(Amazon.Web.ViewModels.Inventory.StylePriceViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            SubmitOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void GetListingsByStyleSizeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleItemId, decimal? initSalePrice, decimal? initSFPSalePrice);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetListingsByStyleSize(long styleItemId, decimal? initSalePrice, decimal? initSFPSalePrice)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetListingsByStyleSize);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleItemId", styleItemId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "initSalePrice", initSalePrice);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "initSFPSalePrice", initSFPSalePrice);
            GetListingsByStyleSizeOverride(callInfo, styleItemId, initSalePrice, initSFPSalePrice);
            return callInfo;
        }

        [NonAction]
        partial void SetListingsForStyleSizeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleItemId, System.Collections.Generic.IList<Amazon.Web.ViewModels.Inventory.MarketPriceEditViewModel> markets);

        [NonAction]
        public override System.Web.Mvc.ActionResult SetListingsForStyleSize(long styleItemId, System.Collections.Generic.IList<Amazon.Web.ViewModels.Inventory.MarketPriceEditViewModel> markets)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.SetListingsForStyleSize);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleItemId", styleItemId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "markets", markets);
            SetListingsForStyleSizeOverride(callInfo, styleItemId, markets);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114