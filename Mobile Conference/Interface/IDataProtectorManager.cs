using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileConference.Interface
{
    public interface IDataProtectorManager
    {
        string Encrypt(string text);
        string Decrypt(string text);
    }
}
