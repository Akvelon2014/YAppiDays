using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Models;

namespace MobileConference.Managers
{
    public class SimpleEmailManager : IEmailManager
    {
        public void SendMessage(string emailTo, string title, string message)
        {
            if (emailTo == null) return;
            var from = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"], "YAppi Days");
            var to = new MailAddress(emailTo);
            var mail = new MailMessage(from, to)
            {
                Subject = title, 
                Body = string.Format("<pre>{0}</pre>",message), 
                IsBodyHtml = true
            };
            var dataProtector = ContainerDI.Container.Resolve<IDataProtectorManager>();
            string password = dataProtector.Decrypt(ConfigurationManager.AppSettings["EmailPassword"]);
            var client = new SmtpClient
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(from.Address, password),
                Host = ConfigurationManager.AppSettings["EmailHost"],
                Port = int.Parse(ConfigurationManager.AppSettings["EmailPort"]),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            try
            {
                client.Send(mail);
            }
            catch (Exception e)
            {
                var logManager = ContainerDI.Container.Resolve<ICustomLogManager>();
                logManager.Error(string.Format("Email {0} is not found",emailTo), e);
            }
        }

        public void SendMessageToAdmin(Models.FeedbackModel feedback)
        {
            string email = ConfigurationManager.AppSettings["EmailFeedback"];
            SendMessage(email, GlobalValuesAndStrings.TitleForFeedback, feedback.MessageForSending);
        }
    }
}