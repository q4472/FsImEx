using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ImEx
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: null,
                url: "ImEx/F0/SendBill/{*pathInfo}",
                defaults: new { controller = "F0", action = "SendBill" }
            );

            routes.MapRoute(
                name: null,
                url: "ImEx/F0/Index/{*pathInfo}",
                defaults: new { controller = "F0", action = "Index" }
            );

            routes.MapRoute(
                name: null,
                url: "ImEx/F0/{*pathInfo}",
                defaults: new { controller = "F0", action = "WaitingPage" }
            );
        }
    }
}
