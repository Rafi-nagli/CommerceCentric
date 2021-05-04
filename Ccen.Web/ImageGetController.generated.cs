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
    public partial class ImageGetController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ImageGetController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected ImageGetController(Dummy d) { }

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
        public virtual System.Web.Mvc.ActionResult Thumbnail()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Thumbnail);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult GetUploadImage()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetUploadImage);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Swatch()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Swatch);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Walmart()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Walmart);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Jet()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Jet);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult eBay()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.eBay);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Groupon()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Groupon);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Raw()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Raw);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult UploadImage()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UploadImage);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Get()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Get);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ImageGetController Actions { get { return MVC.ImageGet; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "ImageGet";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "ImageGet";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Thumbnail = "Thumbnail";
            public readonly string GetUploadImage = "GetUploadImage";
            public readonly string Swatch = "Swatch";
            public readonly string Walmart = "Walmart";
            public readonly string Jet = "Jet";
            public readonly string eBay = "eBay";
            public readonly string Groupon = "Groupon";
            public readonly string Raw = "Raw";
            public readonly string UploadImage = "UploadImage";
            public readonly string Get = "Get";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Thumbnail = "Thumbnail";
            public const string GetUploadImage = "GetUploadImage";
            public const string Swatch = "Swatch";
            public const string Walmart = "Walmart";
            public const string Jet = "Jet";
            public const string eBay = "eBay";
            public const string Groupon = "Groupon";
            public const string Raw = "Raw";
            public const string UploadImage = "UploadImage";
            public const string Get = "Get";
        }


        static readonly ActionParamsClass_Thumbnail s_params_Thumbnail = new ActionParamsClass_Thumbnail();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Thumbnail ThumbnailParams { get { return s_params_Thumbnail; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Thumbnail
        {
            public readonly string path = "path";
            public readonly string width = "width";
            public readonly string height = "height";
            public readonly string rotate = "rotate";
        }
        static readonly ActionParamsClass_GetUploadImage s_params_GetUploadImage = new ActionParamsClass_GetUploadImage();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_GetUploadImage GetUploadImageParams { get { return s_params_GetUploadImage; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_GetUploadImage
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_Swatch s_params_Swatch = new ActionParamsClass_Swatch();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Swatch SwatchParams { get { return s_params_Swatch; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Swatch
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_Walmart s_params_Walmart = new ActionParamsClass_Walmart();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Walmart WalmartParams { get { return s_params_Walmart; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Walmart
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_Jet s_params_Jet = new ActionParamsClass_Jet();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Jet JetParams { get { return s_params_Jet; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Jet
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_eBay s_params_eBay = new ActionParamsClass_eBay();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_eBay eBayParams { get { return s_params_eBay; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_eBay
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_Groupon s_params_Groupon = new ActionParamsClass_Groupon();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Groupon GrouponParams { get { return s_params_Groupon; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Groupon
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_Raw s_params_Raw = new ActionParamsClass_Raw();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Raw RawParams { get { return s_params_Raw; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Raw
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_UploadImage s_params_UploadImage = new ActionParamsClass_UploadImage();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_UploadImage UploadImageParams { get { return s_params_UploadImage; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_UploadImage
        {
            public readonly string filename = "filename";
        }
        static readonly ActionParamsClass_Get s_params_Get = new ActionParamsClass_Get();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Get GetParams { get { return s_params_Get; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Get
        {
            public readonly string filename = "filename";
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
    public partial class T4MVC_ImageGetController : Amazon.Web.Controllers.ImageGetController
    {
        public T4MVC_ImageGetController() : base(Dummy.Instance) { }

        [NonAction]
        partial void ThumbnailOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string path, int width, int height, string rotate);

        [NonAction]
        public override System.Web.Mvc.ActionResult Thumbnail(string path, int width, int height, string rotate)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Thumbnail);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "path", path);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "width", width);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "height", height);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "rotate", rotate);
            ThumbnailOverride(callInfo, path, width, height, rotate);
            return callInfo;
        }

        [NonAction]
        partial void GetUploadImageOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult GetUploadImage(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.GetUploadImage);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            GetUploadImageOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void SwatchOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult Swatch(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Swatch);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            SwatchOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void WalmartOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult Walmart(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Walmart);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            WalmartOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void JetOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult Jet(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Jet);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            JetOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void eBayOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult eBay(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.eBay);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            eBayOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void GrouponOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult Groupon(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Groupon);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            GrouponOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void RawOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult Raw(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Raw);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            RawOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void UploadImageOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult UploadImage(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.UploadImage);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            UploadImageOverride(callInfo, filename);
            return callInfo;
        }

        [NonAction]
        partial void GetOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string filename);

        [NonAction]
        public override System.Web.Mvc.ActionResult Get(string filename)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Get);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "filename", filename);
            GetOverride(callInfo, filename);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591, 3008, 3009, 0108, 0114