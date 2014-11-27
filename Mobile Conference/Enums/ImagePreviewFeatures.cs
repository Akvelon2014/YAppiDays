using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobileConference.Enums
{
    [Flags]
    public enum ImagePreviewFeatures:int
    {
        None = 0,
        Coordinate = 1,
        Size = 2,
        Border = 4,
        All = 7
    }
}