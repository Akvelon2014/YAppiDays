using System;
using System.Linq;
using System.Web.UI.WebControls.Expressions;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Models;
using System.Web.Mvc;
using PagedList;

namespace MobileConference.Controllers
{
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        private IEventRepository eventRepository;
        private IUserRepository userRepository;
        private IIdeasRepository ideaRepository;
        private IImageRepository imageRepository;
        private IEmailManager emailManager;

        public HomeController(IEventRepository eventRepository, IUserRepository userRepository, IEmailManager emailManager,
            IIdeasRepository ideaRepository, IImageRepository imageRepository)
        {
            this.eventRepository = eventRepository;
            this.userRepository = userRepository;
            this.emailManager = emailManager;
            this.ideaRepository = ideaRepository;
            this.imageRepository = imageRepository;
        }

        public ActionResult Index()
        {
            ViewBag.hasProjects = ideaRepository.GetAllIdeas().Any();
            ViewBag.hasMaterials = eventRepository.GetMaterials().Any();
            ViewBag.hasExperts = userRepository.GetExpertsToStartPage(GlobalValuesAndStrings.ExpertCountOnStartPage).Any();
            var curEvent = EventModel.Current;
            ViewBag.caption = (curEvent != null) ? curEvent.Title : GlobalValuesAndStrings.DefaultWelcome;
            ViewBag.description = (curEvent != null) ? curEvent.Description : GlobalValuesAndStrings.DefaultDescription;
            if (User.Identity.IsAuthenticated && ProfileModel.Current != null)
            {
                userRepository.UpdateDateUserLogin(ProfileModel.Current.Login);
            }
            return View();
        }


        [Authorize]
        public ActionResult Ideas(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ViewBag.page = page;
            return View();
        }


        [Authorize]
        public ActionResult Events(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ViewBag.page = page;
            return View();
        }


        [Authorize]
        public ActionResult EventDescribe(int eventId)
        {
            var eventModel = EventModel.GetById(eventId);
            ViewBag.image = imageRepository.GetEventAvatar(eventId);
            if (eventModel == null || eventModel.IsDeleted ||
                eventModel.Level != GlobalValuesAndStrings.NewsLevelInEvent)
            {
                eventModel = null;
            }
            else
            {
                DateTime dateFrom = eventModel.DateFrom.ConvertToDate().Value;
                DateTime dateTo = eventModel.DateTo.ConvertToDate().Value;
                ViewBag.dateFrom = dateFrom.CustomMonthTitle();
                ViewBag.dateTo = dateTo.CustomMonthTitle();
                ViewBag.isOneDate = (dateFrom.Month == dateTo.Month && dateFrom.Day == dateTo.Day);
            }
            return View(eventModel);
        }


        /// <summary>
        /// User profile to show for other users
        /// </summary>
        [Authorize]
        public ActionResult UserDescription(string login)
        {
            ProfileModel user = ProfileModel.GetByLogin(login);
            return View(user);
        }

        [Authorize]
        public ActionResult Materials(string search = "")
        {
            ViewBag.search = search;
            return View();
        }

        public ActionResult Experts(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            return View(page);
        }

        public ActionResult Stages(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ViewBag.page = page;
            return View();
        }

        public ActionResult StagesPartial(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var stages = EventModel.Current.GetCurrentChild(EventModel.Current.ChildIds.Count()).OrderBy(s=>s.Order)
                .ToPagedList(page, OptionModel.Current.ItemPerPage);
            ViewBag.page = page;
            return PartialView(stages);
        }

        public ActionResult Awards()
        {
            var awards = eventRepository.GetAwardsForEvent(EventModel.Current.Id).Select(AwardModel.GetById);
            return View(awards);
        }

        [Authorize]
        public ActionResult ShowVideo(string link, int materialId)
        {
            ViewBag.link = string.Format("//www.youtube.com/embed/{0}?rel=0", link);
            var material = MaterialModel.ForMaterial(materialId);
            return View(material);
        }

        [Authorize]
        public ActionResult ShowMaterial(int materialId)
        {
            var material = MaterialModel.ForMaterial(materialId);
            return View(material);
        }

        #region About and Error
        public ActionResult About()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(FeedbackModel model)
        {
            if (ModelState.IsValid)
            {
                emailManager.SendMessageToAdmin(model);
                return View("FeedbackSuccedSending");
            }
            return View(model);
        }


        public ActionResult Error()
        {
            return View("Error");
        }


        public ActionResult NotFoundError()
        {
            return View();
        }

        public ActionResult RegistrationForbidden()
        {
            if (!OptionModel.Current.ForbiddenProjectRegistration)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        #endregion

    }
}
