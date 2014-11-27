using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MobileConference.Enums;
using MobileConference.Helper;

namespace MobileConference.Models
{
    public class NewsProxy:EmailSending
    {
        [Required(ErrorMessage = "Введите дату начала")]
        [Display(Name = "Дата/время начала")]
        public string DateFrom { get; set; }

        [Required(ErrorMessage = "Введите дату окончания")]
        [Display(Name = "Дата/время окончания")]
        public string DateTo { get; set; }

        public NewsModel ToNews()
        {
            return new NewsModel()
            {
                DateFrom = DateFrom.ConvertToDateTime(),
                RoleFor = GetRoleSet(),
                DateTo = DateTo.ConvertToDateTime(),
                Description = Message,
                Title = Title
            };
        }

        public static NewsProxy GetByNews(NewsModel news)
        {
            var proxy = new NewsProxy
            {
                DateFrom = news.DateFrom.ConvertToDateTimeString(),
                DateTo = news.DateTo.ConvertToDateTimeString(),
                Message = news.Description,
                Title = news.Title
            };
            proxy.SetFromRoles(news.RoleFor);
            return proxy;
        }
    }
}