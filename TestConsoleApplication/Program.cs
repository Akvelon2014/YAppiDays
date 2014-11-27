using System;
using System.IO;
using System.Net;


namespace TestConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            string password = "password";
            string address = "http://localhost:50040/api/ServiceApi/";
            //var proxy = new YAppiServiceClient.YAppiAdminServiceClient();
            //bool p = proxy.DoneAllOperations(password);
            //Console.WriteLine(p);
            //proxy.Close();
            var request = WebRequest.Create(address+password) as HttpWebRequest;
            string result;
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                     result = reader.ReadToEnd();
                }
            }
            Console.Write(result);
            Console.Read();
        }
    }
}
