using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileConference.Interface
{
    public interface ICustomLogManager
    {
        void Error(string message, Exception exception);
        void Fatal(Exception exception);
        void Message(string message);
    }
}
