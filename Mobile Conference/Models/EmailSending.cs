using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MobileConference.Enums;

namespace MobileConference.Models
{
    public class EmailSending
    {
        [Required(ErrorMessage = "Введите заголовок")]
        [Display(Name = "Заголовок")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Введите текст сообщения")]
        [Display(Name = "Сообщение")]
        [AllowHtml]
        public string Message { get; set; }

        [Display(Name = "Студенты")]
        public bool IncludeStudent { get; set; }

        [Display(Name = "Менторы")]
        public bool IncludeMentor { get; set; }

        [Display(Name = "Эксперты")]
        public bool IncludeExpert { get; set; }

        [Display(Name = "Инфопартнеры")]
        public bool IncludeInfoPartner { get; set; }

        [Display(Name = "Спонсоры")]
        public bool IncludeSponsor { get; set; }

        [Display(Name = "Администраторы")]
        public bool IncludeAdmin { get; set; }


        public RoleSet GetRoleSet()
        {
            var roles = RoleSet.None;
            if (IncludeAdmin) roles |= RoleSet.Administrator;
            if (IncludeExpert) roles |= RoleSet.Expert;
            if (IncludeInfoPartner) roles |= RoleSet.InfoPartner;
            if (IncludeMentor) roles |= RoleSet.Mentor;
            if (IncludeSponsor) roles |= RoleSet.Sponsor;
            if (IncludeStudent) roles |= RoleSet.Student;
            return roles;
        }

        public void SetFromRoles(RoleSet roles)
        {
            IncludeExpert = roles.IsRoleInSet(RoleName.Expert);
            IncludeAdmin = roles.IsRoleInSet(RoleName.Administrator);
            IncludeMentor = roles.IsRoleInSet(RoleName.Mentor);
            IncludeSponsor = roles.IsRoleInSet(RoleName.Sponsor);
            IncludeStudent = roles.IsRoleInSet(RoleName.Student);
            IncludeInfoPartner = roles.IsRoleInSet(RoleName.InfoPartner);
        }
    }
}