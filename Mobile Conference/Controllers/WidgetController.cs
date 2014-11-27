using System.Web.UI.WebControls.Expressions;
using Castle.Core.Internal;
using MobileConference.Enums;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobileConference.Controllers
{
    public class WidgetController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IEventRepository eventRepository;
        private readonly IIdeasRepository ideasRepository;
        private readonly IImageRepository imageRepository;
        private readonly IImageManager imageManager;
        private readonly ICapchaManager capchaManager;

        public WidgetController(IUserRepository userRepository, IEventRepository eventRepository,
            IIdeasRepository ideasRepository, IImageRepository imageRepository, IImageManager imageManager,
            ICapchaManager capchaManager)
        {
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
            this.ideasRepository = ideasRepository;
            this.imageRepository = imageRepository;
            this.imageManager = imageManager;
            this.capchaManager = capchaManager;
        }


        public PartialViewResult AccountInfo()
        {
            ViewBag.WithAvatar = false;
            ViewBag.IsAuth = false;
            if (User == null || !User.Identity.IsAuthenticated)
            {
                ViewBag.IsAuth = false;
                return PartialView();
            }
            var user = ProfileModel.Current;
            ViewBag.FirstName = user.FirstName??string.Empty;
            ViewBag.LastName = user.LastName??string.Empty;
            ViewBag.IsAuth = true;
            ViewBag.Id = user.Id;
            ViewBag.WithAvatar = user.Avatar != null;
            if (user.FirstName.IsNullOrEmpty() && user.LastName.IsNullOrEmpty())
            {
                ViewBag.FirstName = user.Login;
            }

            return PartialView(user);
        }

        #region Widget for start page

        [ChildActionOnly]
        public PartialViewResult Adwards()
        {
            var currentEvent = EventModel.Current;
            List<AwardModel> adwards = null;
            if (currentEvent != null)
            {
                adwards = AwardModel.GetForEvent(EventModel.Current.Id);                
            }
            return PartialView(adwards);
        }

        [ChildActionOnly]
        public PartialViewResult RecentStage()
        {
            var stages = new List<EventModelWithOrder>();
            var currentEvent = EventModel.Current;
            if (currentEvent != null)
            {
                stages = currentEvent.GetCurrentChild();
            }
            return PartialView(stages);            
        }


        public PartialViewResult ExpertList()
        {
            List<ProfileModel> experts = userRepository.GetExpertsToStartPage(GlobalValuesAndStrings.ExpertCountOnStartPage);
            return PartialView(experts);
        }


        public PartialViewResult RecentProject(int? platformId = null, int? groupIdeaId = null)
        {
            var ideas = ideasRepository.GetAllIdeas(GlobalValuesAndStrings.FirstPageCount,
                platform: platformId, ideasGroup: groupIdeaId, take: GlobalValuesAndStrings.ProjectCountOnStartPage, remix: true)
                .Take(GlobalValuesAndStrings.ProjectCountOnStartPage).AsQueryable().ToModel();
            
            var projects = ideas.Select(idea => idea.GetIdeaWithPlatform(platformId)).ToList();
            return PartialView(projects);
        }


        public PartialViewResult RecentMaterial(int? materialPlatformId = null)
        {
            var materials = eventRepository.GetMaterials(materialPlatformId).OrderBy(x => Guid.NewGuid())
                .Take(GlobalValuesAndStrings.MaterialCountOnStartPage).AsEnumerable().ToModel();
            return PartialView(materials);
        }


        [ChildActionOnly]
        public PartialViewResult SponsorList()
        {
            var sponsors = userRepository.GetSponsorIds().Select(CompanyModel.GetById).ToList();
            return PartialView(sponsors);
        }


        [ChildActionOnly]
        public PartialViewResult InfoPartnerList()
        {
            var infoPartners = userRepository.GetInfoPartnerIds().Select(CompanyModel.GetById).ToList();
            return PartialView(infoPartners);
        }

        #endregion //Widget for start page

        #region Widget for Materials,Projects, Experts and Events Pages


        [Authorize]
        public PartialViewResult MaterialsWithPlatformDescription(int? materialPlatformId = null, string search = "")
        {
            PlatformModel model = materialPlatformId == null ? null : PlatformModel.ForPlatform((int)materialPlatformId);
            ViewBag.search = search;
            return PartialView(model);
        }


        [Authorize]
        public PartialViewResult AllMaterials(int? materialPlatformId = null, int page = GlobalValuesAndStrings.FirstPageCount,
            string search = "")
        {
            var materials = eventRepository.GetMaterials(materialPlatformId, null,search)
                .OrderByDescending(m => m.AddedDate).ToPagedList(page, OptionModel.Current.ItemPerPage);
            ViewBag.materialPlatformId = materialPlatformId;
            ViewBag.search = search;
            return PartialView(materials);
        }


        public PartialViewResult AllProject(int? platformId = null, int? groupIdeaId = null,
            int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            List<int> currentUserProjects = null;
            int emptyProjectAtTheBegining = 0;
            if (User.Identity.IsAuthenticated)
            {
                var user = ProfileModel.Current;
                if (user != null)
                {
                    if (user.Role == RoleName.Student)
                    {
                        if (user.MemberIdeas.Any())
                        {
                            currentUserProjects = user.MemberIdeas.Select(i=>i.Id).ToList();
                        }
                        else
                        {
                            if (!OptionModel.Current.ForbiddenProjectRegistration)
                            {
                                emptyProjectAtTheBegining = GlobalValuesAndStrings.ProjectsPerOneStudentCount;
                            }
                        }
                    }
                    
                    if (user.Role == RoleName.Mentor)
                    {
                        if (user.MemberIdeas.Any())
                        {
                            currentUserProjects = user.MemberIdeas.Select(i => i.Id).ToList();
                        }
                    }
                }
            }
            var ideas = ideasRepository.GetAllIdeas(page, platform: platformId, ideasGroup: groupIdeaId, search: search,
                topIds: currentUserProjects, withEmptyProjects: emptyProjectAtTheBegining);
            ViewBag.platformId = platformId;
            ViewBag.groupIdeaId = groupIdeaId;
            ViewBag.search = search;
            ViewBag.emptyProject = emptyProjectAtTheBegining;
            return PartialView(ideas);
        }

        /// <summary>
        /// Get previous, next or all events
        /// </summary>
        public PartialViewResult RecentEvents(EventListType categoriesEvent = EventListType.All,
            int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var events = eventRepository.GetNeededLevelEvents(EventModel.Current.Id, GlobalValuesAndStrings.NewsLevelInEvent,
                categoriesEvent, false, page);
            ViewBag.categories = categoriesEvent;
            return PartialView(events);
        }

        public ActionResult Experts(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var experts = userRepository.GetAllUsers(page, string.Empty, false, RoleName.Expert);
            return PartialView(experts);
        }


        [ForbiddenElse("Administrator", "InfoPartner", "Expert", "Sponsor")]
        public ActionResult SelectCompanyPartial(int page = GlobalValuesAndStrings.FirstPageCount, string search = "", string login = null)
        {
            var user = ProfileModel.Current;
            if (user == null) return PartialView(null);
            bool withHiddenCompanies = (user.Role == RoleName.Administrator);
            var companies = userRepository.GetAllCompanies(search, withHiddenCompanies).OrderByDescending(c => c.IsShowed)
                .ThenBy(c=>c.Rank==null).ThenBy(c=>c.Rank).ToPagedList(page, OptionModel.Current.ItemPerPage);
            ViewBag.page = page;
            ViewBag.search = search;
            ViewBag.login = login;
            return PartialView(companies);
        }

        public ActionResult EventForStagePartial(int stageId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ViewBag.page = page;
            ViewBag.stageId = stageId;
            var events = eventRepository.GetEventForStage(stageId, page);
            return PartialView(events);            
        }

        public ActionResult ShowEventForStagePartial(int stageId, int childrenCount, int page = GlobalValuesAndStrings.FirstPageCount,
            bool isShowing = false)
        {
            ViewBag.page = page;
            ViewBag.stageId = stageId;
            ViewBag.childrenCount = childrenCount;
            ViewBag.isShowing = isShowing;
            return PartialView();
        }

        #endregion //Widget for Materials,Projects, Experts and Events Pages

        #region Widget for Ideas

        [ChildActionOnly]
        public PartialViewResult DescribeIdea(IdeasModel idea, string userLogin, bool showAsMain = false, bool displayOnly = false)
        {
            var ideaWithPermission = idea.GetWithPermissionFor(userLogin);
            ideaWithPermission.DisplayAsMain = showAsMain;
            ideaWithPermission.DisplayOnly = displayOnly;
            return PartialView(ideaWithPermission);            
        }


        [ChildActionOnly]
        public PartialViewResult DescribeInvitedUser(IdeasModel idea, ProfileModel user, int page, string search, bool fromRequest = false)
        {
            if (idea.WishedToJoinUsers.FirstOrDefault(i => i.Login == user.Login) != null) ViewBag.status = "wished";
            if (idea.MemberUsers.FirstOrDefault(i => i.Login == user.Login) != null) ViewBag.status = "member";
            if (idea.InvitedUsers.FirstOrDefault(i => i.Login == user.Login) != null) ViewBag.status = "invited";
            ViewBag.idea = idea.Id;
            ViewBag.page = page;
            ViewBag.search = search;
            ViewBag.fromRequest = fromRequest;
            return PartialView(user);
        }


        /// <summary>
        /// Get users list for leadership pass
        /// </summary>
        /// <param name="idea"></param>
        /// <returns></returns>
        [ChildActionOnly]
        public PartialViewResult ListGroupMember(IdeasModel idea)
        {
            var users = idea.MemberUsers;
            if (idea.MentorProfile != null)
            {
                ViewBag.mentorLogin = idea.MentorProfile.Login;
                ViewBag.mentorName = idea.MentorProfile.FullName();
            }
            var leader = idea.LeaderProfile;
            if (leader != null)
            {
                ViewBag.leaderLogin = leader.Login;
                ViewBag.leaderName = leader.FullName();
            }
            return PartialView(users);
        }


       [ForbiddenElse("Student")]
        public ActionResult UsersForInviting(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            ProfileModel profile = ProfileModel.Current;
            if (profile.MemberIdeas.FirstOrDefault(idea => idea.Id == ideaId) == null)
                return PartialView();
            ViewBag.idea = IdeasModel.GetById(ideaId);
            ViewBag.search = search;
            ViewBag.page = page;
            var users = userRepository.GetAllUsers(page, search, false, RoleName.Student);
            return PartialView(users);
        }

        [Authorize]
        public ActionResult MentorsIdeas(int page = GlobalValuesAndStrings.FirstPageCount, ProfileModel profile = null)
        {
            if (profile == null)
            {
                return View();
            }
            List<IdeasModel> ideas = profile.MemberIdeas;
            if (ideas == null)
            {
                return View();
            }
            var ideasWithPaging = ideas.OrderBy(idea => idea.CreatedDate).ToPagedList(page, OptionModel.Current.ItemPerPage);
            return PartialView(ideasWithPaging);
        }

        [Authorize]
        public ActionResult IdeasWithoutMentor(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var ideas = ideasRepository.IdeasWithoutMentor(page);
            if (ideas == null) return View();
            return PartialView(ideas);
        }

        #endregion //Widget for Ideas

        #region Work with comments

        [ChildActionOnly]
        public PartialViewResult DescribeComment(CommentModel comment, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var user = ProfileModel.Current;
            ViewBag.canRemove = false;
            if (user.Role == RoleName.Administrator || user.Login == comment.UserLogin) ViewBag.canRemove = true;
            List<PictureNameModel> photos = imageRepository.GetImagesFor(comment.Id, PictureType.Comment);
            ViewBag.photo = photos.FirstOrDefault();
            ViewBag.userId = user.Id;
            ViewBag.page = page;
            ViewBag.addClass = (comment.Type == CommentModelType.Chat || comment.Type == CommentModelType.Official)
                ? "bottomBorder"
                : "";
            return PartialView(comment);
        }


        public PartialViewResult ShowAllComments(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount,
            CommentModelType commentType = CommentModelType.Chat)
        {
            var ideaWithPermission = IdeasModel.GetById(ideaId).GetWithPermissionFor(ProfileModel.Current.Login);
            if (!ideaWithPermission.Permission.HasFlag(PermissionType.ShowChat)) return PartialView(null);
            IPagedList<Comment> comments = ideasRepository.GetCommentsForLink(ideaId, commentType, page);
            ViewBag.page = page;
            if (comments.PageCount>0 && comments.PageCount < comments.PageNumber)
            {
                comments = ideasRepository.GetCommentsForLink(ideaId, commentType, comments.PageCount);
                ViewBag.page = comments.PageCount;
            }
            ViewBag.ideaId = ideaId;
            ViewBag.commentType = commentType;
            return PartialView(comments);
        }


        public ActionResult RemoveComment(int linkId, int commentId, int page = GlobalValuesAndStrings.FirstPageCount,
            CommentModelType commentType = CommentModelType.Chat)
        {
            var user = ProfileModel.Current;
            ideasRepository.RemoveComment(commentId, user.Login);
            if (commentType == CommentModelType.Material)
            {
                return RedirectToAction("MaterialComments", "Widget", new { materialId = linkId, page, IsShowing = true});                
            }
            return RedirectToAction("ShowAllComments", "Widget", new { ideaId = linkId, page, commentType });
        }

        #endregion //Work with comments

        #region Work with material comments

        [Authorize]
        public ActionResult MaterialComments(int materialId, bool IsShowing = false, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            MaterialModel material = MaterialModel.ForMaterial(materialId);
            ViewBag.IsShowing = IsShowing;
            ViewBag.page = page;
            ViewBag.materialId = materialId;
            return PartialView(material);
        }


        [Authorize]
        public ActionResult AddMaterialComment(CommentModel comment, int materialId)
        {
            if (!comment.Message.Trim().IsNullOrEmpty())
            {
                comment.LinkId = materialId;
                comment.Type = CommentModelType.Material;
                comment.CreationDate = DateTime.Now;
                comment.UserLogin = ProfileModel.Current.Login;
                ideasRepository.AddComment(comment);
            }
            return RedirectToAction("MaterialComments", "Widget", new {materialId, IsShowing = true});
        }


        [Authorize]
        public ActionResult ShowMaterialComments(int materialId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            MaterialModel material = MaterialModel.ForMaterial(materialId);
            if (material == null) return PartialView(null);
            ViewBag.materialId = materialId;
            ViewBag.page = page;
            var comments = material.GetComments(page);
            if (comments.PageCount > 0 && comments.PageCount < comments.PageNumber)
            {
                comments = material.GetComments(comments.PageCount);
                ViewBag.page = comments.PageCount;
            }
            return PartialView(comments);
        }

        #endregion //Work with material comments

        #region Work with picture, capcha

        [HttpPost]
        public ActionResult LoadImage(HttpPostedFileBase picture, string altImage = null, bool isForUser = false)
        {
            var fileName = string.Empty;
            if (ModelState.IsValid && picture!=null && picture.ContentLength>0)
            {
                PictureNameModel result = imageManager.Save(picture);
                imageRepository.SavePicture(result, true);
                fileName = (imageManager.GetImageName(result, isForUser));
            }
            if (altImage.IsNullOrEmpty()) fileName = "Images/Std.jpg";
            ViewBag.altImage = fileName;
            return PartialView();
        }


        [HttpPost]
        public ActionResult RotateImage(string picture, bool isForUser = false)
        {
            var fileName = picture;
            PictureNameModel result = imageManager.Rotate(picture);
            if (result != null)
            {
                imageRepository.SavePicture(result, true);
                fileName = (imageManager.GetImageName(result, isForUser));
            }
            ViewBag.fileName = fileName;
            return PartialView();
        }


        public ActionResult InsertPhotoData(HttpPostedFileBase loadedPhoto)
        {
            PictureNameModel result = imageManager.Save(loadedPhoto);
            if (result != null)
            {
                imageRepository.SavePicture(result);
                ViewBag.image = imageManager.GetImageName(result);
                return PartialView(result);
            }
            return null;
        }


        public ActionResult CapchaImage()
        {
            string answer;
            var mem = capchaManager.GetCapcha(GlobalValuesAndStrings.CapchaWidth, GlobalValuesAndStrings.CapchaHeight,
                out answer);
            Session[GlobalValuesAndStrings.CapchaSessionName] = answer;
            var img = File(mem.GetBuffer(), "image/Jpeg");
            return img;
        }


        //Used to cropping image for Idea, when picture added
        public ActionResult AddPicturePartial(int ideaId)
        {
            ProfileModel profile = ProfileModel.Current;
            var idea = IdeasModel.GetById(ideaId).GetWithPermissionFor(profile.Login);
            return PartialView(idea);
        }

        //Photogallery for Idea
        public ActionResult ShowIdeaPhotos(int ideaId, bool error = false)
        {
            ProfileModel profile = ProfileModel.Current;
            var idea = IdeasModel.GetById(ideaId).GetWithPermissionFor(profile.Login);
            List<PictureNameModel> pictures = null;
            if (idea.Permission.HasFlag(PermissionType.ShowPhotos))
            {
                pictures = imageRepository.GetIdeaPictures(ideaId);
            }
            ViewBag.ideaId = ideaId;
            ViewBag.idea = idea;
            ViewBag.error = error;
            return PartialView(pictures);
        }


        public ActionResult ShowPicturesForEvent(int eventId, bool withoutTitle = false)
        {
            var pictures = imageRepository.GetEventPictures(eventId);
            ViewBag.isAdmin = ProfileModel.Current!=null && ProfileModel.Current.Role == RoleName.Administrator;
            ViewBag.eventId = eventId;
            ViewBag.withoutTitle = withoutTitle;
            return PartialView(pictures);
        }

        #endregion //Work with picture, capcha

        #region Widget for Chat

        public ActionResult ChatMessageSender(int ideaId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            ProfileModel profile = ProfileModel.Current;
            var idea = IdeasModel.GetById(ideaId).GetWithPermissionFor(profile.Login);
            ViewBag.page = page;
            ViewBag.permission = idea.Permission;
            ViewBag.ideaId = ideaId;
            return PartialView();
        }


        [HttpPost]
        public ActionResult ChatMessageSender(CommentModel comment, int ideaId, string picture = null
            , int page = GlobalValuesAndStrings.FirstPageCount )
        {
            ProfileModel profile = ProfileModel.Current;
            var idea = IdeasModel.GetById(ideaId).GetWithPermissionFor(profile.Login);
            if (!idea.Permission.HasFlag(PermissionType.ShowChat))
            {
                return RedirectToAction("ChatMessageSender", new { ideaId, page });
            }
            if (!idea.Permission.HasFlag(PermissionType.UseChat))
            {
                return RedirectToAction("ChatMessageSender", new { ideaId, page });
            }
            ViewBag.page = page;
            ViewBag.ideaId = ideaId;
            ViewBag.permission = idea.Permission;
            if (comment== null || comment.Message == null || comment.Message.Trim().IsNullOrEmpty())
            {
                if (picture != null)
                {
                    comment = new CommentModel {Message = string.Empty};
                }
                else
                {
                    return RedirectToAction("ChatMessageSender", new { ideaId, page });                           
                }
            }
            comment.LinkId = ideaId;
            comment.Type = CommentModelType.Chat;
            comment.CreationDate = DateTime.Now;
            comment.UserLogin = profile.Login;
            int? commentId = ideasRepository.AddComment(comment);

            //if picture was added, assign this picture with comments
            if (picture != null && commentId != null)
            {
                var photoId = imageRepository.GetIdForUrl(picture);
                if (photoId != null)
                {
                    imageRepository.AssignPicture((int)photoId, PictureType.Comment, (int)commentId);
                }
            }
            return RedirectToAction("ChatMessageSender", new { ideaId, page });
        }


        public ActionResult AddPhotoToMessageInChat()
        {
            return PartialView();
        }

        #endregion //Widget for Chat

        #region Get JSON data for autocomplete

        public ActionResult GetCities()
        {
            return Json(eventRepository.GetCurrentCitiesNames(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetSkills()
        {
            return Json(userRepository.GetAllSkills().ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion //Get JSON data for autocomplete

    }
}
