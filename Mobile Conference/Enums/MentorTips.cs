using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileConference.Enums
{
    [Flags]
    public enum MentorTips : int
    {
        None = 0,
        Intro = 1
    }
}