using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileConference.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ForbiddenElseAttribute : AuthorizeAttribute
    {
        private string[] UserProfilesRequired { get; set; }

        public ForbiddenElseAttribute(params string[] userProfilesRequired)
        {
            this.UserProfilesRequired = userProfilesRequired;
        }

        public override void OnAuthorization(AuthorizationContext context)
        {
            bool authorized = false;

            foreach (var role in this.UserProfilesRequired)
                if (HttpContext.Current.User.IsInRole(role))
                    authorized = true;

            if (!authorized)
            {
                var url = new UrlHelper(context.RequestContext);
                var logonUrl = url.Action("NotFoundError", "Home");
                context.Result = new RedirectResult(logonUrl);
            }
        }
    }
}