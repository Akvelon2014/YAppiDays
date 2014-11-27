using System;
using System.ComponentModel.DataAnnotations;
using MobileConference.Enums;

namespace MobileConference.Models
{
    public class CommentModel
    {
        [Required(ErrorMessage = "Введите текст сообщения")]
        [Display(Name = "Сообщение")]
        public string Message { get; set; }

        public int LinkId { get; set; }

        public string UserLogin { get; set; }

        public DateTime CreationDate { get; set; }

        public CommentModelType Type { get; set; }
        public bool IsDeleted { get; set; }
        public int Id { get; set; }

        public ProfileModel User
        {
            get { return ProfileModel.GetByLogin(UserLogin); }
        }

        public IdeasModel Idea
        {
            get
            {
                if (Type != CommentModelType.Chat && Type != CommentModelType.Official) return null;
                return IdeasModel.GetById(LinkId);
            }
        }
    }
}