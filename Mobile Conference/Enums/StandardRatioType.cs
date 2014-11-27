using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace MobileConference.Enums
{
    public enum StandardRatioType
    {
        IdeaRatio = 0,
        UserRatio = 1,
        CompanyRatio = 2,
        EventRatio = 3,
        AwardRatio = 4
    }

    public static class StandardRationTypeExtend
    {
        public static string GetCSSClass(this StandardRatioType ratio)
        {
            switch (ratio)
            {
                case StandardRatioType.IdeaRatio:
                    return "ideaRatio";
                    break;
                case StandardRatioType.AwardRatio:
                    return "awardRatio";
                    break;
                case StandardRatioType.UserRatio:
                    return "userRatio";
                    break;
            }
            return string.Empty;
        }

        public static string GetCSSClassForPreview(this StandardRatioType ratio)
        {
            switch (ratio)
            {
                case StandardRatioType.IdeaRatio:
                    return "ideaImage";
                    break;
                case StandardRatioType.CompanyRatio:
                    return "companyImage";
                    break;
                case StandardRatioType.EventRatio:
                    return "eventImage";
                    break;
                case StandardRatioType.UserRatio:
                    return "userImage";
                    break;
                case StandardRatioType.AwardRatio:
                    return "awardImage";
                    break;
            }
            return string.Empty;
        }
    }
}