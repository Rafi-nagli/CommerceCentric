using System;
using System.Web.Optimization;
using Amazon.Web.Models;

namespace Amazon.Web.App_Start
{
    public class BundleConfig
    {
        public static void AddDefaultIgnorePatterns(IgnoreList ignoreList)
        {
            if (ignoreList == null)
                throw new ArgumentNullException("ignoreList");
            ignoreList.Ignore("*.intellisense.js");
            ignoreList.Ignore("*-vsdoc.js");
            ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
            //ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
            //ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
        }

        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            //BundleTable.EnableOptimizations = true;

            bundles.IgnoreList.Clear();
            AddDefaultIgnorePatterns(bundles.IgnoreList);

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                "~/Scripts/kendo/2013.3.1119/kendo.web.min.js",
                //"~/Scripts/kendo/2012.2.913/kendo.dataviz.min.js",
                        //"~/Scripts/kendo/2012.2.913/kendo.all.min.js",
                        //"~/Scripts/kendo/2012.2.913/kendo.aspnetmvc.min.js",
                        "~/Scripts/helpers/kendo.web.ext.js"//,
                        //"~/Scripts/kendo/2013.3.1119/kendo.multiselect.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/ko").Include(
                "~/Scripts/knockout-2.2.1.js",
                "~/Scripts/knockout.mapping-2.4.0.js",
                //"~/Scripts/knockout-kendo.min.js",
                "~/Scripts/knockout.validation.js"));

            bundles.Add(new ScriptBundle("~/bundles/ko3").Include(
                "~/Scripts/knockout-3.3.0.js",
                "~/Scripts/knockout.mapping-2.4.0.js",
                "~/Scripts/knockout-kendo.min.js",
                "~/Scripts/knockout.validation.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/bootstrap2-toggle.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.min.js",
                        "~/Scripts/jquery.validate.unobtrusive.min.js",
                        "~/Scripts/jquery.unobtrusive-ajax.min.js",
                        "~/Scripts/jquery.tiny-pubsub.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr")
                .Include("~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/utils")
                .IncludeDirectory("~/Scripts/utils", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/helpers")
                .IncludeDirectory("~/Scripts/helpers", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/app")
                .IncludeDirectory("~/Scripts/app", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/components")
                .IncludeDirectory("~/Scripts/components", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/models")
                .IncludeDirectory("~/Scripts/models", "*.js", true));

            bundles.Add(new ScriptBundle("~/bundles/common").IncludeDirectory(
                "~/Scripts/common", "*.js", true));


            bundles.Add(new StyleBundle("~/Content/css-bootstrap").IncludeDirectory(
                "~/Content/css2", "*.css", true));

            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/Site.css")
                .Include("~/Content/Amazon.css")
                .Include("~/Content/Amazon.controls.css")
                .Include("~/Content/Amazon.orders.css")
                .Include("~/Content/Amazon.admin.css")
                .Include("~/Content/Amazon.kendo.css")
                            //.Include("~/Content/print.css")
                );

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/bootstrap-theme.min.css",
                "~/Content/bootstrap2-toggle.min.css",
                "~/Content/bootstrap-custom.css"));

            bundles.Add(new StyleBundle("~/Content/kendo/2012.2.913/kendo").Include(
                "~/Content/kendo/2013.3.1119/kendo.common.min.css",
                //"~/Content/kendo/2012.2.913/kendo.dataviz.min.css",
                "~/Content/kendo/2013.3.1119/kendo.black.min.css"));

            bundles.Add(new StyleBundle("~/Content/kendo/2013.3.1119/kendo-new").Include(
            "~/Content/kendo/2013.3.1119/kendo.common.min.css",
                        //"~/Content/kendo/2012.2.913/kendo.dataviz.min.css",
            "~/Content/kendo/2013.3.1119/kendo.bootstrap.min.css"));


            bundles.Add(new StyleBundle("~/Content/themes/base/css" ).Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}