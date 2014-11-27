using System.Configuration;
using System.IO;
using System.Web;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Models;

namespace MobileConference.Managers
{
    public class FakeEmailManager:IEmailManager
    {

        public void SendMessage(string emailTo, string title, string message)
        {
            string path = HttpContext.Current.Server.MapPath("/EmailFromFakeEmailManager");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string pathToEmail = path + "/" + emailTo;
            if (!Directory.Exists(pathToEmail)) Directory.CreateDirectory(pathToEmail);
            File.AppendAllText(pathToEmail+"/"+title+".txt",message);
        }


        public void SendMessageToAdmin(FeedbackModel feedback)
        {
            string email = ConfigurationManager.AppSettings["EmailFeedback"];
            SendMessage(email, GlobalValuesAndStrings.TitleForFeedback, feedback.MessageForSending);
        }
    }
}