using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ImageResizer.Configuration.Logging;
using log4net;

namespace MobileConference.GlobalData
{
    public class CustomLogger:ILogger
    {
        private readonly ILog logger;

        public CustomLogger()
        {
            logger = LogManager.GetLogger(GetType());
        }

        public void Error(string message)
        {
            logger.Error(message);
        }


        public void Error(string message, Exception x)
        {
            logger.Error(message, x);
        }

        public void Fatal(string message)
        {
            logger.Fatal(message);
        }


        #region Not Implement Methods (it's can be implements, if you are need it)
        public void Debug(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Debug(string message)
        {
            throw new NotImplementedException();
        }

        public void Error(string message, params object[] args)
        {
            throw new NotImplementedException();
        }


        public void Fatal(string message, params object[] args)
        {
            throw new NotImplementedException();
        }


        public void Info(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(string message)
        {
            throw new NotImplementedException();
        }

        public bool IsDebugEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsEnabled(string level)
        {
            throw new NotImplementedException();
        }

        public bool IsErrorEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFatalEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsInfoEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsTraceEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsWarnEnabled
        {
            get { throw new NotImplementedException(); }
        }

        public void Log(string level, string message)
        {
            throw new NotImplementedException();
        }

        public string LoggerName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Trace(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Trace(string message)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Warn(string message)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}