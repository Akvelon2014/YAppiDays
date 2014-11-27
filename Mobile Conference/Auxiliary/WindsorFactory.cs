using MobileConference.GlobalData;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace MobileConference.Auxiliary
{
    /// <summary>
    /// Dependency injection factory with using Castle Windsor
    /// </summary>
    public class WindsorFactory:DefaultControllerFactory
    {
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType != null)
            {
                return (IController) ContainerDI.Container.Resolve(controllerType);
            }
            return base.GetControllerInstance(requestContext, controllerType);
        }
    }
}