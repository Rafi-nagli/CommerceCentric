//using RazorEngine;
//using RazorEngine.Templating;

using RazorEngine;
using RazorEngine.Templating;

namespace Amazon.Templates
{
    public class TemplateHelper
    {
        public static string EBayDescriptionTemplateName = "eBayDescription.cshtml";
        public static string EBayDescriptionMultiListingTemplateName = "eBayDescriptionForMultiListing.cshtml";

        public static string RunRazorTemplate(string name, string templateContent, object model)
        {
            //var engine = EngineFactory.
            //var engine = EngineFactory.CreatePhysical(@"D:\path\to\views\folder");
            string html = Engine.Razor.RunCompile(templateContent, name, model.GetType(), model);
            return html;
        }
    }
}
