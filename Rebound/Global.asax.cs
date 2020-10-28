using Rebound.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Rebound
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpContext = ((HttpApplication)sender).Context;
            httpContext.Response.Clear();
            httpContext.ClearError();

            if (new HttpRequestWrapper(httpContext.Request).IsAjaxRequest())
            {
                httpContext.Response.StatusCode = 403;
                httpContext.Response.Clear();
                ExecuteErrorController(httpContext, exception as HttpException);
                //return;
            }

            ExecuteErrorController(httpContext, exception as HttpException);
        }

        private void ExecuteErrorController(HttpContext httpContext, HttpException exception)
        {
            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";

            if (exception != null && exception.GetHttpCode() == (int)HttpStatusCode.NotFound)
            {
                routeData.Values["action"] = "NotFound";
            }
            else if (exception != null && exception.GetHttpCode() == (int)HttpStatusCode.ExpectationFailed)
            {
                routeData.Values["action"] = "InternalServerError";
            }
            else if (exception != null && exception.GetHttpCode() == (int)HttpStatusCode.InternalServerError)
            {
                routeData.Values["action"] = "InternalServerError";
            }
            else if (exception != null && exception.GetHttpCode() == (int)HttpStatusCode.RequestTimeout)
            {
                routeData.Values["action"] = "InternalServerError";
            }
            else if (exception != null && exception.GetHttpCode() == (int)HttpStatusCode.Forbidden)
            {
                routeData.Values["action"] = "InternalServerError";
            }
            else if (exception.GetHttpCode() == (int)HttpStatusCode.Unauthorized)
            {
                routeData.Values["action"] = "Unauthorized";
            }
            else
            {
                routeData.Values["action"] = "InternalServerError";
            }

            using (Controller controller = new ErrorController())
            {
                ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
            }
        }
    }
}
