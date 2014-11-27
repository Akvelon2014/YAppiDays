using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Managers;

namespace MobileConference.Models
{
    public class AwardModel
    {
        private static readonly IEventRepository eventRepository;

        static AwardModel()
        {
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();
        }

        [Required(ErrorMessage = "Введите название награды")]
        [Display(Name = "Сообщение")]
        public string Title { get; set; }

        public string Subtitle { get; set; }
        public string PostTitle { get; set; }
        public string Description { get; set; }

        public int? PictureId { get; set; }
        public PictureNameModel PictureModel { get; set; }

        public int Id { get; set; }
        public int EventId { get; set; }

        [ValidInteger(ErrorMessage = "Укажите год поступления в формате из четырех цифр")]
        [Required(ErrorMessage = "Введите порядок награды")]
        public int? OrderInList { get; set; }
        public bool IsDeleted { get; set; }


        public static AwardModel GetById(int awardId)
        {
            AwardModel model;
            if (CacheManager.TryReadAward(awardId, out model)) return model;
            model = eventRepository.GetAwardById(awardId);

            CacheManager.UpdateAward(awardId, model);
            return model;
        }

        public static List<AwardModel> GetForEvent(int eventId)
        {
            List<int> list;
            if (CacheManager.TryReadAwardList(eventId, out list)) return list.Select(GetById).ToList();
            list = eventRepository.GetAwardsForEvent(eventId);
            CacheManager.UpdateAwardList(eventId, list);
            return list.Select(GetById).ToList();
        }

        /// <summary>
        /// Using only in repositories
        /// </summary>
        public static void SinchronizeWithCache(int awardId)
        {
            AwardModel model = eventRepository.GetAwardById(awardId);
            CacheManager.UpdateAward(awardId, model);
            List<int> list = eventRepository.GetAwardsForEvent(model.EventId);
            CacheManager.UpdateAwardList(model.EventId, list);
        }
    }
}