using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MobileConference.GlobalData;

namespace MobileConference.Controllers
{
    public class ServiceApiController : ApiController
    {
        // GET api/<controller>/5
        public string Get(string id)
        {
            return (ServiceOperation.DoneAllOperations(id)).ToString();
        }

    }
}