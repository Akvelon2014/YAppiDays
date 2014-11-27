using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileConference.Enums
{
    [Flags]
    public enum StudentTips:int
    {
        None = 0,
        Intro = 1,
        NewIdea = 2,
        AfterJoinToIdea = 4,
        IdeaProfile = 8
    }
}