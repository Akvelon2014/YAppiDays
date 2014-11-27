using System.Web;
using MobileConference.Models;

namespace MobileConference.GlobalData
{
    /// <summary>
    /// Global Entity Framework context
    /// </summary>
    public static class  ContextEF
    {
        private static Entities context;

        public static Entities Context
        {
            get
            {
                string ocKey = "key_" + HttpContext.Current.GetHashCode().ToString("x");
                if (!HttpContext.Current.Items.Contains(ocKey))
                    HttpContext.Current.Items.Add(ocKey, new Entities());
                return HttpContext.Current.Items[ocKey] as Entities;
                //return context ?? (context = new MobileConferenceDBEntities());
            }
        }
    }
}