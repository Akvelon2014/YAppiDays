using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Castle.Core.Internal;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Models;

namespace MobileConference.Helper
{
    public static class AdditionalExtender
    {
        public static bool ClearMeasure(this ModelStateDictionary dict)
        {
            bool isCleared= false;
            if (dict.ContainsKey("x1") )
            {
                if (dict["x1"].Errors.Any()) isCleared = true;
                dict.Remove("x1");
            }
            if (dict.ContainsKey("x2") )
            {
                if (dict["x2"].Errors.Any()) isCleared = true;
                dict.Remove("x2");
            }
            if (dict.ContainsKey("y1"))
            {
                if (dict["y1"].Errors.Any()) isCleared = true;
                dict.Remove("y1");
            }
            if (dict.ContainsKey("y2") )
            {
                if (dict["y2"].Errors.Any()) isCleared = true;
                dict.Remove("y2");
            }
            return isCleared;
        }

        public static string NullOrTrim(this string data)
        {
            return (data == null) ? null : data.Trim();
        }

        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return GlobalValuesAndStrings.DuplicateUserNameMessage;

                case MembershipCreateStatus.DuplicateEmail:
                    return GlobalValuesAndStrings.DuplicateEmailMessage;

                case MembershipCreateStatus.InvalidPassword:
                    return GlobalValuesAndStrings.InvalidPasswordMessage;

                case MembershipCreateStatus.InvalidEmail:
                    return GlobalValuesAndStrings.InvalidEmailMessage;

                case MembershipCreateStatus.InvalidAnswer:
                    return GlobalValuesAndStrings.InvalidAnswerMessage;

                case MembershipCreateStatus.InvalidQuestion:
                    return GlobalValuesAndStrings.InvalidQuestionMessage;

                case MembershipCreateStatus.InvalidUserName:
                    return GlobalValuesAndStrings.InvalidUserNameMessage;

                case MembershipCreateStatus.ProviderError:
                    return GlobalValuesAndStrings.ProviderErrorMessage;

                case MembershipCreateStatus.UserRejected:
                    return GlobalValuesAndStrings.UserRejectedMessage;

                default:
                    return GlobalValuesAndStrings.UnknownErrorMessage;
            }
        }

        public static bool SendEmailAfterRegistration(this IEmailManager emailManager, string email)
        {
            var userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            var cryptManager = ContainerDI.Container.Resolve<ICryptManager>();
            var user = userRepository.GetUserByEmail(email);
            if (user == null) return false;

            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string hash = cryptManager.GetHash(user.EmailConfirmData);
            hash = cryptManager.GetHash(hash);
            string link = OptionModel.FullyQualifiedApplicationPath + url.Action("ConfirmMessage", "Account", new { login = user.Login, code = hash });
            string text = GlobalValuesAndStrings.MessageForUserAfterRegistration(user.FullName(), link);
            emailManager.SendMessage(user.Email, GlobalValuesAndStrings.TitleToAfterRegistrationMessage, text);
            return true;
        }

        public static void SendEmailForUsers(this IEmailManager emailManager, string[] emails, string title, string message)
        {
            foreach (var email in emails)
            {
                if(email == GlobalValuesAndStrings.EmailForInternalUser) continue;
                emailManager.SendMessage(email, title, message);
            }
        }

        public static string AsLink(this string text,string title="", bool blank = true)
        {
            return string.Format("<a href='{0}' {1}>{2}</a>",text,blank?"target='_blank' ":string.Empty
                ,title.IsNullOrEmpty()?text:title);
        }
    }
}