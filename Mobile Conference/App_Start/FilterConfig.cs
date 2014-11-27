using System.Web.Mvc;

namespace MobileConference
{
    /// <summary>
    /// Register filter when application was started
    /// </summary>
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}