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
    public partial class InventoryQuantityController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryQuantityController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected InventoryQuantityController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult EditStyleQuantity()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.EditStyleQuantity);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.JsonResult GenerateOpenBox()
        {
            return new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.GenerateOpenBox);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.JsonResult Redistribute()
        {
            return new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.Redistribute);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Validate()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Validate);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Submit()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Submit);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public InventoryQuantityController Actions { get { return MVC.InventoryQuantity; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "InventoryQuantity";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "InventoryQuantity";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string EditStyleQuantity = "EditStyleQuantity";
            public readonly string GenerateOpenBox = "GenerateOpenBox";
            public readonly string Redistribute = "Redistribute";
            public readonly string Validate = "Validate";
            public readonly string Submit = "Submit";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string EditStyleQuantity = "EditStyleQuantity";
            public const string GenerateOpenBox = "GenerateOpenBox";
            public const string Redistribute = "Redistribute";
            public const string Validate = "Validate";
            public const string Submit = "Submit";
        }


        static readonly ActionParamsClass_EditStyleQuantity s_params_EditStyleQuantity = new ActionParamsClass_EditStyleQuantity();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_EditStyleQuantity EditStyleQuantityParams { get { return s_params_EditStyleQuantity; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_EditStyleQuantity
        {
            public readonly string styleId = "styleId";
        }
        static readonly ActionParamsClass_GenerateOpenBox s_params_GenerateOpenBox = new ActionParamsClass_GenerateOpenBox();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GenerateOpenBox GenerateOpenBoxParams { get { return s_params_GenerateOpenBox; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GenerateOpenBox
        {
            public readonly string styleId = "styleId";
        }
        static readonly ActionParamsClass_Redistribute s_params_Redistribute = new ActionParamsClass_Redistribute();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Redistribute RedistributeParams { get { return s_params_Redistribute; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Redistribute
        {
            public readonly string styleId = "styleId";
        }
        static readonly ActionParamsClass_Validate s_params_Validate = new ActionParamsClass_Validate();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Validate ValidateParams { get { return s_params_Validate; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Validate
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
                public readonly string StyleQuantityViewModel = "StyleQuantityViewModel";
            }
            public readonly string StyleQuantityViewModel = "~/Views/InventoryQuantity/StyleQuantityViewModel.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_InventoryQuantityController : Amazon.Web.Controllers.InventoryQuantityController
    {
        public T4MVC_InventoryQuantityController() : base(Dummy.Instance) { }

        [NonAction]
        partial void EditStyleQuantityOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, long styleId);

        [NonAction]
        public override System.Web.Mvc.ActionResult EditStyleQuantity(long styleId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.EditStyleQuantity);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            EditStyleQuantityOverride(callInfo, styleId);
            return callInfo;
        }

        [NonAction]
        partial void GenerateOpenBoxOverride(T4MVC_System_Web_Mvc_JsonResult callInfo, long styleId);

        [NonAction]
        public override System.Web.Mvc.JsonResult GenerateOpenBox(long styleId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.GenerateOpenBox);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            GenerateOpenBoxOverride(callInfo, styleId);
            return callInfo;
        }

        [NonAction]
        partial void RedistributeOverride(T4MVC_System_Web_Mvc_JsonResult callInfo, long styleId);

        [NonAction]
        public override System.Web.Mvc.JsonResult Redistribute(long styleId)
        {
            var callInfo = new T4MVC_System_Web_Mvc_JsonResult(Area, Name, ActionNames.Redistribute);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "styleId", styleId);
            RedistributeOverride(callInfo, styleId);
            return callInfo;
        }

        [NonAction]
        partial void ValidateOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.Inventory.StyleQuantityViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Validate(Amazon.Web.ViewModels.Inventory.StyleQuantityViewModel model)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Validate);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "model", model);
            ValidateOverride(callInfo, model);
            return callInfo;
        }

        [NonAction]
        partial void SubmitOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, Amazon.Web.ViewModels.Inventory.StyleQuantityViewModel model);

        [NonAction]
        public override System.Web.Mvc.ActionResult Submit(Amazon.Web.ViewModels.Inventory.StyleQuantityViewModel model)
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
