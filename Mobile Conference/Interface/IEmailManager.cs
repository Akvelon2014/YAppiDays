using MobileConference.Models;

namespace MobileConference.Interface
{
    public interface IEmailManager
    {
        void SendMessage(string emailTo, string title, string message);
        void SendMessageToAdmin(FeedbackModel feedback);
    }
}
