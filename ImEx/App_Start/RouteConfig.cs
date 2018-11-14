using System.Web.Mvc;
using System.Web.Routing;

namespace ImEx
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: null,
                url: "ImEx/F0/SendBill",
                defaults: new { controller = "F0", action = "SendBill" }
            );

            routes.MapRoute(
                name: null,
                url: "ImEx/F0/Index",
                defaults: new { controller = "F0", action = "Index" }
            );

            routes.MapRoute(
                name: null,
                url: "ImEx/F0",
                defaults: new { controller = "F0", action = "WaitingPage" }
            );

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*pathInfo}");
        }
    }
}
