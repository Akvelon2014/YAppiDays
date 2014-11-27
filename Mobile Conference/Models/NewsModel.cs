using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Managers;

namespace MobileConference.Models
{
    public class NewsModel
    {
        private static readonly IEventRepository eventRepository;

        static NewsModel()
        {
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();
        }

        [Required(ErrorMessage = "Введите название")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Введите описание")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        public int Id { get; set; }
        public int EventId { get; set; }
        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = "Введите дату начала")]
        [Display(Name = "Дата начала")]
        public DateTime? DateFrom { get; set; }

        [Required(ErrorMessage = "Введите дату окончания")]
        [Display(Name = "Дата окончания")]
        public DateTime? DateTo{ get; set; }

        public RoleSet RoleFor { get; set; }

        public static NewsModel ForNews(int id)
        {
            return eventRepository.GetNewsById(id).ToModel();
        }
    }

}