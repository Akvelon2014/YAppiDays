using System.ComponentModel.DataAnnotations;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Managers;

namespace MobileConference.Models
{
    public class CompanyModel
    {
        private static readonly IUserRepository userRepository;

        static CompanyModel()
        {
            userRepository = ContainerDI.Container.Resolve<IUserRepository>();
        }

        [Required(ErrorMessage = "Введите название")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Сайт")]
        [Required(ErrorMessage = "Введите официальный сайт компании")]
        public string Site { get; set; }

        public int? Id { get; set; }
        public int? Avatar { get; set; }
        public int? CreatorId { get; set; }

        [Display(Name="Внутр. рейтинг")]
        public int? Rank { get; set; }

        [Display(Name = "Показывать в списке")]
        public bool IsShowed { get; set; }

        public static CompanyModel GetById(int companyId)
        {
            CompanyModel model;
            if (CacheManager.TryReadCompany(companyId, out model)) return model;

            model = userRepository.GetCompanyById(companyId);

            CacheManager.UpdateCompany(companyId, model);
            return model;
        }


        /// <summary>
        /// Using only in repositories
        /// </summary>
        public static void SinchronizeWithCache(int companyId)
        {
            CompanyModel model;
            //if (!CacheManager.TryReadCompany(companyId, out model)) return;
            model = userRepository.GetCompanyById(companyId);
            CacheManager.UpdateCompany(companyId, model);
        }
    }
}