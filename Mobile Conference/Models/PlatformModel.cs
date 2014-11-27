using System.ComponentModel.DataAnnotations;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Managers;

namespace MobileConference.Models
{
    public class PlatformModel
    {
        private static readonly IEventRepository eventRepository;

        static PlatformModel()
        {
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }
        public int? PictureId { get; set; }
        public bool IsDeleted { get; set; }


        public static PlatformModel ForPlatform(int platformId)
        {
            PlatformModel model;
            if (CacheManager.TryReadPlatform(platformId, out model)) return model;

            model = eventRepository.GetPlatformById(platformId);

            CacheManager.UpdatePlatform(platformId, model);
            return model;
        }


        /// <summary>
        /// Using only in repositories
        /// </summary>
        public static void SinchronizeWithCache(int platformId)
        {
            PlatformModel model;
            //if (!CacheManager.TryReadPlatform(platformId, out model)) return;
            model = eventRepository.GetPlatformById(platformId);
            CacheManager.UpdatePlatform(platformId, model);
        }
    }
}