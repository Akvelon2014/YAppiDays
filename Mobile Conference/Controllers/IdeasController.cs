using System.Web;
using Castle.Core.Internal;
using MobileConference.Enums;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MobileConference.Controllers
{
    [InitializeSimpleMembership]
    [Authorize]
    public class IdeasController : Controller
    {
        private readonly IIdeasRepository ideasRepository;
        private readonly IEventRepository eventRepository;
        private readonly IUserRepository userRepository;
        private readonly IImageRepository imageRepository;
        private readonly IImageManager imageManager;

        public IdeasController(IIdeasRepository ideasRepository, IEventRepository eventRepository,
            IUserRepository userRepository, IImageRepository imageRepository, IImageManager imageManager)
        {
            this.ideasRepository = ideasRepository;
            this.eventRepository = eventRepository;
            this.userRepository = userRepository;
            this.imageRepository = imageRepository;
            this.imageManager = imageManager;
        }

        #region Actions from account popup

        [ForbiddenElse("Student")]
        public ActionResult IndexFromPopup()
        {
            if (Request.IsAjaxRequest()) return Json(new { data = GlobalValuesAndStrings.EmptyAnswerInAction }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Index", "Ideas");
        }


        [ForbiddenElse("Mentor")]
        public ActionResult MentorPageFromPopup()
        {
            if (Request.IsAjaxRequest()) return Json(new { data = GlobalValuesAndStrings.EmptyAnswerInAction }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("MentorPage", "Ideas");
        }


        [ForbiddenElse("Mentor")]
        public ActionResult IdeasWithoutMentorFromPopup()
        {
            if (Request.IsAjaxRequest()) return Json(new { data = GlobalValuesAndStrings.EmptyAnswerInAction }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("IdeasWithoutMentor", "Ideas");
        }

        #endregion //Actions from account popup

        #region Main pages for student and mentor and other users

        [Authorize]
        public ActionResult MainPage(int ideaId = GlobalValuesAndStrings.OwnIdea)
        {
            IdeasModel model;
            ProfileModel profile = ProfileModel.Current;
            if (ideaId == GlobalValuesAndStrings.OwnIdea)
            {
                if (profile.Role != RoleName.Student) return RedirectToAction("Index", "Home");
                if (!profile.MemberIdeas.Any()) return RedirectToAction("Index", "Home");
                model = profile.MemberIdeas.First();
            }
            else
            {
                model = IdeasModel.GetById(ideaId);
            }
            if (model == null) return RedirectToAction("Index", "Home");
            return View(model.GetWithPermissionFor(profile.Login));
        }


        [ForbiddenElse("Student")]
        public ActionResult Index(int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            ViewBag.search = search;
            ProfileModel profile = ProfileModel.GetByLogin(User.Identity.Name);

            //if user haven't own idea or wished idea
            if (!profile.MemberIdeas.Any() && !profile.WishedIdeas.Any() && !profile.InvitedIdeas.Any())
            {
                //user can select from all ideas
                var ideas = ideasRepository.GetAllIdeas(page, search);
                if (ideas == null) return View();
                return View(ideas);
            }

            // user have inviting to ideas and user also have wished idea or his own idea
            if (profile.InvitedIdeas.Any())
            {
                return View("InvitingForUser", profile);
            }

            // user have wished idea
            if (profile.WishedIdeas.Any()) return View("WishedIdeaForStudent",profile.WishedIdeas.First().Id);

            // user just have own idea
            return RedirectToAction("MainPage");
        }

        [Authorize]
        public ActionResult MentorPage(int page = GlobalValuesAndStrings.FirstPageCount, string mentorLogin = null)
        {
            ProfileModel profile = (mentorLogin == null) ? ProfileModel.Current : ProfileModel.GetByLogin(mentorLogin);
            if (profile == null) return RedirectToAction("Index", "Home");
            ViewBag.self = mentorLogin == null;
            ViewBag.page = page;
            ViewBag.profile = profile;
            return View();
        }


        [ForbiddenElse("Mentor")]
        public ActionResult IdeasWithoutMentor(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ViewBag.page = page;
            return View();
        }

        #endregion //Main pages for student and mentor and other users

        #region Create and Change Ideas

        [HttpGet]
        [ProjectForbiddenPossible]
        [ForbiddenElse("Student")]
        public ActionResult CreateNew()
        {
            var user = ProfileModel.Current;
            if (user == null) return RedirectToAction("Index");
            if (!user.IsAllowedToCreateNewIdeas) return RedirectToAction("Index");
            return View();
        }


        [HttpPost]
        [ForbiddenElse("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNew(IdeasModel idea)
        {
            var user = ProfileModel.Current;
            if (user == null) return RedirectToAction("Index");
            if (!user.IsAllowedToCreateNewIdeas) return RedirectToAction("Index");
            if (ModelState.IsValid)
            {
                if (idea.GroupIdea!=null && !eventRepository.IsCorrectIdForGroupIdeas((int) idea.GroupIdea))
                {
                    ModelState.AddModelError(GlobalValuesAndStrings.IdeaGroupError, GlobalValuesAndStrings.UnknownIdeaGroup);
                    return View(idea);
                }
                int eventId = OptionModel.Current.CurrentEventId;
                ideasRepository.AddIdeaToDb(idea.Title, idea.Description, idea.GroupIdea, eventId, user.Login);
                return RedirectToAction("MainPage", "Ideas");
            }
            return View(idea);
        }


        [ForbiddenElse("Student")]
        public ActionResult TechnologySection(int ideaId)
        {
            ProfileModel profile = ProfileModel.Current;
            var idea = IdeasModel.GetById(ideaId).GetWithPermissionFor(profile.Login);
            return PartialView(idea);
        }


        [HttpGet]
        [ForbiddenElse("Student")]
        public ActionResult IdeaProfile(int ideaId)
        {
            ProfileModel profile = ProfileModel.Current;
            IdeasModel idea = IdeasModel.GetById(ideaId);
            if (!idea.GetWithPermissionFor(profile.Login).Permission.HasFlag(PermissionType.ChangeProfile))
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            return View(idea);
        }


        [HttpPost]
        [ForbiddenElse("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult IdeaProfile(IdeasModel model)
        {
            if (!UserHasChangeInIdea(model.Id))
                return RedirectToAction("MainPage", "Ideas", new { model.Id });
            if (ModelState.IsValid)
            {
                ideasRepository.UpdateIdea(model);
                return RedirectToAction("MainPage", "Ideas", new { model.Id });                
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult ChangeAvatar(int ideaId)
        {
            if (!UserHasChangeInIdea(ideaId))
                return new EmptyResult();
            var idea = IdeasModel.GetById(ideaId);
            return PartialView(idea);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAvatar(int ideaId, string picture, ImageSizeParams cropSize = null)
        {
            if (!UserHasChangeInIdea(ideaId))
                return new EmptyResult();
            if (picture != null && cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var avatar = imageManager.Save(picture, cropSize);
                    imageRepository.SetIdeaAvatar(avatar, ideaId);
                }
            }
            var idea = IdeasModel.GetById(ideaId);
            return PartialView(idea);
        }


        [ForbiddenElse("Student")]
        public ActionResult AddPlatformToIdea(int ideaId, int platformId)
        {
            if (!UserHasChangeInIdea(ideaId))
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            ideasRepository.AddPlatformToIdea(ideaId, platformId);
            return RedirectToAction("TechnologySection", "Ideas", new { ideaId });
        }


        [ForbiddenElse("Student")]
        public ActionResult SavePlatformStatus(int ideaId, int platformId, StatusByRealizedPlatform status)
        {
            if (!UserHasChangeInIdea(ideaId))
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            ideasRepository.ChangePlatformStatusInIdea(ideaId, platformId, status);
            return RedirectToAction("TechnologySection", "Ideas", new { ideaId });
        }


        [ForbiddenElse("Student")]
        public ActionResult RemovePlatformFromIdea(int ideaId, int platformId)
        {
            if (!UserHasChangeInIdea(ideaId))
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            ideasRepository.RemovePlatformFromIdea(ideaId, platformId);
            return RedirectToAction("TechnologySection", "Ideas", new { ideaId });
        }


        [HttpGet]
        [ForbiddenElse("Student")]
        public ActionResult Leadership(int ideaId)
        {
            IdeasModel model = IdeasModel.GetById(ideaId);
            ProfileModel profile = ProfileModel.Current;
            if (model == null || model.LeaderProfile == null || model.LeaderProfile.Login != profile.Login)
                return RedirectToAction("Index", "Home");
            return View(model);
        }


        [HttpPost]
        [ForbiddenElse("Student")]
        [ValidateAntiForgeryToken]
        public ActionResult Leadership(int ideaId, string member)
        {
            if (member == null) return RedirectToAction("MainPage", "Ideas");
            IdeasModel model = IdeasModel.GetById(ideaId);
            ProfileModel profile = ProfileModel.Current;
            if (model == null || profile == null) return RedirectToAction("MainPage", "Ideas");
            if (model.LeaderProfile == null || model.LeaderProfile.Login != profile.Login)
                return RedirectToAction("Index", "Home");
            ideasRepository.SetLeader(ideaId, member);

            return RedirectToAction("MainPage", "Ideas");
        }

        #endregion //Create and Change Ideas

        #region Inviting and Join

        [ForbiddenElse("Student")]
        public ActionResult Inviting(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.GetByLogin(User.Identity.Name);
            if (!profile.InvitedIdeas.Any())
            {
                return RedirectToAction("Index");
            }
            return PartialView(profile.InvitedIdeas.OrderByDescending(i => i.CreatedDate)
                    .ToPagedList(page, OptionModel.Current.ItemPerPage));
        }


        [ForbiddenElse("Student")]
        public ActionResult InvitedUser(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            ProfileModel profile = ProfileModel.Current;
            if (profile.MemberIdeas.FirstOrDefault(idea => idea.Id == ideaId) == null)
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            ViewBag.ideaId = ideaId;
            ViewBag.search = search;
            ViewBag.page = page;
            return View();
        }


        [HttpPost]
        [ForbiddenElse("Student")]
        public ActionResult InvitedUser(string userLogin, int ideaId, int page = GlobalValuesAndStrings.FirstPageCount,
            string search = "", bool fromRequest = false)
        {
            ProfileModel profile = ProfileModel.Current;
            if (profile.MemberIdeas.FirstOrDefault(idea => idea.Id == ideaId) == null)
                return new EmptyResult();
            ideasRepository.Invite(ideaId, userLogin);
            if (fromRequest)
            {
                return RedirectToAction("RequestToJoinPartial", "Ideas", new { ideaId, page });                
            }
            return RedirectToAction("UsersForInviting", "Widget", new { ideaId, page, search });
        }


        [HttpGet]
        [ForbiddenElse("Mentor","Student")]
        public ActionResult JoinIdea(int ideaId, bool withUrl = false, string returnUrl = "")
        {
            IdeasModel idea = IdeasModel.GetById(ideaId);
            var user = ProfileModel.Current;
            if (idea == null || user == null) return RedirectToAction("Index");
            if (idea.IsDeleted) return RedirectToAction("UnjoinIdea", new { ideaId });
            if (user.Role == RoleName.Mentor)
            {
                ideasRepository.SetMentor(ideaId, user.Login);
                if (withUrl)
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("IdeasWithoutMentor");
            }
            if (!user.IsAllowedToCreateNewIdeas)
            {
                //user in other idea
                if (user.WishedIdeas.Any()) ViewBag.status = GlobalValuesAndStrings.WishedStatus;
                IdeasModel joinedIdea = user.MemberIdeas.FirstOrDefault() ?? user.WishedIdeas.FirstOrDefault();
                if (joinedIdea != null) ViewBag.joinedIdea = joinedIdea;
                return View(idea);
            }
            ideasRepository.SendRequestForIdea(ideaId,user.Login);
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ForbiddenElse("Student")]
        public ActionResult JoinIdea(int ideaId, string status)
        {
            if (status != GlobalValuesAndStrings.StatusOK) return RedirectToAction("Index");
            IdeasModel idea = IdeasModel.GetById(ideaId);
            var user = ProfileModel.Current;
            if (idea == null || user==null) return RedirectToAction("Index","Home");
            if (idea.IsDeleted) return RedirectToAction("UnjoinIdea", new { ideaId });
            IdeasModel oldIdea = user.MemberIdeas.FirstOrDefault() ?? user.WishedIdeas.FirstOrDefault();
            if (oldIdea != null)
            {
                ideasRepository.RemoveMemberFromIdea(oldIdea.Id,user.Login);
                ideasRepository.SendRequestForIdea(ideaId,user.Login);
            }
            return RedirectToAction("Index");
        }


        [ForbiddenElse("Mentor","Student")]
        public ActionResult UnjoinIdea(int ideaId)
        {
            IdeasModel idea = IdeasModel.GetById(ideaId);
            var user = ProfileModel.Current;
            if (idea == null || user == null) return RedirectToAction("Index");
            if (user.Role == RoleName.Mentor)
            {
                ideasRepository.UnsetMentor(ideaId, user.Login);
                return RedirectToAction("MentorPage");
            }
            ideasRepository.RemoveMemberFromIdea(ideaId,user.Login);
            return RedirectToAction("Index");
        }


        [ForbiddenElse("Student")]
        public ActionResult DeclineUserFromWished(int ideaId, string userLogin, int page, bool fromRequest = false, string search = "")
        {
            ProfileModel profile = ProfileModel.Current;
            ViewBag.ideaId = ideaId;
            if (profile.MemberIdeas.FirstOrDefault(idea => idea.Id == ideaId) == null)
                return new EmptyResult();
            ideasRepository.RemoveMemberFromIdea(ideaId, userLogin);
            if (fromRequest)
            {
                return RedirectToAction("RequestToJoinPartial", "Ideas", new { ideaId, page });                
            }
            return RedirectToAction("UsersForInviting", "Widget", new { ideaId, page, search });
        }


        [ForbiddenElse("Student")]
        public ActionResult RequestToJoin(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.Current;
            if (profile.MemberIdeas.FirstOrDefault(i => i.Id == ideaId) == null)
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            ViewBag.page = page;
            ViewBag.ideaId = ideaId;
            return View();
        }

        [ForbiddenElse("Student")]
        public ActionResult RequestToJoinPartial(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.Current;
            if (profile.MemberIdeas.FirstOrDefault(i => i.Id == ideaId) == null)
                return PartialView();
            IdeasModel idea = IdeasModel.GetById(ideaId);
            ViewBag.page = page;
            ViewBag.idea = idea;
            List<ProfileModel> students = idea.WishedToJoinUsers;
            if (!students.Any()) return PartialView();
            return PartialView(students.OrderBy(user => user.DateActivity).ToPagedList(page, OptionModel.Current.ItemPerPage));
        }

        #endregion // Inviting and Join

        #region Photogallery

        [HttpGet]
        public ActionResult Photogallery(int ideaId)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return RedirectToAction("Index", "Home");
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.ShowPhotos))
            {
                return RedirectToAction("MainPage", "Ideas", new {ideaId });
            }
            ViewBag.ideaId = ideaId;
            ViewBag.idea = idea;
            return View();
        }


        [HttpPost]
        public ActionResult PhotogalleryPartial(int ideaId, HttpPostedFileBase picture)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return new EmptyResult();
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.ShowPhotos))
            {
                return new EmptyResult();
            }
            ViewBag.ideaId = ideaId;
            ViewBag.idea = idea;
            if (!idea.Permission.HasFlag(PermissionType.AddPhotos))
            {
                return RedirectToAction("ShowIdeaPhotos", "Widget", new{ideaId});
            }

            var ideaPictureIsNotSaved = false;
            //uploading
            if (picture != null)
            {
                PictureNameModel file = imageManager.PhotoLoad(picture);
                if (file != null)
                {
                    imageRepository.SaveIdeaPicture(file, ideaId);
                }
                else
                {
                    ideaPictureIsNotSaved = true;
                }
            }

            return RedirectToAction("ShowIdeaPhotos", "Widget", new { ideaId, error = ideaPictureIsNotSaved });
        }


        public ActionResult RemoveImageFromPhototgallery(string url, int ideaId)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return RedirectToAction("Index", "Home");
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.DeletePhotos))
            {
                return RedirectToAction("ShowIdeaPhotos", "Widget", new { ideaId });
            }
            var pictureId = imageRepository.GetIdForUrl(url);
            if (pictureId != null)
            {
                imageRepository.DeleteIdeaPicture((int)pictureId, ideaId);
            }
            return RedirectToAction("ShowIdeaPhotos", "Widget", new { ideaId});
        }

        #endregion // Photogallery

        #region Chat and Official Report

        public ActionResult Chat(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return RedirectToAction("Index", "Home");
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.ShowChat))
            {
                return RedirectToAction("MainPage", "Ideas", new {ideaId });
            }
            ViewBag.page = page;
            ViewBag.permission = idea.Permission;
            ViewBag.ideaId = ideaId;
            return View();
        }


        [HttpPost]
        public ActionResult Chat(CommentModel comment, int ideaId, string store = null, 
            int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return RedirectToAction("Index", "Home");
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.ShowChat))
            {
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            }
            if (!idea.Permission.HasFlag(PermissionType.UseChat))
            {
                return RedirectToAction("Chat", "Ideas", new {ideaId, page });
            }
            ViewBag.page = page;
            ViewBag.ideaId = ideaId;
            ViewBag.permission = idea.Permission;
            if (!ModelState.IsValid)
            {
                return View(comment);
            }
            comment.LinkId = ideaId;
            comment.Type = CommentModelType.Chat;
            comment.CreationDate = DateTime.Now;
            comment.UserLogin = profile.Login;
            int? commentId = ideasRepository.AddComment(comment);

            //if picture was added, assign this picture with comments
            if (store != null && commentId!=null)
            {
                int? imageId = imageRepository.GetIdForUrl(store);
                if (imageId != null)
                {
                    imageRepository.AssignPicture((int)imageId,PictureType.Comment,(int)commentId);
                }
            }
            return RedirectToAction("Chat", "Ideas", new {ideaId, page });
        }


        public ActionResult ChatPhotoSection(int ideaId)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return new EmptyResult();
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.UseChat))
            {
                return new EmptyResult();
            }
            ViewBag.ideaId = ideaId;
            ViewBag.idea = idea;
            ViewBag.pictureName = null;
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddChatPhoto(int ideaId, HttpPostedFileBase picture)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return new EmptyResult();
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.UseChat))
            {
                return new EmptyResult();
            }
            ViewBag.ideaId = ideaId;
            ViewBag.idea = idea;
            ViewBag.pictureName = null;
            //uploading
            if (picture != null)
            {
                PictureNameModel file = imageManager.PhotoLoad(picture);
                if (file != null)
                {
                    imageRepository.SavePicture(file, true);
                    ViewBag.pictureName = imageManager.GetImageName(file);
                    ViewBag.pictureModel = file;
                }
                else
                {
                    return new EmptyResult();
                }
            }
            return PartialView("ChatPhotoSection");
        }


        public ActionResult OfficialReports(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return RedirectToAction("Index", "Home");
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.ShowOfficialReports))
            {
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            }
            ViewBag.page = page;
            ViewBag.permission = idea.Permission;
            ViewBag.ideaId = ideaId;
            return View();
        }


        [HttpPost]
        public ActionResult OfficialReports(CommentModel comment, int ideaId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.Current;
            var ideaModel = IdeasModel.GetById(ideaId);
            if (ideaModel == null) return RedirectToAction("Index", "Home");
            var idea = ideaModel.GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.ShowOfficialReports))
            {
                return RedirectToAction("MainPage", "Ideas", new { ideaId });
            }
            if (!idea.Permission.HasFlag(PermissionType.AsMember))
            {
                return RedirectToAction("OfficialReports", "Ideas", new { ideaId, page });
            }
            ViewBag.page = page;
            ViewBag.ideaId = ideaId;
            ViewBag.permission = idea.Permission;
            if (!ModelState.IsValid || comment.Message.Trim().IsNullOrEmpty())
            {
                return View(comment);
            }
            comment.LinkId = ideaId;
            comment.Type = CommentModelType.Official;
            comment.CreationDate = DateTime.Now;
            comment.UserLogin = profile.Login;
            ideasRepository.AddComment(comment);

            return RedirectToAction("OfficialReports", "Ideas", new { ideaId, page });
        }


        public JsonResult GetLastUpdateForComment(int ideaId)
        {
            return Json(ideasRepository.GetLastUpdateForIdeaComments(ideaId, CommentModelType.Chat), JsonRequestBehavior.AllowGet);
        }

        #endregion //Chat and Official Report

        #region Helper

        private bool UserHasChangeInIdea(int ideaId)
        {
            ProfileModel profile = ProfileModel.Current;
            IdeasModel idea = IdeasModel.GetById(ideaId);
            
            if (idea != null && profile != null)
            {
                bool canChangeProfile = idea.GetWithPermissionFor(profile.Login).Permission.
                    HasFlag(PermissionType.ChangeProfile);
                return canChangeProfile;
            }
            return false;
        }

        #endregion //Helper
    } 
}

