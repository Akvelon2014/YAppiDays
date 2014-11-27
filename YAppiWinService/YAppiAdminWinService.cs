using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using YAppiWinService.YAppiWebService;

namespace YAppiWinService
{
    partial class YAppiAdminWinService : ServiceBase
    {
        private const int periodToInvoke = 1800000; //30 minutes
        private const int hoursBeforeRequestToWebService = 24; //1 day

        private Timer timer;
        private DateTime nextDate;
        private int hour;
        private int minute;
        private string password;
        private string address;
        private string logFileName;

        public YAppiAdminWinService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //initialize data setting from app.config
            var lastDate = DateTime.Parse(ConfigurationManager.AppSettings.Get("LastInvokeDate"));
            hour = int.Parse(ConfigurationManager.AppSettings.Get("Hour"));
            minute = int.Parse(ConfigurationManager.AppSettings.Get("Minute"));
            password = ConfigurationManager.AppSettings.Get("Password");
            address = ConfigurationManager.AppSettings.Get("Address");
            logFileName = ConfigurationManager.AppSettings.Get("LogFileName");

            nextDate = lastDate;
            timer = new Timer(SendRequestToWebService,null, 0, periodToInvoke);

            WriteInLog(string.Format("YAppiDays Service started with params: \r\n lastDate = {0} \r\n hour = {1} \r\n" +
                                     " minute = {2} \r\n nextDate = {3} \r\n",lastDate,hour,minute,nextDate));
        }

        private StreamWriter log = null;
        private void WriteInLog(string message)
        {
            string directory = Path.GetDirectoryName(logFileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (log == null)
            {
                log = new StreamWriter(new FileStream(logFileName, FileMode.Append));
            }
            log.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " - " + message+"\r\n");
            log.Flush();
        }

        private void SendRequestToWebService(object state)
        {
            if (DateTime.Now < nextDate)
            {
                return;
            }
            WriteInLog("Open connection with web service");
            //request to web service
            string result = "failed";
            try
            {
                result = SendToWebServiceMessage();
            }
            catch (Exception)
            {
                WriteInLog("Connection with the web service is unsuccessfull");
                return;
            }
            nextDate = GetNextDate(DateTime.Now);
            ConfigurationManager.AppSettings.Set("LastInvokeDate", nextDate.ToString() );
            WriteInLog("Result: "+result);
            WriteInLog("Close connection with web service. LastInvokeDate now is " + nextDate.ToString("dd.MM HH:mm:ss"));
        }

        private DateTime GetNextDate(DateTime lastDate)
        {
            var nextDateTime = new DateTime(lastDate.Year, lastDate.Month, lastDate.Day, hour, minute, 0);
            return nextDateTime.AddHours(hoursBeforeRequestToWebService);
        }

        protected override void OnStop()
        {
            WriteInLog("YAppiDays Service stopped");
            log.Close();
            timer.Dispose();
        }

        private string SendToWebServiceMessage()
        {
            var request = WebRequest.Create(address + password) as HttpWebRequest;
            string result;
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }
    }
}
