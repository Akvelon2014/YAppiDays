using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MobileConference.Models;

namespace MobileConference.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ProjectForbiddenPossibleAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext context)
        {
            if (OptionModel.Current.ForbiddenProjectRegistration)
            {
                var url = new UrlHelper(context.RequestContext);
                var logonUrl = url.Action("RegistrationForbidden", "Home");
                context.Result = new RedirectResult(logonUrl);
            }
        }
    }
}