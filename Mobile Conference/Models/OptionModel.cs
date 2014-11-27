using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;
using MobileConference.GlobalData;
using MobileConference.Interface;

namespace MobileConference.Models
{
    public class OptionModel
    {
        private static IEventRepository eventRepository;
        private static OptionModel option = null;

        [Required(ErrorMessage = "Введите значение количество элементов на странице")]
        [Display(Name = "Количество элементов на странице")]
        public  int ItemPerPage { get;  set; }

        [Required(ErrorMessage = "Выберите текущее событие")]
        [Display(Name = "Текущее событие")]
        public  int CurrentEventId { get;  set; }

        [Display(Name = "Запретить регистрацию проектов")]
        public bool ForbiddenProjectRegistration { get; set; }

        [Display(Name = "Безвозвратно удалять изображения")]
        public bool IsPhisicalDelete { get; set; }

        static OptionModel()
        {
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();  
        }

        public OptionModel()
        {
            IsPhisicalDelete = bool.Parse(ConfigurationManager.AppSettings["PhisicalDelete"]);
        }

        public void Refresh()
        {
            ItemPerPage = int.Parse(eventRepository.GetOption(OptionNames.ItemPerPage));
            CurrentEventId = int.Parse(eventRepository.GetOption(OptionNames.CurrentEvent));
            ForbiddenProjectRegistration = bool.Parse(eventRepository.GetOption(OptionNames.ForbiddenProjectRegistration));
        }


        /// <summary>
        /// Update database
        /// </summary>
        public void Update()
        {
            eventRepository.SetOption(OptionNames.ItemPerPage, ItemPerPage.ToString());
            eventRepository.SetOption(OptionNames.CurrentEvent, CurrentEventId.ToString());
            eventRepository.SetOption(OptionNames.ForbiddenProjectRegistration, ForbiddenProjectRegistration.ToString());
            Refresh();
        }


        public static OptionModel Current
        {
            get
            {
                if (option == null)
                {
                    option = new OptionModel();
                    option.Refresh();
                }
                return option;
            }
        }


        public static string FullyQualifiedApplicationPath
        {
            get
            {
                //Return variable declaration
                var appPath = string.Empty;

                //Getting the current context of HTTP request
                var context = HttpContext.Current;

                //Checking the current context content
                if (context != null)
                {
                    //Formatting the fully qualified website url/name
                    appPath = string.Format("{0}://{1}{2}{3}",
                                            context.Request.Url.Scheme,
                                            context.Request.Url.Host,
                                            context.Request.Url.Port == 80
                                                ? string.Empty
                                                : ":" + context.Request.Url.Port,
                                            context.Request.ApplicationPath);
                }
                if (appPath.EndsWith("/"))
                    appPath = appPath.Remove(appPath.Count() - 1);

                return appPath;
            }
        }
    }
}