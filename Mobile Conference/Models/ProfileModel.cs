using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages;
using Castle.Core.Internal;
using MobileConference.CustomAttribute;
using MobileConference.Enums;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Interface;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using MobileConference.Managers;

namespace MobileConference.Models
{
    public class ProfileModel
    {
        private static readonly IUserRepository userRepository;

        static ProfileModel()
        {
            userRepository = ContainerDI.Container.Resolve<IUserRepository>();
        }

        public int Id { get; set; }
        public string Login { get; set; }
        public RoleName Role { get; set; }
        public string RoleInRussian { get; set; }
        public bool IsDeleted { get; set; }
        public int? Avatar { get; set; }
        public bool EmailConfirmation { get; set; }
        public int Tips { get; set; }
        public string Email { get; set; }

        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Введите ваше имя")]
        [RegularExpression(@"^[a-zA-ZА-Яа-я]{1}[a-zA-ZА-Яа-я`'\s]{0,}$", ErrorMessage = "Имя должно состоять из букв русского или латинского алфавита (может включать апостроф и пробел)")]
        [StringLength(50, ErrorMessage = "Пожалуйста укажите сокращенный вариант вашего имени (не более 50 символов)")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        [Required(ErrorMessage = "Введите вашу фамилию")]
        [RegularExpression(@"^[a-zA-ZА-Яа-я]{1}[a-zA-ZА-Яа-я`'\s]{0,}$", ErrorMessage = "Фамилия должна состоять из букв русского или латинского алфавита (может включать апостроф и пробел)")]
        [StringLength(70, ErrorMessage = "Пожалуйста укажите сокращенный вариант вашей фамилии (не более 70 символов)")]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        [RegularExpression(@"^[a-zA-ZА-Яа-я]{1}[a-zA-ZА-Яа-я`'\s]{0,}$", ErrorMessage = "Отчество должно состоять из букв русского или латинского алфавита (может включать апостроф и пробел)")]
        [StringLength(70, ErrorMessage = "Пожалуйста укажите сокращенный вариант вашего отчества (не более 70 символов)")]
        public string SecondName { get; set; }

        [Display(Name = "День рождения")]
        [DataType(DataType.Date)]
        [DateFormat]
        public string BirthDate { get; set; }

        [Display(Name="О себе")]
        public string Description { get; set; }
        
        [Display(Name = "Город")]
        [StringLength(100, ErrorMessage = "Пожалуйста укажите верное название города (не более 100 символов)")]
        public string City { get; set; }

        //int? used, because DropDownList don't work with enum correct
        [Display(Name = "Желаемая роль")]
        public int? WishedRoleAsInt
        {
            get { return (int?)WishedRole; }
            set { WishedRole = (RoleName?)value; }
        }

        [Display(Name = "Желаемая роль")]
        public RoleName? WishedRole { get; set; }

        [Display(Name = "Компания")]
        public int? Company { get; set; }

        [Display(Name = "Место работы")]
        [StringLength(150, ErrorMessage = "Укажите сокращенный вариант названия вашей работы (не более 150 символов)")]
        public string PlaceJob { get; set; }

        [Display(Name = "Университет")]
        [StringLength(100, ErrorMessage = "Укажите сокращенный вариант названия вашего учебного заведения (не более 100 символов)")]
        public string University { get; set; }

        [Display(Name = "Год поступления")]
        [ValidInteger(ErrorMessage = "Укажите год поступления в формате из четырех цифр")]
        [Range(1940, 2020, ErrorMessage = "Год поступления должен быть в разумных пределах")]
        public short? YearBeginning { get; set; }

        [Display(Name = "Год окончания")]
        [ValidInteger(ErrorMessage = "Укажите год поступления в формате из четырех цифр")]
        [Range(1940, 2025, ErrorMessage = "Год окончания должен быть в разумных пределах")]
        public short? YearGraduating { get; set; }

        [Display(Name = "Факультет")]
        [StringLength(150, ErrorMessage = "Пожалуйста укажите сокращенный вариант названия вашего факультета (не более 150 символов)")]
        public string Faculty { get; set; }


        private Dictionary<StudentStatusInGroup, List<int>> ideasDictionary = 
                                                new Dictionary<StudentStatusInGroup, List<int>>();
        private Dictionary<int, List<int>> platformForEventDictionary = new Dictionary<int, List<int>>();
 
        public DateTime? DateActivity { get; set; }

        public bool IsAllowedToCreateNewIdeas
        {
            get { return !(WishedIdeas.Any() || MemberIdeas.Any()); }
        }


        public string FirstNameSecondName
        {
            get
            {
                if (FirstName.IsNullOrEmpty())
                {
                    return SecondName.IsNullOrEmpty() ? string.Empty : SecondName;
                }
                return SecondName.IsNullOrEmpty() ? FirstName : string.Format("{0} {1}",FirstName, SecondName);                
            }
        }


        public CompanyModel CompanyModel
        {
            get
            {
                if (Company == null) return null;
                return CompanyModel.GetById((int)Company);
            }
        }

        public List<IdeasModel> InvitedIdeas
        {
            get
            {
                if (!ideasDictionary.ContainsKey(StudentStatusInGroup.Invited) ||
                    !ideasDictionary[StudentStatusInGroup.Invited].Any()) return new List<IdeasModel>();
                return ideasDictionary[StudentStatusInGroup.Invited].Select(IdeasModel.GetById).ToList();
            }
        }


        public List<IdeasModel> WishedIdeas
        {
            get
            {
                if (!ideasDictionary.ContainsKey(StudentStatusInGroup.Wished) ||
                    !ideasDictionary[StudentStatusInGroup.Wished].Any()) return new List<IdeasModel>();
                return ideasDictionary[StudentStatusInGroup.Wished].Select(IdeasModel.GetById).ToList();
            }
        }


        public List<IdeasModel> MemberIdeas
        {
            get
            {
                if (!ideasDictionary.ContainsKey(StudentStatusInGroup.Member) ||
                    !ideasDictionary[StudentStatusInGroup.Member].Any()) return new List<IdeasModel>();
                return ideasDictionary[StudentStatusInGroup.Member].Select(IdeasModel.GetById).ToList();
            }
        }


        /// <summary>
        /// Key - event id, Value - platform id
        /// </summary>
        public Dictionary<int, List<int>> PlatformToEventDictionary
        {
            get { return platformForEventDictionary; }
        }

        public int? PlatformForExpertId
        {
            get
            {
                if (!platformForEventDictionary.Any()) return null;
                int? currentIndex = -1;
                if (platformForEventDictionary.ContainsKey(EventModel.Current.Id) && platformForEventDictionary
                    [EventModel.Current.Id].Any()) currentIndex = platformForEventDictionary[EventModel.Current.Id].First();
                if (currentIndex < 0) currentIndex = platformForEventDictionary.First().Value.First();
                return currentIndex;
            }
            set
            {
                if (value == null) return;
                AddEventAndPlatform(EventModel.Current.Id,(int)value);
            }
        }

        public string EmailConfirmData
        {
            get { return Id.ToString() + Email + Login; }
        }

        public static ProfileModel Current
        {
            get
            {
                var membershipUser = Membership.GetUser();
                if (membershipUser == null) return null;
                string login = membershipUser.UserName;
                return GetByLogin(login);
            }
        }
        

        public List<string> SkillNames
        {
            get { return userRepository.GetAllSkills(Login); }
        }


        public static ProfileModel GetByLogin(string login, bool withDeleted = false)
        {
            ProfileModel profile;
            if (CacheManager.TryReadUserProfileModel(login, out profile)) return profile;

            profile = userRepository.GetProfileModel(login, withDeleted);
            CacheManager.UpdateUserProfileModel(login, profile);
            return profile;
        }


        /// <summary>
        /// Using only in repositories
        /// </summary>
        public void AddIdeaForUser(int ideaId, StudentStatusInGroup status)
        {
            if (!ideasDictionary.ContainsKey(status))
            {
                ideasDictionary.Add(status, new List<int>());
            }
            ideasDictionary[status].Add(ideaId);
        }


        /// <summary>
        /// Using only in repositories
        /// </summary>
        public void AddEventAndPlatform(int eventId, int platformId)
        {
            if (!platformForEventDictionary.ContainsKey(eventId))
            {
                platformForEventDictionary.Add(eventId, new List<int>());
            }
            platformForEventDictionary[EventModel.Current.Id].Add(platformId);
        }

        /// <summary>
        /// Using only in repositories
        /// </summary>
        public static void SinchronizeWithCache(string login)
        {
            ProfileModel userProfile;
            //if (!CacheManager.TryReadUserProfileModel(login, out userProfile)) return;
            userProfile = userRepository.GetProfileModel(login);
            CacheManager.UpdateUserProfileModel(login, userProfile);
        }
    }
} 