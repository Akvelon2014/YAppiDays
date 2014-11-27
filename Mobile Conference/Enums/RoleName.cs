using System;
using MobileConference.GlobalData;

namespace MobileConference.Enums
{
    public enum RoleName
    {
        Guest = 0,
        Administrator = 1,
        Student = 2,
        Mentor = 3,
        Sponsor = 4,
        Expert = 5,
        InfoPartner = 7
    }

    public static class ExtensionForRoleName
    {
        public static string GetName(this RoleName role)
        {
            return Enum.GetName(typeof (RoleName), role);
        }


        public static string GetInRussian(this RoleName role)
        {
            switch (role)
            {
                case RoleName.Administrator:
                    return "Администратор";
                case RoleName.Student:
                    return "Студент";
                case RoleName.Mentor:
                    return "Ментор";
                case RoleName.Sponsor:
                    return "Спонсор";
                case RoleName.Expert:
                    return "Эксперт";
                case RoleName.InfoPartner:
                    return "Инфо партнер";
            }
            return GlobalValuesAndStrings.Guest;
        }

        public static RoleName GetRole(this string roleAsString)
        {
            RoleName role;
            if (Enum.TryParse(roleAsString, out role)) return role;
            return RoleName.Guest;
        }

        public static string GetInRussian(this RoleName? role)
        {
            return (role!=null)?    ((RoleName)role).GetInRussian()
                                    :GlobalValuesAndStrings.Guest;
        }
    }
}