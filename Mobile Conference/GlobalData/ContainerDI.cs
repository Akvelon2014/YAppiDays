using Castle.Core;
using Castle.Core.Resource;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace MobileConference.GlobalData
{
    /// <summary>
    /// Container for dependency injection
    /// </summary>
    public static class ContainerDI
    {
        private static WindsorContainer container;

        public static WindsorContainer Container
        {
            get
            {
                if (container == null)
                {

                    container = new WindsorContainer(new XmlInterpreter(new ConfigResource("castle")));

                    // IT'S TWO WAY TO USE CASTLE WINDSOR, ONE OF THEM IS COMMENTED, BUT IT CAN WILL BE USED IN THE FUTURE 
                    // INSTEAD ANOTHER WAY

                    //Register controllers
                    //var controllerTypes = from t in Assembly.GetExecutingAssembly().GetTypes().Distinct()
                    //                      where typeof(IController).IsAssignableFrom(t)
                    //                      select t;
                    //foreach (Type t in controllerTypes)
                    //{
                    //    container.Register(Component.For(t).Named(t.FullName).LifeStyle.Is(LifestyleType.Transient));
                    //}

                    container.Register(Classes.FromThisAssembly() //Identify assembly containing your controllers
                        .BasedOn<IController>()
                        .If(Component.IsInSameNamespaceAs<Controllers.HomeController>())
                        .If(t => t.Name.EndsWith("Controller"))
                        .Configure((c => c.LifestyleTransient())));
                }
                return container;
            }
        }
    }
}