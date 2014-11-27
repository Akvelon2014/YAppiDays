using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MobileConference.GlobalData;

namespace MobileConference.Enums
{
    [Flags]
    public enum RoleSet
    {
        None = 0,
        Administrator = 1,
        Student = 2,
        Mentor = 4,
        Sponsor = 8,
        Expert = 16,
        InfoPartner = 32
    }

    public static class ExtensionForRoleSet
    {
        public static bool IsRoleInSet(this RoleSet roleSet, RoleName role)
        {
            switch (role)
            {
                case RoleName.Administrator:
                    return roleSet.HasFlag(RoleSet.Administrator);
                case RoleName.Student:
                    return roleSet.HasFlag(RoleSet.Student);
                case RoleName.Mentor:
                    return roleSet.HasFlag(RoleSet.Mentor);
                case RoleName.Sponsor:
                    return roleSet.HasFlag(RoleSet.Sponsor);
                case RoleName.Expert:
                    return roleSet.HasFlag(RoleSet.Expert);
                case RoleName.InfoPartner:
                    return roleSet.HasFlag(RoleSet.InfoPartner);
            }
            return false;
        }

        public static void SetRoleInSet(this RoleSet roleSet, RoleName role)
        {
            switch (role)
            {
                case RoleName.Administrator:
                    roleSet = (roleSet | RoleSet.Administrator);
                    break;
                case RoleName.Student:
                    roleSet = (roleSet | RoleSet.Student);
                    break;
                case RoleName.Mentor:
                    roleSet = (roleSet | RoleSet.Mentor);
                    break;
                case RoleName.Sponsor:
                    roleSet = (roleSet | RoleSet.Sponsor);
                    break;
                case RoleName.Expert:
                    roleSet = (roleSet | RoleSet.Expert);
                    break;
                case RoleName.InfoPartner:
                    roleSet = (roleSet | RoleSet.InfoPartner);
                    break;
            }
        }

        public static List<RoleName> GetRoleList(this RoleSet roleSet)
        {
            return ((RoleName[]) Enum.GetValues(typeof (RoleName))).Where(role => role != RoleName.Guest)
                .Where(role => roleSet.IsRoleInSet(role)).ToList();
        }
    }
}