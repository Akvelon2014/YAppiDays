using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.Services.Protocols;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.WebPages;
using Castle.Core.Internal;
using MobileConference.Enums;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Models;
using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace MobileConference.Controllers
{
    [ForbiddenElse("Administrator")]
    [InitializeSimpleMembership]
    public class AdminController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IEventRepository eventRepository;
        private readonly IIdeasRepository ideasRepository;
        private readonly IImageRepository imageRepository;
        private readonly IImageManager imageManager;
        private readonly IEmailManager emailManager;
        private readonly IDataProtectorManager dataProtectorManager;

        public AdminController(IUserRepository userRepository, IEventRepository eventRepository,
            IImageRepository imageRepository, IImageManager imageManager, IIdeasRepository ideasRepository,
            IDataProtectorManager dataProtectorManager, IEmailManager emailManager)
        {
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
            this.imageRepository = imageRepository;
            this.imageManager = imageManager;
            this.ideasRepository = ideasRepository;
            this.emailManager = emailManager;
            this.dataProtectorManager = dataProtectorManager;
        }


        public ActionResult Index()
        {
            ViewBag.wishedAdmin = userRepository.GetUsersWithRoleWished(RoleName.Administrator).Count();
            ViewBag.wishedMentor = userRepository.GetUsersWithRoleWished(RoleName.Mentor).Count();
            ViewBag.wishedSponsor= userRepository.GetUsersWithRoleWished(RoleName.Sponsor).Count();
            ViewBag.wishedExpert = userRepository.GetUsersWithRoleWished(RoleName.Expert).Count();
            ViewBag.wishedInfoPartner = userRepository.GetUsersWithRoleWished(RoleName.InfoPartner).Count();
            return View();
        }

        public ActionResult IndexPartial()
        {
            ViewBag.wishedAdmin = userRepository.GetUsersWithRoleWished(RoleName.Administrator).Count();
            ViewBag.wishedMentor = userRepository.GetUsersWithRoleWished(RoleName.Mentor).Count();
            ViewBag.wishedSponsor = userRepository.GetUsersWithRoleWished(RoleName.Sponsor).Count();
            ViewBag.wishedExpert = userRepository.GetUsersWithRoleWished(RoleName.Expert).Count();
            ViewBag.wishedInfoPartner = userRepository.GetUsersWithRoleWished(RoleName.InfoPartner).Count();
            return PartialView("AdminPopup");
        }

        #region Users Manage

        public ActionResult ManageUsers(int page = GlobalValuesAndStrings.FirstPageCount, string search = "",
            RoleName? WishedRoleAsInt = null, bool withDeleted = false)
        {
            ViewBag.search = search;
            ViewBag.role = WishedRoleAsInt;
            ViewBag.withDeleted = withDeleted;
            ViewBag.page = page;
            return View();
        }

        public ActionResult ManageUsersPartial(int page = GlobalValuesAndStrings.FirstPageCount, string search = "",
            RoleName? WishedRoleAsInt = null, bool withDeleted = false)
        {
            var users = userRepository.GetAllUsers(page, search, withDeleted, WishedRoleAsInt);
            ViewBag.search = search;
            ViewBag.role = WishedRoleAsInt;
            ViewBag.withDeleted = withDeleted;
            return PartialView(users);
        }


        public ActionResult WishedInRole(RoleName role, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ViewBag.Role = role;
            ViewBag.page = page;
            return View();
        }

        public ActionResult WishedInRolePartial(RoleName role, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            IQueryable<UserProfile> users = userRepository.GetUsersWithRoleWished(role);
            var usersWithPaging = users.OrderBy(x => x.DateActivity).ToPagedList(page, OptionModel.Current.ItemPerPage);
            ViewBag.Role = role;
            return PartialView(usersWithPaging);            
        }

        public ActionResult ResolveRole(string login, bool accept)
        {
            RoleName role = userRepository.AcceptOrDeclineUserToWishRole(login,accept);
            return RedirectToAction("WishedInRole", "Admin", new {role});
        }

        public ActionResult UserDescribe(string login)
        {
            var user = ProfileModel.GetByLogin(login, true);
            return PartialView(user);
        }

        /// <summary>
        /// Remove or restore user by admin depend on user.IsDeleted
        /// </summary>
        /// <param name="withoutRestore">if it's true, than user will deleted without sending email to restoring</param>
        public ActionResult RemoveAndRestore(string login, bool withoutRestore = false)
        {
            var user = ProfileModel.GetByLogin(login, true);
            if (user.IsDeleted)
            {
                userRepository.RestoreUser(login);
            }
            else
            {
                userRepository.RemoveUser(login, !withoutRestore);
                
            }
            return PartialView("UserBlockForAdmin", ProfileModel.GetByLogin(login, true));
        }


        public ActionResult ChangeRoleForUser(string login, int? WishedRoleAsInt)
        {
            var user = ProfileModel.GetByLogin(login, true);
            if (user == null || user.IsDeleted || WishedRoleAsInt == null) return null;
            userRepository.SetUserToRole(login, (RoleName)WishedRoleAsInt);
            return PartialView("UserBlockForAdmin", ProfileModel.GetByLogin(login));
        }

        #endregion //Users Manage

        #region User Profile Manage

        [HttpGet]
        public ActionResult CreateUserProfile()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUserProfile(InternalUser model)
        {
            if (!model.Login.IsNullOrEmpty() && userRepository.GetUserByLogin(model.Login, true) != null)
            {
                ModelState.AddModelError(GlobalValuesAndStrings.LoginError, GlobalValuesAndStrings.IncorrectUserByLogin);
            }
            if (ModelState.IsValid)
            {
                WebSecurity.CreateUserAndAccount(model.Login, model.Password, new
                {
                    Email = GlobalValuesAndStrings.EmailForInternalUser,
                    EmailConfirmation = true
                });

                if (model.Role == null || model.Role == RoleName.Guest)
                {
                    model.Role = RoleName.Student;
                }
                userRepository.SetUserToRole(model.Login, (RoleName)model.Role);

                return RedirectToAction("ChangeProfile", new{login = model.Login});
            }
            return View();
        }


        [HttpGet]
        public ActionResult ChangeProfile(string login)
        {
            ViewBag.login = login;
            ProfileModel model = ProfileModel.GetByLogin(login, true);
            if (model != null) return View(model);
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeProfile(ProfileModel profile)
        {
            ViewBag.login = profile.Login;
            var oldProfile = ProfileModel.GetByLogin(profile.Login);
            profile.Role = oldProfile.Role;

            profile.University = profile.University.NullOrTrim();
            profile.Faculty = profile.Faculty.NullOrTrim();
            profile.FirstName = profile.FirstName.NullOrTrim();
            profile.LastName = profile.LastName.NullOrTrim();
            profile.SecondName = profile.SecondName.NullOrTrim();
            profile.Email = profile.Email.NullOrTrim();
            profile.City = profile.City.NullOrTrim();
            profile.PlaceJob = profile.PlaceJob.NullOrTrim();

            if (!profile.Email.IsNullOrEmpty())
            {
                if (profile.Email != GlobalValuesAndStrings.EmailForInternalUser)
                {
                    var user = userRepository.GetUserByEmail(profile.Email, true);
                    if (user != null && user.Login != profile.Login)
                    {
                        ModelState.AddModelError(GlobalValuesAndStrings.EmailError,
                            GlobalValuesAndStrings.IncorrectUserByEmail);
                    }
                }
            }
            if ((!profile.Faculty.IsNullOrEmpty() || profile.YearBeginning != null || profile.YearGraduating != null)
                && profile.University.IsNullOrEmpty())
            {
                ModelState.AddModelError(GlobalValuesAndStrings.UniversityDataWithoutTitleError
                    , GlobalValuesAndStrings.UniversityDataWithoutTitle);
            }
            ViewBag.error = false;
            if (!ModelState.IsValid)
            {
                ViewBag.error = true;
                return View(profile);
            }
            userRepository.SetProfileData(profile);
            return View(profile);
        }

        [HttpGet]
        public ActionResult ChangeAvatar(string login)
        {
            var profile = ProfileModel.GetByLogin(login);
            return PartialView(profile);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAvatar(string picture, string login, ImageSizeParams cropSize = null)
        {
            var profile = ProfileModel.GetByLogin(login);
            if (picture != null && cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var avatar = imageManager.Save(picture, cropSize);
                    imageRepository.SaveAvatar(avatar, profile.Id);
                }
            }
            return PartialView(profile);
        }

        public ActionResult ChangeSkill(string login)
        {
            var user = ProfileModel.GetByLogin(login);
            if (user == null) return RedirectToAction("Index", "Home");
            return View(user);
        }

        public ActionResult ChangeSkillsPartial(string login, string message = null)
        {
            var user = ProfileModel.GetByLogin(login);
            if (user == null) return PartialView();
            if (message != null)
            {
                ModelState.AddModelError(string.Empty, message);
            }
            return PartialView(user);
        }

        public ActionResult AddSkill(string login, string skillName)
        {
            var user = ProfileModel.GetByLogin(login);
            if (user == null) return new EmptyResult();
            if (skillName.IsNullOrEmpty())
            {
                return RedirectToAction("ChangeSkillsPartial", new {login, message = GlobalValuesAndStrings.SkillIsEmpty });
            }
            if (skillName.Length > GlobalValuesAndStrings.SkillLength)
            {
                return RedirectToAction("ChangeSkillsPartial", new { login, message = GlobalValuesAndStrings.SkillIsTooLarge });
            }
            if (!userRepository.AddSkillToUser(user.Login, skillName))
            {
                return RedirectToAction("ChangeSkillsPartial", new { login, message = GlobalValuesAndStrings.SkillIsExists });
            }
            return RedirectToAction("ChangeSkillsPartial", new { login });
        }


        public ActionResult RemoveSkill(string login, string skillName)
        {
            var user = ProfileModel.GetByLogin(login);
            if (user == null) return new EmptyResult();
            userRepository.RemoveSkillFromUser(user.Login, skillName);
            return RedirectToAction("ChangeSkillsPartial", new{login});
        }

        public ActionResult SelectCompany(string login, int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            ViewBag.page = page;
            ViewBag.search = search;
            ViewBag.login = login;
            return View();
        }

        [HttpPost]
        public ActionResult AddToCompany(string login, int companyId, int page, string search = "")
        {
            var company = CompanyModel.GetById(companyId);
            if (company != null && login!=null)
            {
                userRepository.AddCompanyToUser(companyId, login);
                return RedirectToAction("ChangeProfile", new {login});
            }
            return RedirectToAction("SelectCompany", new { login, page, search });
        }

        #endregion

        #region Ideas Manage
        public ActionResult RemoveIdea(int ideaId, string returnUrl)
        {
            ideasRepository.RemoveIdea(ideaId);
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Ideas", "Home");
            }
        }


        public ActionResult RestoreIdea(int ideaId, string returnUrl)
        {
            ideasRepository.RestoreIdea(ideaId);
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion //Ideas Manage

        #region Locations Manage

        [HttpGet]
        public ActionResult ManageRegionAndCities(string message = null)
        {
            var regions = eventRepository.GetAllRegions();
            if (message != null)
            {
                ModelState.AddModelError(string.Empty,message);
            }
            return View(regions.Values.ToArray());
        }


        [HttpPost]
        public ActionResult AddRegion(string regionName)
        {
            if (regionName.IsNullOrEmpty())
            {
                return RedirectToAction("ManageRegionAndCities", new {message = GlobalValuesAndStrings.RegionIsEmpty});                
            }
            eventRepository.AddRegion(regionName);
            return RedirectToAction("ManageRegionAndCities");
        }


        [HttpGet]
        public ActionResult Cities(string regionName, string message = null)
        {
            var cities = eventRepository.GetCitiesForRegion(regionName);
            ViewBag.region = regionName;
            if (message != null)
            {
                ModelState.AddModelError(string.Empty, message);
            }
            return View(cities);
        }


        [HttpPost]
        public ActionResult AddCity(string regionName, string cityName)
        {
            if (regionName.IsNullOrEmpty())
            {
                return RedirectToAction("Cities", new {regionName, message = GlobalValuesAndStrings.InvalidRegion });
            }
            if (cityName.IsNullOrEmpty())
            {
                return RedirectToAction("Cities", new {regionName, message = GlobalValuesAndStrings.CityIsEmpty });
            }
            eventRepository.AddCity(regionName,cityName);
            ViewBag.region = regionName;
            return RedirectToAction("Cities", new {regionName});
        }


        public ActionResult RemoveRegion(string regionName)
        {
            eventRepository.RemoveRegion(regionName);
            return RedirectToAction("ManageRegionAndCities");
        }


        public ActionResult RemoveCity(string cityName, string regionName)
        {
            eventRepository.RemoveCityFromRegion(regionName,cityName);
            return RedirectToAction("Cities",new{regionName});
        }

        #endregion //Locations Manage

        #region Platforms Manage
        public ActionResult ManagePlatform(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ViewBag.page = page;
            return View();
        }


        public ActionResult ManagePlatformPartial(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var platforms = eventRepository.GetAllPlatform(withDeleted:true).Select(pair => pair.Value);
            ViewBag.page = page;
            return PartialView(platforms.OrderBy(pl=>pl.IsDeleted).ThenByDescending(pl => pl.Id).ToPagedList(page, OptionModel.Current.ItemPerPage));
        }


        [HttpPost]
        public ActionResult RemovePlatform(int platformId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            eventRepository.RemovePlatform(platformId);
            return RedirectToAction("ManagePlatformPartial", new { page });
        }


        [HttpPost]
        public ActionResult RestorePlatform(int platformId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            eventRepository.RestorePlatform(platformId);
            return RedirectToAction("ManagePlatformPartial", new { page});
        }


        [HttpGet]
        public ActionResult CreatePlatform()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePlatform(PlatformModel model)
        {
            if (ModelState.IsValid)
            {
                int? id = eventRepository.AddPlatform(model);
                if (id != null)
                {
                    return RedirectToAction("ChangePlatform", "Admin", new { platformId = (int)id });                           
                }
                ModelState.AddModelError(GlobalValuesAndStrings.PlatformTitleError, GlobalValuesAndStrings.IncorrectPlatformTitle);
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult ChangePlatform(int platformId)
        {
            PlatformModel model = PlatformModel.ForPlatform(platformId);
            if (model == null) RedirectToAction("ManagePlatform", "Admin");
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePlatform(PlatformModel model, string picture = null, ImageSizeParams cropSize = null)
        {
            if (ModelState.IsValid)
            {
                eventRepository.ChangePlatform(model);
                RedirectToAction("ManagePlatform", "Admin");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult ChangePlatformAvatar(int platformId)
        {
            var platform = PlatformModel.ForPlatform(platformId);
            return PartialView(platform);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePlatformAvatar(int platformId, string picture, ImageSizeParams cropSize = null)
        {
            var platform = PlatformModel.ForPlatform(platformId);
            if (picture != null && cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var avatar = imageManager.Save(picture, cropSize);
                    imageRepository.SetPlatformAvatar(avatar, platformId);
                }
            }
            return PartialView(platform);
        }

        #endregion //Platforms Manage

        #region Materials Manage

        public ActionResult ManageMaterial(int page = GlobalValuesAndStrings.FirstPageCount, string search = "", int? materialPlatformId = null)
        {
            ViewBag.page = page;
            ViewBag.platform = materialPlatformId;
            ViewBag.search = search;
            return View();
        }

        public ActionResult ManageMaterialParial(int page = GlobalValuesAndStrings.FirstPageCount, string search = "", int? materialPlatformId = null)
        {
            var materials = eventRepository.GetMaterials(search: search,platformId: materialPlatformId, withDeleted: true);
            ViewBag.page = page;
            ViewBag.search = search;
            ViewBag.platform = materialPlatformId;
            return PartialView(materials.OrderBy(mat=>mat.IsDeleted).ThenByDescending(mat => mat.Id)
                                                            .ToPagedList(page, OptionModel.Current.ItemPerPage));
        }


        [HttpPost]
        public ActionResult RemoveMaterial(int materialId, int page = GlobalValuesAndStrings.FirstPageCount
            , string search = "", int? materialPlatformId = null)
        {
            eventRepository.RemoveMaterial(materialId);
            return RedirectToAction("ManageMaterialParial", new {page, search, materialPlatformId});
        }


        [HttpPost]
        public ActionResult RestoreMaterial(int materialId, int page = GlobalValuesAndStrings.FirstPageCount
            , string search = "", int? materialPlatformId = null)
        {
            eventRepository.RestoreMaterial(materialId);
            return RedirectToAction("ManageMaterialParial", new { page, search, materialPlatformId });
        }

        [HttpGet]
        public ActionResult CreateMaterial()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMaterial(MaterialModel model, bool externalCheker = false)
        {
            if (model.Text != null)
            {
                if (ModelState.ContainsKey("Link"))
                {
                    ModelState.Remove("Link");
                }
                model.Link = GlobalValuesAndStrings.ExternalMaterialLink;
                if (model.Text.IsEmpty())
                {
                    ModelState.AddModelError(GlobalValuesAndStrings.MaterialDescriptionError
                        , GlobalValuesAndStrings.MaterialDescriptionNeeded);                    
                }
            }
            if (ModelState.IsValid)
            {
                model.AddedDate = DateTime.Now;
                int? id = eventRepository.AddMaterial(model);
                
                if (id != null)
                {
                    if (model.Text != null && model.Link == GlobalValuesAndStrings.ExternalMaterialLink)
                    {
                        ideasRepository.AddComment(new CommentModel()
                        {
                            CreationDate = DateTime.Now,
                            IsDeleted = false,
                            LinkId = (int)id,
                            Message = model.Text,
                            Type = CommentModelType.MaterialText,
                            UserLogin = ProfileModel.Current.Login
                        });
                    }

                    eventRepository.AddMaterialToEvent(OptionModel.Current.CurrentEventId, (int)id);
                    return RedirectToAction("ChangeMaterial", "Admin", new { materialId = (int)id });
                }
                ModelState.AddModelError(GlobalValuesAndStrings.MaterialAddedError, GlobalValuesAndStrings.IncorrectMaterialAdded);
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult ChangeMaterial(int materialId)
        {
            MaterialModel model = MaterialModel.ForMaterial(materialId);
            if (model == null) RedirectToAction("ManageMaterial", "Admin");
            model.Text = model.GetDescription();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMaterial(MaterialModel model)
        {
            if (ModelState.IsValidField("model"))
            {
                eventRepository.ChangeMaterial(model);
                if (model.Link == GlobalValuesAndStrings.ExternalMaterialLink)
                {
                    var commentId = model.DescriptionId;
                    if (commentId == null)
                    {
                        ideasRepository.AddComment(new CommentModel()
                        {
                            CreationDate = DateTime.Now,
                            IsDeleted = false,
                            LinkId = (int)model.Id,
                            Message = model.Text,
                            Type = CommentModelType.MaterialText,
                            UserLogin = ProfileModel.Current.Login
                        });
                    }
                    else
                    {
                        ideasRepository.UpdateComment(new CommentModel
                        {
                            IsDeleted = false,
                            Message = model.Text,
                            LinkId = model.Id,
                            Id = (int)commentId,
                            Type = CommentModelType.MaterialText
                        });
                    }
                }
                return RedirectToAction("ManageMaterial", "Admin");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult ChangeMaterialAvatar(int materialId)
        {
            var material = MaterialModel.ForMaterial(materialId);
            return PartialView(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeMaterialAvatar(int materialId, string picture, ImageSizeParams cropSize = null)
        {
            var material = MaterialModel.ForMaterial(materialId);
            if (picture != null && cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var avatar = imageManager.Save(picture, cropSize);
                    imageRepository.SetMaterialAvatar(avatar, materialId);
                }
            }
            return PartialView(material);
        }


        public ActionResult AddMaterialToEvent(int materialId, int eventId, int page)
        {
            eventRepository.AddMaterialToEvent(eventId,materialId);
            return RedirectToAction("ManageMaterial", "Admin", new {page});
        }

        #endregion //Materials Manage

        #region Idea Group Manage

        [HttpGet]
        public ActionResult ManageIdeaGroup(string message = null)
        {
            if (message != null)
            {
                ModelState.AddModelError(string.Empty, message);
            }
            var ideaGroup = eventRepository.GetAllIdeaGroups();
            ViewBag.deletedIdeaGroup = eventRepository.GetAllIdeaGroups(deletedOnly: true).Values.ToArray();
            return View(ideaGroup.Values.ToArray());
        }


        [HttpPost]
        public ActionResult AddIdeaGroup(string newIdeaGroup)
        {
            if (newIdeaGroup.IsNullOrEmpty())
            {
                return RedirectToAction("ManageIdeaGroup", new {message = GlobalValuesAndStrings.IdeaTypeIsEmpty});                
            }
            eventRepository.AddIdeaGroup(newIdeaGroup);
            return RedirectToAction("ManageIdeaGroup");
        }

        [HttpPost]
        public ActionResult RemoveIdeaGroup(string ideaGroup)
        {
            eventRepository.RemoveIdeaGroup(ideaGroup);
            return RedirectToAction("ManageIdeaGroup");
        }

        [HttpPost]
        public ActionResult RestoreIdeaGroup(string ideaGroup)
        {
            eventRepository.RestoreIdeaGroup(ideaGroup);
            return RedirectToAction("ManageIdeaGroup");
        }

        #endregion //Idea Group Manage

        #region Events Manage

        public ActionResult ManageEvents()
        {
            var events = eventRepository.GetAllEvents(onlyRoot: true);
            return View(events);
        }


        [HttpGet]
        public ActionResult CreateEvent(int? parentId = null)
        {
            if (parentId == null)
            {
                ViewBag.parent = null;
                ViewBag.parentId = null;
            }
            else
            {
                var parent = EventModel.GetById((int)parentId);
                ViewBag.parent = parent;
                ViewBag.parentId = parent.Id;
            }
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEvent(EventModel model)
        {
            ViewBag.parent = null;
            ViewBag.parentId = null;

            if (model!=null && model.DateFrom != null && model.DateTo != null)
            {
                if (model.ParentId != null)
                {
                    EventModel parent = EventModel.GetById((int) model.ParentId);
                    if (parent != null)
                    {
                        ViewBag.parent = parent;
                        ViewBag.parentId = parent.Id;
                        if (!model.IsDateInsideIn(parent))
                        {
                            ModelState.AddModelError(GlobalValuesAndStrings.DateError,
                                GlobalValuesAndStrings.IncorrectChildEventStart);
                        }
                    }
                }

                if (model.DateFrom.ConvertToDate() > model.DateTo.ConvertToDate())
                {
                    ModelState.AddModelError(GlobalValuesAndStrings.DateError, GlobalValuesAndStrings.StartAfterFinish);
                }
            }
            if (ModelState.IsValid)
            {
                int id = eventRepository.AddEvent(model);
                return RedirectToAction("EventPage", "Admin",new {eventId = id});
            }
            return View(model);
        }


        public ActionResult EventPage(int eventId, EventProfileCommand command = EventProfileCommand.None)
        {
            var eventProfile = EventModel.GetById(eventId);
            if (eventProfile == null) return RedirectToAction("ManageEvents");
            if (command == EventProfileCommand.Remove)
            {
                eventRepository.RemoveEvent(eventId);
                eventProfile = EventModel.GetById(eventId);
            }
            if (command == EventProfileCommand.Restore)
            {
                eventRepository.RestoreEvent(eventId);
                eventProfile = EventModel.GetById(eventId);
            }
            return View(eventProfile);
        }


        [HttpGet]
        public ActionResult ChangeEvent(int eventId)
        {
            var eventProfile = EventModel.GetById(eventId);
            if (eventProfile == null) return RedirectToAction("ManageEvents");
            ViewBag.parent = null;
            ViewBag.parentId = null;
            if (eventProfile.ParentId != null)
            {
                var parent = EventModel.GetById((int)eventProfile.ParentId);
                if (parent != null)
                {
                    ViewBag.parent = parent;
                    ViewBag.parentId = parent.Id;
                }
            }
            return View(eventProfile);
        }


        [HttpPost]
        public ActionResult ChangeEvent(EventModel model)
        {
            if (model == null) return RedirectToAction("ManageEvents");
            ViewBag.parent = null;
            ViewBag.parentId = null;
            var oldModel = EventModel.GetById(model.Id);
            model.ChildIds = oldModel.ChildIds;
            model.ParentId = oldModel.ParentId;

            if (model.DateFrom != null && model.DateTo != null)
            {
                if (oldModel.ParentId != null)
                {
                    var parent = EventModel.GetById((int) oldModel.ParentId);
                    if (parent != null)
                    {
                        ViewBag.parent = parent;
                        ViewBag.parentId = parent.Id;
                        if (!model.IsDateInsideIn(parent))
                        {
                            ModelState.AddModelError(GlobalValuesAndStrings.DateError,
                                GlobalValuesAndStrings.IncorrectChildEventStart);
                        }
                    }
                }

                if (model.DateFrom.ConvertToDate() > model.DateTo.ConvertToDate())
                {
                    ModelState.AddModelError(GlobalValuesAndStrings.DateError, GlobalValuesAndStrings.StartAfterFinish);
                }

                if (!model.IsIncludeAllChildren)
                {
                    ModelState.AddModelError(GlobalValuesAndStrings.DateError, GlobalValuesAndStrings.ParentMustIncludeChild);
                }
            }
            
            if (ModelState.IsValid)
            {   
                eventRepository.UpdateEventModel(model);

                return RedirectToAction("EventPage", "Admin", new { eventId = model.Id });
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult ChangeEventAvatar(int eventId)
        {
            var eventProfile = EventModel.GetById(eventId);
            return PartialView(eventProfile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeEventAvatar(int eventId, string picture, ImageSizeParams cropSize = null)
        {
            var eventProfile = EventModel.GetById(eventId);
            if (cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var avatar = imageManager.Save(picture, cropSize);
                    imageRepository.SetEventAvatar(avatar, eventId);
                }
            }
            return PartialView(eventProfile);
        }

        //Code bellow in this region about photogallery
        
        [HttpGet]
        public ActionResult EventPhotogallery(int eventId)
        {
            var eventProfile = EventModel.GetById(eventId);
            if (eventProfile == null) return RedirectToAction("ManageEvents");
            ViewBag.eventId = eventId;
            return View();

        }

        [HttpPost]
        public ActionResult EventPhotogalleryPatial(int eventId, HttpPostedFileBase picture)
        {
            var eventProfile = EventModel.GetById(eventId);
            if (eventProfile == null) return new EmptyResult();
            ViewBag.eventId = eventId;
            
            //uploading
            if (picture != null)
            {
                PictureNameModel file = imageManager.PhotoLoad(picture);
                if (file != null)
                {
                    imageRepository.SaveEventPicture(file, eventId);
                }
            }
            return RedirectToAction("ShowPicturesForEvent", "Widget", new { eventId });
        }

        public ActionResult RemoveImageFromPhototgallery(string url, int eventId)
        {

            var pictureId = imageRepository.GetIdForUrl(url);
            if (pictureId != null)
            {
                var picture = imageRepository.GetPictureById((int)pictureId);
                imageRepository.DeleteEventPicture(picture, eventId);
            }
            return RedirectToAction("ShowPicturesForEvent", "Widget", new { eventId });
        }

        #endregion //Events Manage

        #region Add location, platform and idea type to event

        public ActionResult RegionInEventPartial(int eventId)
        {
            var eventModel = EventModel.GetById(eventId);
            ViewBag.regionCount = eventRepository.GetAllRegions().Count;
            return PartialView(eventModel);
        }


        public ActionResult PlatformInEventPartial(int eventId)
        {
            var eventModel = EventModel.GetById(eventId);
            ViewBag.platformCount = eventRepository.GetAllPlatform(withDeleted: false).Count;
            return PartialView(eventModel);
        }


        public ActionResult IdeaGroupInEventPartial(int eventId)
        {
            var eventModel = EventModel.GetById(eventId);
            ViewBag.ideaGroupCount = eventRepository.GetAllIdeaGroups().Count;
            return PartialView(eventModel);
        }


        public ActionResult ManageRegionForEvent(int eventId,int regionId, bool delete = false)
        {
            if (delete)
            {
                eventRepository.RemoveRegionFromEvent(eventId, regionId);
            }
            else
            {
                eventRepository.AddRegionToEvent(eventId, regionId);
            }
            return RedirectToAction("RegionInEventPartial", "Admin", new {eventId = eventId});
        }


        public ActionResult ManagePlatformForEvent(int eventId, int platformId, bool delete = false)
        {
            if (delete)
            {
                eventRepository.RemovePlatformFromEvent(eventId, platformId);
            }
            else
            {
                eventRepository.AddPlatformToEvent(eventId, platformId);
            }
            return RedirectToAction("PlatformInEventPartial", "Admin", new { eventId });
        }


        public ActionResult ManageIdeaGroupsForEvent(int eventId, int ideasGroupId, bool delete = false)
        {
            if (delete)
            {
                eventRepository.RemoveGroupIdeaFromEvent(eventId, ideasGroupId);
            }
            else
            {
                eventRepository.AddGroupIdeaToEvent(eventId, ideasGroupId);
            }
            return RedirectToAction("IdeaGroupInEventPartial", "Admin", new { eventId });   
        }

        #endregion //Add location, platform and idea type to event

        #region Companies Manage

        public ActionResult ManageCompany(int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            ViewBag.page = page;
            ViewBag.search = search;
            return View();
        }


        [HttpGet]
        public ActionResult CreateCompany()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCompany(CompanyModel model)
        {
            if (ModelState.IsValid)
            {
                model.CreatorId = ProfileModel.Current.Id;
                int id = userRepository.AddCompany(model);
                userRepository.AddCompanyToUser(id, ProfileModel.Current.Login);
                return RedirectToAction("ChangeCompany", new { companyId = id });
            }
            return View();
        }


        [HttpGet]
        public ActionResult ChangeCompany(int companyId)
        {
            var company = CompanyModel.GetById(companyId);
            return View(company);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeCompany(CompanyModel model)
        {
            if (ModelState.IsValid)
            {
                userRepository.UpdateCompany(model);
                return RedirectToAction("ManageCompany");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeCompanyAvatar(int companyId, string picture, ImageSizeParams cropSize = null)
        {
            var company = CompanyModel.GetById(companyId);
            if (cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var avatar = imageManager.Save(picture, cropSize);
                    imageRepository.SetCompanyAvatar(avatar, companyId);
                }
            }
            return PartialView(company);
        }

        [HttpGet]
        public ActionResult ChangeCompanyAvatar(int companyId)
        {
            var company = CompanyModel.GetById(companyId);
            return PartialView(company);
        }

        #endregion //Companies Manage

        #region ManageAward

        public ActionResult ManageAward(int eventId)
        {
            var model = AwardModel.GetForEvent(eventId);
            ViewBag.eventId = eventId;
            return View(model);
        }


        [HttpGet]
        public ActionResult CreateAward(int eventId)
        {
            ViewBag.eventId = eventId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAward(AwardModel model, string picture, ImageSizeParams cropSize = null)
        {
            ViewBag.picture = picture;
            //remove check for size x1, x2, y1, y2
            if (ModelState.ClearMeasure())
            {
                ModelState.AddModelError(GlobalValuesAndStrings.SelectImageError, GlobalValuesAndStrings.SelectImage);
            } 
            else if (cropSize == null || (cropSize.Width < GlobalValuesAndStrings.MinSelectedAreaSize
                                            && cropSize.Height < GlobalValuesAndStrings.MinSelectedAreaSize))
            {
                ModelState.AddModelError(GlobalValuesAndStrings.SelectAreaForImageError, GlobalValuesAndStrings.SelectAreaForImage);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.eventId = model.EventId;
                return View(model);
            }
            var awardId = eventRepository.AddAward(model);

            if (picture != null && cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var image = imageManager.Save(picture, cropSize);
                    var id = imageRepository.SaveImagesFor(image, model.EventId, PictureType.AwardPicture);
                    eventRepository.SetPictureToAward(awardId, id);
                }
            }
            return RedirectToAction("ManageAward", "Admin", new {eventId = model.EventId});
        }


        [HttpGet]
        public ActionResult ChangeAwardAvatar(int awardId)
        {
            var award = AwardModel.GetById(awardId);
            return PartialView(award);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAwardAvatar(int awardId, string picture, ImageSizeParams cropSize = null)
        {
            var award = AwardModel.GetById(awardId);
            if (picture != null && cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var image = imageManager.Save(picture, cropSize);
                    var id = imageRepository.SaveImagesFor(image, award.EventId, PictureType.AwardPicture);
                    eventRepository.SetPictureToAward(awardId, id);
                    award = AwardModel.GetById(awardId);
                }
            }
            return PartialView(award);
        }

        [HttpGet]
        public ActionResult ChangeAward(int awardId)
        {
            var award = AwardModel.GetById(awardId);
            return View(award);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAward(AwardModel model)
        {
            //remove check for size x1, x2, y1, y2
            if (ModelState.ClearMeasure())
            {
                ModelState.AddModelError(GlobalValuesAndStrings.SelectImageError, GlobalValuesAndStrings.SelectImage);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            eventRepository.UpdateAward(model);
            return RedirectToAction("ManageAward", "Admin", new { eventId = model.EventId });
        }


        public ActionResult RemoveAward(int awardId)
        {
            var model = AwardModel.GetById(awardId);
            if (model == null) return RedirectToAction("ManageEvents");
            eventRepository.RemoveAward(awardId);
            return RedirectToAction("ManageAward", new{eventId = model.EventId});
        }

        #endregion //ManageAward

        #region EmailSending

        [HttpGet]
        public ActionResult ManageEmailSending()
        {
            return View();
        }


        [HttpPost]
        public ActionResult ManageEmailSending(EmailSending model)
        {
            if (ModelState.IsValid)
            {
                return View("SendEmailSucced", model);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult ManageEmailPartial(EmailSending model)
        {
            var roles = model.GetRoleSet().GetRoleList();
            if (roles!=null && roles.Any())
            {
                foreach (var role in roles)
                {
                    var emails = userRepository.GetEmailsForUsers(role).Select(d => d.Value)
                        .Where(m=>m!=null).ToList();
                    foreach (var email in emails)
                    {
                        if(email == GlobalValuesAndStrings.EmailForInternalUser) continue;
                        emailManager.SendMessage(email, model.Title, model.Message);
                    }
                }
            }
            return PartialView();
        }
        #endregion //EmailSending

        #region Options and Crypt Manage
        [HttpGet]
        public ActionResult ManageOption()
        {
            OptionModel model = OptionModel.Current;
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageOption(OptionModel model)
        {
            if (ModelState.IsValid)
            {
                model.Update();
                OptionModel.Current.Refresh();
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpGet]
        public ActionResult CryptText()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CryptText(string text)
        {
            string code = dataProtectorManager.Encrypt(text);
            return View("CryptText",model: code);
        }

        #endregion //Options and Crypt Manage

        #region News Manage

        public ActionResult ManageNews(string dateFrom = null, string dateTo = null, int page = GlobalValuesAndStrings.FirstPageCount,
            bool withDeleted = false)
        {
            ViewBag.dateFrom = dateFrom;
            ViewBag.dateTo = dateTo;
            ViewBag.page = page;
            ViewBag.withDeleted = withDeleted;
            return View();
        }

        public ActionResult ManageNewsPartial(string dateFrom = null, string dateTo = null, int page = GlobalValuesAndStrings.FirstPageCount,
            bool withDeleted = false)
        {
            var news = eventRepository.GetAllNews(page, dateFrom.ConvertToDate(), dateTo.ConvertToDate(), withDeleted);
            ViewBag.dateFrom = dateFrom;
            ViewBag.dateTo = dateTo;
            ViewBag.page = page;
            ViewBag.withDeleted = withDeleted;
            return PartialView("NewManagePartial", news);
        } 


        public ActionResult NewsRemoveOrRestore(int newsId, DateTime? dateFrom = null, DateTime? dateTo = null,
            int page = GlobalValuesAndStrings.FirstPageCount, bool withDeleted = false, bool remove = true)
        {
            if (remove)
            {
                eventRepository.RemoveNews(newsId);
            }
            else
            {
                eventRepository.RestoreNews(newsId);
            }
            return RedirectToAction("ManageNewsPartial", "Admin", new { dateFrom, dateTo, page, withDeleted });
        }


        [HttpGet]
        public ActionResult CreateNews()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNews(NewsProxy model)
        {
            var news = model.ToNews();
            if (ModelState.IsValid)
            {
                news.EventId = EventModel.Current.Id;
                eventRepository.AddNews(news);
                return RedirectToAction("ManageNews");
            }
            return View();
        }


        [HttpGet]
        public ActionResult ChangeNews(int newsId)
        {
            var news = NewsModel.ForNews(newsId);
            if (news == null)
            {
                return View();
            }
            var proxyNews = NewsProxy.GetByNews(news);
            ViewBag.newsId = newsId;
            return View(proxyNews);
        }

        public ActionResult ChangeNews(int newsId, NewsProxy proxyNews)
        {
            if (ModelState.IsValid)
            {
                var news = proxyNews.ToNews();
                news.Id = newsId;
                eventRepository.UpdateNews(news);
                return RedirectToAction("ManageNews");
            }
            ViewBag.newsId = newsId;
            return View(proxyNews);
        }

        #endregion // News Manage (not used now)
        

    }
}
