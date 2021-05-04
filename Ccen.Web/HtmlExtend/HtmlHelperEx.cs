using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Amazon.Web.HtmlExtend
{
    static public class HtmlHelperEx
    {
        public static ViewDataDictionary GetPrefix(string prefix)
        {
            return new ViewDataDictionary
            {
                TemplateInfo = new System.Web.Mvc.TemplateInfo
                {
                    HtmlFieldPrefix = prefix
                }
            };
        }

        public static void RenderPartialWithPrefix(
            this HtmlHelper helper,
            string partialViewName,
            object model,
            string prefix)
        {
            helper.RenderPartial(partialViewName,
                model,
                new ViewDataDictionary
                {
                    TemplateInfo = new System.Web.Mvc.TemplateInfo
                    {
                        HtmlFieldPrefix = prefix
                    }
                });
        }

        public static MvcHtmlString PartialWithPrefix(
            this HtmlHelper helper,
            string partialViewName,
            object model,
            string prefix)
        {
            return helper.Partial(partialViewName,
                    model,
                    new ViewDataDictionary
                    {
                        TemplateInfo = new System.Web.Mvc.TemplateInfo
                        {
                            HtmlFieldPrefix = prefix
                        }
                    });
        }
    }
}