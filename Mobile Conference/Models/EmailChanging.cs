using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MobileConference.Models
{
    public class EmailChanging
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Введите электронную почту")]
        [StringLength(100, ErrorMessage = "Укажите почту размером не более 100 символов")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Электронная почта")]
        [RegularExpression(@"^([\w.-]+)@([\w-]+)((.(\w){2,3})+)$", ErrorMessage = "Неверный формат электронной почты")]
        public string Email { get; set; }
    }
}