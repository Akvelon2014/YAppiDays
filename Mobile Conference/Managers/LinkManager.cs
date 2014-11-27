using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileConference.Managers
{
    public static class LinkManager
    {
        public static bool HasDomain(string link, string domain)
        {
            string address = link;
            int startIndex = link.IndexOf("://");
            if (startIndex >= 0)
            {
                address = link.Substring(startIndex + 3);
            }
            return address.StartsWith(domain) || address.StartsWith("www." + domain);
        }

        public static Dictionary<string, string> GetQuery(string link)
        {
            int startIndex = link.IndexOf("?");
            if (startIndex < 0)
            {
                return new Dictionary<string, string>();
            }
            string address = link.Substring(startIndex + 1);
            return address.Split('&').ToDictionary(l => l.Substring(0, l.IndexOf("=") < 0 ? 0 : l.IndexOf("="))
                , l => l.Substring(l.IndexOf("=")+1));
        }
    }
}