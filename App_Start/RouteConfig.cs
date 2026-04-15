using System.Web.Mvc;
using System.Web.Routing;

namespace HQSoft
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "FS10901",
                url: "FS10901/{action}/{id}",
                defaults: new { controller = "FS10901", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "FS10901", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
