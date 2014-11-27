using System.Collections.Generic;
using Microsoft.Web.WebPages.OAuth;
using MobileConference.GlobalData;

namespace MobileConference
{
    /// <summary>
    /// Registration and login via social network
    /// </summary>
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            //registration via facebook
            var facebooksocialData = new Dictionary<string, object> {{"divClass", "facebook"}};
            OAuthWebSecurity.RegisterFacebookClient(
                appId: "797437023621814",
                appSecret: "2f5840dcfef7c6076a1ce64200b6724d",
                displayName:"Facebook",
                extraData: facebooksocialData);

            //registration via vkontakte
            var vkData = new Dictionary<string, object> {{"divClass", "vk"}};
            OAuthWebSecurity.RegisterClient(
                client: new VKontakteAuthenticationClient(
                    "4527052",
                    "ui24TE9jRua7C2bsFcPE"),
                displayName: "ВКонтакте",
                extraData: vkData);
        }
    }
}
