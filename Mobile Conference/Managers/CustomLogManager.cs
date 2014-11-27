using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using MobileConference.GlobalData;
using MobileConference.Interface;

namespace MobileConference.Managers
{
    public class CustomLogManager:ICustomLogManager
    {
        private readonly CustomLogger logger;

        public CustomLogManager()
        {
            logger = new CustomLogger();
        }

        public void Error(string message, Exception exception)
        {
            logger.Error(message,exception);
        }

        public void Fatal(Exception exception)
        {
            logger.Fatal(BuildExceptionMessage(exception));
        }

        public void Message(string message)
        {
            logger.Debug(message);
        }

        private static string BuildExceptionMessage(Exception exception)
        {
            Exception logException = exception;
            if (exception.InnerException != null)
                logException = exception.InnerException;
            string errorMessage = Environment.NewLine + "Error in Path :" + HttpContext.Current.Request.Path;

            errorMessage += Environment.NewLine + "Raw Url :" + HttpContext.Current.Request.RawUrl;
            errorMessage += Environment.NewLine + "Message :" + logException.Message;
            errorMessage += Environment.NewLine + "Source :" + logException.Source;
            errorMessage += Environment.NewLine + "Stack Trace :" + logException.StackTrace;
            errorMessage += Environment.NewLine + "Target Site :" + logException.TargetSite;

            return errorMessage;
        }
    }
}