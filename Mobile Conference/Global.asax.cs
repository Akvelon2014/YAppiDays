using System;
using System.Data.Entity;
using System.Web;
using MobileConference.Auxiliary;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MobileConference.Controllers;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Models;

namespace MobileConference
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(ValidInteger), typeof(ValidIntegerValidator));
            ControllerBuilder.Current.SetControllerFactory(new WindsorFactory());
            
            string log4netConfig = Server.MapPath("~/log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfig));
        }

        
        protected void Application_Error(object sender, EventArgs e)
        {
            var logManager = ContainerDI.Container.Resolve<ICustomLogManager>();
            try
            {
                var httpContext = ((MvcApplication) sender).Context;
                var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
                var currentController = " ";
                var currentAction = " ";

                if (currentRouteData != null)
                {
                    if (currentRouteData.Values["controller"] != null &&
                        !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
                    {
                        currentController = currentRouteData.Values["controller"].ToString();
                    }

                    if (currentRouteData.Values["action"] != null &&
                        !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
                    {
                        currentAction = currentRouteData.Values["action"].ToString();
                    }
                }

                var ex = Server.GetLastError().GetBaseException();
                var requestUrl = Request.Url.ToString();
                var exMessage = "An error has occurred in " + currentController + "/" + currentAction +
                                ", request url: " +
                                requestUrl;

                logManager.Error(exMessage, ex);
                var homeController = ContainerDI.Container.Resolve<HomeController>();
                var routeData = new RouteData();
                var action = "Error";

                if (ex is HttpException)
                {
                    var httpEx = ex as HttpException;
                    switch (httpEx.GetHttpCode())
                    {
                        case 404:
                            action = "NotFoundError";
                            break;
                        default:
                            action = "Error";
                            break;
                    }
                }

                httpContext.ClearError();
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = ex is HttpException ? ((HttpException) ex).GetHttpCode() : 500;
                httpContext.Response.TrySkipIisCustomErrors = true;
                routeData.Values["controller"] = "Home";
                routeData.Values["action"] = action;

                homeController.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
                ((IController) homeController).Execute(new RequestContext(new HttpContextWrapper(httpContext),
                                                                           routeData));
            }
            catch (Exception ex)
            {
                logManager.Fatal(ex);
            }
        } 
    }
}