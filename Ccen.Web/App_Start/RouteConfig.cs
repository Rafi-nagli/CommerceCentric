using System.Web.Mvc;
using System.Web.Routing;

namespace Amazon.Web.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "GetImage",
                url: "Image/Get/{filename}",
                defaults: new { controller = "ImageGet", action = "Get" });

            routes.MapRoute(
                name: "GetFeed",
                url: "Feed/GetFeed/{feedId}",
                defaults: new { controller = "Feed", action = "GetFeed" });


            routes.MapRoute(
                name: "UploadImage",
                url: "Image/UploadImage/{filename}",
                defaults: new { controller = "ImageGet", action = "UploadImage" });

            routes.MapRoute(
                name: "GetGrouponFeed",
                url: "Groupon/GetFeed/{filename}",
                defaults: new { controller = "Groupon", action = "GetFeed" });


            routes.MapRoute(
                name: "SwatchImage",
                url: "Image/Swatch/{filename}",
                defaults: new { controller = "ImageGet", action = "Swatch" });
            routes.MapRoute(
                name: "RawImage",
                url: "Image/Raw/{filename}",
                defaults: new { controller = "ImageGet", action = "Raw" });
            routes.MapRoute(
                name: "WalmartImage",
                url: "Image/Walmart/{filename}",
                defaults: new { controller = "ImageGet", action = "Walmart" });
            routes.MapRoute(
                name: "JetImage",
                url: "Image/Jet/{filename}",
                defaults: new { controller = "ImageGet", action = "Jet" });
            routes.MapRoute(
                name: "eBayImage",
                url: "Image/eBay/{filename}",
                defaults: new { controller = "ImageGet", action = "eBay" });
            routes.MapRoute(
                name: "GrouponImages",
                url: "GrouponImages/{filename}",
                defaults: new { controller = "ImageGet", action = "Groupon" });

            routes.MapRoute(
                name: "Thumbnail",
                url: "Image/Thumbnail/{width}/{height}/{rotate}/{*path}",
                defaults: new { controller = "ImageGet", action = "Thumbnail", width = 0, height = 0, rotate = "", path = "" },
                constraints: new { width = @"\d+", height=@"\d+" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                //defaults: new { controller = "Inventory", action = "Styles", id = UrlParameter.Optional }
            );
        }
    }
}