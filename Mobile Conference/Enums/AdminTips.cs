using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileConference.Enums
{
    [Flags]
    public enum AdminTips : int
    {
        None = 0,
        Intro = 1,
        ManageUser = 2,
        ManageEvent = 4,
        GlobalEvent = 8,
        GlobalEventProfile = 16,
        MaterialCreate = 32
    }
}