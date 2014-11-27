using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileConference.Interface
{
    public interface ICapchaManager
    {
        MemoryStream GetCapcha(int width, int height, out string answer);
    }
}
