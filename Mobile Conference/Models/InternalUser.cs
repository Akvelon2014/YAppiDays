using System.ComponentModel.DataAnnotations;
using MobileConference.Enums;

namespace MobileConference.Models
{
    public class InternalUser
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        [RegularExpression(@"^[a-zA-Z]{1}[a-zA-Z0-9_-]{0,19}$", ErrorMessage = "Логин должен начинаться с латинской буквы и может содержать только цифры и знаки -_")]
        [StringLength(20, ErrorMessage = "Логин должен содержать от 3 до 20 символов", MinimumLength = 3)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        //int? used, because DropDownList don't work with enum correct
        [Display(Name = "Роль")]
        public int? WishedRoleAsInt
        {
            get { return (int?)Role; }
            set { Role = (RoleName?)value; }
        }

        [Display(Name = "Роль")]
        public RoleName? Role { get; set; }
    }
}