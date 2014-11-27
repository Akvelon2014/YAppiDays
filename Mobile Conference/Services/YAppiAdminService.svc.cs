using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Managers;
using MobileConference.Models;

namespace MobileConference.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "YAppiAdminService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select YAppiAdminService.svc or YAppiAdminService.svc.cs at the Solution Explorer and start debugging.
    public class YAppiAdminService : IYAppiAdminService
    {
        public bool DoneAllOperations(string password)
        {
            return ServiceOperation.DoneAllOperations(password);
        }
    }
}
