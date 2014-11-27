using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MobileConference.GlobalData;

namespace MobileConference.Enums
{
    public enum TipType
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public static class ExtensionForTipType
    {
        public static string GetClass(this TipType tipType)
        {
            switch (tipType)
            {
                case TipType.Top:
                    return GlobalValuesAndStrings.TopTipClass;
                case TipType.Bottom:
                    return GlobalValuesAndStrings.BottomTipClass;
                case TipType.Right:
                    return GlobalValuesAndStrings.RightTipClass;
                default:
                    return GlobalValuesAndStrings.LeftTipClass;
            }
        }
    }
}