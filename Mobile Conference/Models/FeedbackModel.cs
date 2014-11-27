using System.ComponentModel.DataAnnotations;

namespace MobileConference.Models
{
    public class FeedbackModel
    {
        [Required(ErrorMessage = "Введите ваше имя")]
        [Display(Name="Ваше имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите ваш email ")]
        [Display(Name = "Ваш Email")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^([\w.-]+)@([\w-]+)((.(\w){2,3})+)$", ErrorMessage = "Неверный формат электронной почты")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите ваше сообщение")]
        [Display(Name = "Ваше сообщение")]
        public string Message { get; set; }


        public string MessageForSending
        {
            get { return string.Format("Name: {0} <br>Email: <a href='mailto:{1}'>{1}</a> <br> Message: {2}", Name, Email, Message); }
        }
    }
}