using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ICSI_WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "login",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "login", UserType = UrlParameter.Optional }
           );

            routes.MapRoute(
               name: "Portal",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Home", action = "DelegatePortal", UserType = UrlParameter.Optional }
           );
        }
    }
}
