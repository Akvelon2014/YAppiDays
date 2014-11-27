using System.Collections.Generic;
using MobileConference.Enums;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace MobileConference.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext() : base("DefaultConnection") { }

        public DbSet<UserProfile> UserProfiles { get; set; }
    }

    public class RegisterExternalLoginModel
    {
        
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        [RegularExpression(@"^[a-zA-Z]{1}[a-zA-Z0-9_-]{0,19}$", ErrorMessage = "Логин должен начинаться с латинской буквы и может содержать только цифры и знаки -_")]
        [StringLength(20, ErrorMessage = "Логин должен содержать от 3 до 20 символов", MinimumLength = 3)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        [RegularExpression(@"^[a-zA-ZА-Яа-я]{1}[a-zA-ZА-Яа-я`'\s]{0,}$", ErrorMessage = "Имя должно состоять из букв русского или латинского алфавита (может включать апостроф и пробел)")]
        [StringLength(50, ErrorMessage = "Укажите сокращенный вариант вашего имени (не более 50 символов)")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        [RegularExpression(@"^[a-zA-ZА-Яа-я]{1}[a-zA-ZА-Яа-я`'\s]{0,}$", ErrorMessage = "Фамилия должна состоять из букв русского или латинского алфавита (может включать апостроф и пробел)")]
        [StringLength(70, ErrorMessage = "Укажите сокращенный вариант вашей фамилии (не более 70 символов)")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Введите адрес электронной почты")]
        [StringLength(100, ErrorMessage = "Укажите почту размером не более 100 символов")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Электронная почта")]
        [RegularExpression(@"^([\w.-]+)@([\w-]+)((.(\w){2,3})+)$", ErrorMessage = "Неверный формат электронной почты")]
        public string Email { get; set; }

        public string ExternalLoginData { get; set; }

        public IDictionary<string, string> ExtraData
        {
            set
            {
                if (value != null && value.ContainsKey("photo"))
                {
                    PictureLink = value["photo"];
                }
                if (value != null && value.ContainsKey("firstName"))
                {
                    FirstName = value["firstName"];
                }
                if (value != null && value.ContainsKey("lastName"))
                {
                    LastName = value["lastName"];
                }
            }
        }
        public string PictureLink { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required(ErrorMessage = "Введите текущий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен состоять минимум из 6 символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Повторите пароль")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и подтверждение не совпадают")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [RegularExpression(@"^[a-zA-Z]{1}[a-zA-Z0-9_-]{0,19}$", ErrorMessage = "Логин должен начинаться с латинской буквы и может содержать только цифры и знаки -_")]
        [StringLength(20, ErrorMessage = "Логин должен содержать от 3 до 20 символов", MinimumLength = 3)]
        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        [RegularExpression(@"^[a-zA-ZА-Яа-я]{1}[a-zA-ZА-Яа-я`'\s]{0,}$", ErrorMessage = "Имя должно состоять из букв русского или латинского алфавита (может включать апостроф и пробел)")]
        [StringLength(50, ErrorMessage = "Укажите сокращенный вариант вашего имени (не более 50 символов)")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Введите фамилию")]
        [Display(Name = "Фамилия")]
        [RegularExpression(@"^[a-zA-ZА-Яа-я]{1}[a-zA-ZА-Яа-я`'\s]{0,}$", ErrorMessage = "Фамилия должна состоять из букв русского или латинского алфавита (может включать апостроф и пробел)")]
        [StringLength(70, ErrorMessage = "Укажите сокращенный вариант вашей фамилии (не более 70 символов)")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Введите электронную почту")]
        [StringLength(100, ErrorMessage = "Укажите почту размером не более 100 символов")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Электронная почта")]
        [RegularExpression(@"^([\w.-]+)@([\w-]+)((.(\w){2,3})+)$", ErrorMessage = "Неверный формат электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен состоять как минимум из 6 символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Повторите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Повторите пароль")]
        [Compare("Password", ErrorMessage = "Пароль и подтверждение не совпадают")]
        public string ConfirmPassword { get; set; }

       //int? used, because DropDownList don't work with enum correct
        [Display(Name = "Желаемая роль")]
        public int? WishedRoleAsInt
        {
            get { return (int?)WishedRole; }
            set { WishedRole = (RoleName?)value; }
        }

        [Display(Name = "Желаемая роль")]
        public RoleName? WishedRole { get; set; }

        [Required(ErrorMessage = "Решите пример на картинке")]
        [Display(Name = "Решение примера")]
        public string Capcha { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
