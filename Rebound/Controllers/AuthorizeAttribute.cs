using System;
using System.Web.Mvc;


public class AccessAuthorizeAttribute : AuthorizeAttribute
{
    public override void OnAuthorization(AuthorizationContext filterContext)
     
    {
        
        if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
        {
            filterContext.Result = new RedirectResult("~/Error/InternalServerError");
            return;
        }

        if (filterContext.Result is HttpUnauthorizedResult)
        {
            filterContext.Result = new RedirectResult("~/Error/Unauthorized");
        }
        OnAuthorizationHelp(filterContext);

        base.OnAuthorization(filterContext);
        


    }

    internal void OnAuthorizationHelp(AuthorizationContext filterContext)
    {

        if (filterContext.Result is HttpUnauthorizedResult)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                filterContext.HttpContext.Response.StatusCode = 403;
                filterContext.Result = new RedirectResult("~/Error/InternalServerError");
                filterContext.HttpContext.Response.End();
            }
        }
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        // returns a 401 already
        
        if (filterContext.HttpContext.Request.IsAjaxRequest())
        {
            filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
            filterContext.Result = new RedirectResult("~/Error/Unauthorized");
        }
        filterContext.Result = new RedirectResult("~/Error/Unauthorized");
    }
}

