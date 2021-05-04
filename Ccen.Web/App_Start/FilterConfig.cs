using System.Web.Mvc;
using Amazon.Web.Filters;

namespace Amazon.Web.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new SecurityFilter());
            filters.Add(new LogHandleErrorAttribute()); //HandleErrorAttribute
        }
    }
}