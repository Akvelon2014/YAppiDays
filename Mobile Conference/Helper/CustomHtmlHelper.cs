using System.Text;
using Castle.Core.Internal;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Managers;
using MobileConference.Models;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using HtmlHelper = System.Web.Mvc.HtmlHelper;

namespace MobileConference.Helper
{
    /// <summary>
    /// Extensions for HtmlHelper
    /// </summary>
    public static class CustomHtmlHelper
    {
        private static readonly IUserRepository userRepository;
        private static readonly IEventRepository eventRepository;
        private static readonly IImageManager imageManager;
        private static readonly IImageRepository imageRepository;
        private static readonly IIdeasRepository ideasRepository;
        private const string Undefined = "Неизвестно";

        static CustomHtmlHelper()
        {
            userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();
            imageManager = ContainerDI.Container.Resolve<IImageManager>();
            imageRepository = ContainerDI.Container.Resolve<IImageRepository>();
            ideasRepository = ContainerDI.Container.Resolve<IIdeasRepository>();
        }

        #region Common used utitlity

        public static string Truncate(this HtmlHelper helper, string input, int length)
        {
            if (input.IsNullOrEmpty()) return "";
            if (input.Length <= length)
            {
                return input;
            }

            return input.Substring(0, length) + "...";
        }


        public static string TruncateOneLine(this HtmlHelper helper, string input, int length)
        {
            var positionNewLine = input.IndexOf('\n');
            if (positionNewLine < 0 || positionNewLine > length)
            {
                return helper.Truncate(input, length>3?(length-2):length);
            }
            return input.Substring(0, positionNewLine) + " ...";
        }

        public static HtmlString TruncatedTitle(this HtmlHelper helper, string tag, string value, int maxLength, 
            object htmlAttributes = null, string asTitle = null)
        {
            if (value.Length > maxLength)
            {
                return new HtmlString(string.Format("<{0} title='{1}' {3}>{2}</{0}>",tag,value
                    ,asTitle??helper.Truncate(value, maxLength - 3), AttributesEncode(htmlAttributes)));
            }
            return new HtmlString(string.Format("<{0} {1}>{2}</{0}>", tag, AttributesEncode(htmlAttributes), value));
        }


        public static HtmlString Pager(this HtmlHelper helper, IPagedList list, Func<int, string> pageUrlFunc)
        {
            if (list.PageCount < 2) return new HtmlString("");
            return helper.PagedListPager(list, pageUrlFunc, PagedListRenderOptions.OnlyShowFivePagesAtATime);
        }


        public static HtmlString ProfileLik(this HtmlHelper helper, string login, int truncate = 0)
        {
            if (truncate > 0)
            {
                return helper.ActionLink(helper.Truncate(ProfileModel.GetByLogin(login).FullName(),truncate), 
                    "UserDescription", "Home", new { login }, null);
            }
            return helper.ActionLink(ProfileModel.GetByLogin(login).FullName(), "UserDescription", "Home",
                new { login }, null);
        }


        public static MvcHtmlString GetSocialIconByProvider(this HtmlHelper helper, string providerDisplayName, string providerUserId = null)
        {
            if (providerDisplayName == "Facebook")
            {
                string addHref = (providerUserId != null) ? "?id" + providerUserId : "";
                return new MvcHtmlString(string.Format("<a href='http://facebook.com/profile.php{0}'><span class='facebook social-btn'></span></a>", addHref));
            }
            if (providerDisplayName == "ВКонтакте")
            {
                string addHref = (providerUserId != null) ? "?id" + providerUserId : "";
                return new MvcHtmlString(string.Format("<a href='http://vk.com{0}'><span class='vk social-btn'></span>", addHref));
            }
            return new MvcHtmlString(string.Format("<span>{0}</span>", providerDisplayName));

        }

        public static string TitlePlatform(this HtmlHelper helper, int platformId)
        {
            var platform = PlatformModel.ForPlatform(platformId);
            if (platform == null) return Undefined;
            return platform.Title;
        }


        public static string TitlePlatformStatus(this HtmlHelper helper, int statusId)
        {
            return ideasRepository.GetPlatformStatusTitle((StatusByRealizedPlatform)statusId);
        }


        public static string LinkCorrect(this string link)
        {
            return (!link.Contains("://"))?("http://" + link.Trim()):link;
        }

        public static string MaterialLinkOnStartPage(this UrlHelper helper, MaterialModel material)
        {
            var linkToResource = material.LinkToResource;
            if (linkToResource != material.Link) return linkToResource;
            return helper.Action("Materials", "Home", new { search = material.Title });
        }


        public static string GetTitleNameOfEventLevel(this HtmlHelper helper, int level, bool returnNumberIfUnknown = true)
        {
            switch (level)
            {
                case 0:
                    return GlobalValuesAndStrings.EventLevel1;
                case 1:
                    return GlobalValuesAndStrings.EventLevel2;
                case 2:
                    return GlobalValuesAndStrings.EventLevel3;
            }
            return returnNumberIfUnknown?level.ToString():null;
        }

        public static string Duration(this HtmlHelper helper, int? firstYear, int? secondYear)
        {
            if (firstYear == null && secondYear == null) return string.Empty;
            if (firstYear != null && secondYear!=null)
            {
                return firstYear + " - " + secondYear;
            }
            if (firstYear == null)
            {
                return "до " + secondYear;
            }
            return "с " + firstYear;
        }

        #endregion //Common used utitlity

        #region Lists

        public static MvcHtmlString RoleList(this HtmlHelper helper, RoleName? wishedRole = null, string defValue = "нет",
            string name = "WishedRoleAsInt", object htmlAttribute = null)
        {
            List<SelectListItem> listItems = new List<SelectListItem>
            {
                new SelectListItem{Selected = wishedRole==null, Text = defValue, Value = ((int)RoleName.Guest).ToString()}
            };

            listItems.AddRange(userRepository.GetRolesList().ToListItem((int?)wishedRole));
            return helper.DropDownList(name, listItems,htmlAttribute);

        }


        public static HtmlString GroupIdeaList(this HtmlHelper helper, int? groupIdeas = null, int? forEvent = null,
            bool exceptEvent = false, string name = "IdeasGroup", string defValue = null, object htmlAttribute = null)
        {
            List<SelectListItem> list;
            if (defValue == null)
            {
                list = eventRepository.GetAllIdeaGroups(forEvent, exceptEvent).ToListItem(groupIdeas).ToList();                
            }
            else
            {
                list = new List<SelectListItem>
                {
                    new SelectListItem{Selected = groupIdeas==null, Text = defValue, Value = ""}
                };
                list.AddRange(eventRepository.GetAllIdeaGroups(forEvent, exceptEvent).ToListItem(groupIdeas));
            }
            return helper.DropDownList(name, list,htmlAttribute);
        }


        public static HtmlString PlatformList(this HtmlHelper helper, int? platform = null, int? forEvent = null
            , bool exceptEvent = false, string name = "Platform", List<int> exceptItem = null, string defValue = null
            , object htmlAttribute = null)
        {
            Dictionary<int, string> list = eventRepository.GetAllPlatform(forEvent, exceptEvent).Where(p=>!p.Value.IsDeleted)
                .ToDictionary(pair => pair.Key, pair => pair.Value.Title);
            IEnumerable<SelectListItem> result = exceptItem != null ? list.ToListItem(platform, exceptItem) :
                                                                        list.ToListItem(platform);
            if (defValue != null)
            {
                var listWithDef = new List<SelectListItem>
                {
                    new SelectListItem{Selected = platform==null, Text = defValue, Value = ""}
                };
                listWithDef.AddRange(result);
                return helper.DropDownList(name, listWithDef, htmlAttribute);
            }
            return helper.DropDownList(name, result, htmlAttribute);
        }


        public static HtmlString RegionList(this HtmlHelper helper, int? region = null, int? forEvent = null,
            bool exceptEvent = false, string name = "Region", object htmlAttribute = null)
        {
            IEnumerable<SelectListItem> list = eventRepository.GetAllRegions(forEvent, exceptEvent).ToListItem(region);
            return helper.DropDownList(name, list, htmlAttribute);
        }


        public static HtmlString TypeEventList(this HtmlHelper helper, int? eventType = null, string name = "EventType"
            , object htmlAttribute = null) 
        {
            IEnumerable<SelectListItem> list = eventRepository.GetEventTypes().ToListItem(eventType);
            return helper.DropDownList(name, list,htmlAttribute);
        }


        public static HtmlString RecentEventsList(this HtmlHelper helper, int? currentEvent, string name = "currentEvent"
            , object htmlAttribute = null)
        {
            var list = eventRepository.GetAllEvents(onlyRoot: true).ToDictionary(ev => ev.Id, ev => ev.Title).
                ToListItem(currentEvent);
            return helper.DropDownList(name, list, htmlAttribute);            
        }


        public static HtmlString MemberOfIdeaList(this HtmlHelper helper, int ideaId, bool withLeader = true,
            string labelAboutNobodyInGroup = "Нет участников", object htmlAttribute = null)
        {
            IdeasModel idea = IdeasModel.GetById(ideaId);
            List<ProfileModel> members = idea.MemberUsers;
            ProfileModel leader = idea.LeaderProfile;
            if (members == null || members.Count()<GlobalValuesAndStrings.NeededCountOfMemberToChangeLeader)
            {
                return new HtmlString(string.Format("<p>{0}</p>", labelAboutNobodyInGroup));
            }
            var membersList = new List<SelectListItem>();
            bool firstElementWasAssigned = false;

            foreach (var member in members)
            {
                if (leader.Id != member.Id || withLeader)
                {
                    membersList.Add(new SelectListItem
                    {
                        Text = member.FullName(),
                        Value = member.Login,
                        Selected = !firstElementWasAssigned
                    });
                    firstElementWasAssigned = true;
                }
            }
            return helper.DropDownList("member", membersList, htmlAttribute);
        }


        public static HtmlString PlatformStatusList(this HtmlHelper helper,  int? currentStatus, string name = "statusId"
            , object htmlAttribute = null)
        {
            IEnumerable<SelectListItem> list = ideasRepository.GetPlatformStatuses().ToListItem(currentStatus);
            return helper.DropDownList(name, list, htmlAttribute);
        }


        public static HtmlString ProfileCommandList(this HtmlHelper helper, string name = "profileCommandList",
            object htmlAttribute = null, RoleName? role = null)
        {
            List<SelectListItem> list;
            
            //if user authorithated
            if (role == null || role == RoleName.Guest)
            {
                list = new List<SelectListItem>
                {
                    new SelectListItem {Text = "Вход", Value = ProfileCommands.Login.GetValue(), Selected = false},
                    new SelectListItem {Text = "Регистрация", Value = ProfileCommands.Register.GetValue(), Selected = false}
                };
            }
            else
            {
                list = new List<SelectListItem>
                {
                    new SelectListItem {Text = "Редактировать профиль", Value = ProfileCommands.ProfileChange.GetValue(), Selected = false}
                };
                if (role == RoleName.Administrator)
                {
                    list.Add(new SelectListItem{Text = "Администрирование",Value=ProfileCommands.Admin.GetValue(),Selected = false});
                }

                if (role == RoleName.Student)
                {
                    list.Add(new SelectListItem { Text = "Мой проект", Value = ProfileCommands.MyIdea.GetValue(), Selected = false });
                }

                if (role == RoleName.Mentor)
                {
                    list.Add(new SelectListItem { Text = "Мои проекты", Value = ProfileCommands.MentorPage.GetValue(), Selected = false });
                    string title = "Проекты без ментора";
                    int ideasWithoutMentorCount = ideasRepository.IdeasWithoutMentorCount();
                    if ( ideasWithoutMentorCount> 0)
                    {
                        title += string.Format(" ({0})", ideasWithoutMentorCount);
                    }
                    list.Add(new SelectListItem { Text = title, Value = ProfileCommands.IdeasWithoutMentor.GetValue(), Selected = false });
                }

                list.Add(new SelectListItem { Text = "Учетные записи", Value = ProfileCommands.AccountsManage.GetValue(), Selected = false });
                list.Add(new SelectListItem { Text = "Выход", Value = ProfileCommands.Logoff.GetValue(), Selected = false });
            }
            if (htmlAttribute == null)
            {
                return helper.DropDownList(name, list, htmlAttribute);
            }
            return helper.DropDownList(name, list, htmlAttribute);
        }


        public static HtmlString EventCategoriesList(this HtmlHelper helper, string name = "categoriesEvent", object htmlAttribute = null)
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem {Text = GlobalValuesAndStrings.AllEvents, Value = EventListType.All.ToString(), Selected = false},
                new SelectListItem {Text = GlobalValuesAndStrings.PreviousEvents, Value = EventListType.Previous.ToString(), Selected = false},
                new SelectListItem {Text = GlobalValuesAndStrings.NextEvents, Value = EventListType.Next.ToString(), Selected = true}
            };
            return helper.DropDownList(name, list, htmlAttribute);            
        }

        #endregion //Lists

        #region Image filename

        public static string UrlForPicture(this HtmlHelper helper, PictureNameModel picture)
        {
            return imageManager.GetImageName(picture);
        }

        public static bool IsUrlValid(this HtmlHelper helper, PictureNameModel picture)
        {
            return imageManager.GetImageName(picture, isGetNullIfEmpty: true) != null;
        }

        public static string StandardImageFileName(this HtmlHelper helper,bool max = true, bool forUser = false)
        {
            return (max) ? imageManager.GetStandardImageName(forUser) : imageManager.GetStandardMiniImageName(forUser);
        }

        public static string AvatarFileNameForUser(this HtmlHelper helper, int userId)
        {
            PictureNameModel picture = imageRepository.GetAvatar(userId);
            string fileName =  imageManager.GetImageName(picture, true);
            return fileName;
        }

        public static string AvatarFileNameForEvent(this HtmlHelper helper, int eventId)
        {
            PictureNameModel picture = imageRepository.GetEventAvatar(eventId);
            string fileName = imageManager.GetImageName(picture, false);
            return fileName;
        }

        public static string AvatarFileNameForCompany(this HtmlHelper helper, int companyId)
        {
            PictureNameModel picture = imageRepository.GetCompanyAvatar(companyId);
            string fileName = imageManager.GetImageName(picture, false);
            return fileName;
        }

        public static string AvatarFileNameForPlatform(this HtmlHelper helper, int platformId)
        {
            PictureNameModel picture = imageRepository.GetPlatformAvatar(platformId);
            string fileName = imageManager.GetImageName(picture, false);
            return fileName;
        }

        public static string AvatarFileNameForMaterial(this HtmlHelper helper, int materialId)
        {
            PictureNameModel picture = imageRepository.GetMaterialAvatar(materialId);
            string fileName = imageManager.GetImageName(picture, false);
            return fileName;
        }

        public static string AvatarFileNameForIdea(this HtmlHelper helper, int ideaId)
        {
            PictureNameModel picture = imageRepository.GetIdeaAvatar(ideaId);
            string fileName = imageManager.GetImageName(picture, false);
            return fileName;
        }

        #endregion //Image filename

        #region Avatar and Picture for different things

        public static HtmlString DrawPicture(this HtmlHelper helper, PictureNameModel picture, bool max,
           string className, object htmlAttributes = null, bool withPopupLink = false)
        {
            string maxPictureName = imageManager.GetImageName(picture);
            string minPictureName = imageManager.GetMiniImageName(picture);
            string pictureName = (max) ? maxPictureName : minPictureName;
            return new HtmlString(string.Format("<div class='{0}' {1}><div><img src='{2}' height='100%' width='100%'" +
                                                "{3}/></div></div>", className, AttributesEncode(htmlAttributes)
                                                , pictureName, (withPopupLink) ? "data-mfp-src='" + maxPictureName + "'" +
                                                " href='" + minPictureName + "'" : ""));
        }


        public static HtmlString DrawStandardPicture(this HtmlHelper helper, bool forUser = false, bool max = true,
            object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<div {0}><img src='{1}'/></div>", AttributesEncode(htmlAttributes),
                StandardImageFileName(helper, max, forUser)));
        }


        public static HtmlString AvatarForUser(this HtmlHelper helper, int userId, bool max = false, object htmlAttributes = null,
            bool isCircle = false, bool hideIfEmptyImage = false)
        {
            PictureNameModel picture = imageRepository.GetAvatar(userId);
            string className = (max) ? "userImage" : "miniUserImage";
            string pictureName = (max) ? imageManager.GetImageName(picture, true, hideIfEmptyImage)
                                       : imageManager.GetMiniImageName(picture, true, hideIfEmptyImage);
            if (hideIfEmptyImage && pictureName == null)
            {
                return new HtmlString(string.Format("{1}<div {0}><div></div></div>{2}",
                    AttributesEncode(htmlAttributes, className)
                    , (isCircle) ? "<div class='circle'>" : "", (isCircle) ? "</div>" : ""));
            }
            
            return new HtmlString(string.Format("{2}<div  {0}><div><img src='{1}' height='100%' width='100%'/></div></div>{3}", 
                AttributesEncode(htmlAttributes, className),pictureName
                , (isCircle) ? "<div class='circle'>" : "", (isCircle) ? "</div>" : ""));
        }


        public static HtmlString AvatarForIdea(this HtmlHelper helper, int ideaId, bool max = true, object htmlAttributes = null)
        {
            PictureNameModel picture = imageRepository.GetIdeaAvatar(ideaId);
            string className = (max) ? "ideaImage" : "miniIdeaImage";
            return DrawPicture(helper, picture, max, className, htmlAttributes);
        }


        public static HtmlString AvatarForPlatform(this HtmlHelper helper, int platformId, bool max = true, object htmlAttributes = null)
        {
            PictureNameModel picture = imageRepository.GetPlatformAvatar(platformId);
            string className = (max) ? "ideaImage" : "miniIdeaImage";
            return DrawPicture(helper, picture, max, className, htmlAttributes);
        }


        public static HtmlString AvatarForMaterial(this HtmlHelper helper, int materialId, bool max = true, object htmlAttributes = null)
        {
            PictureNameModel picture = imageRepository.GetMaterialAvatar(materialId);
            string className = (max) ? "ideaImage" : "miniIdeaImage";
            return DrawPicture(helper, picture, max, className, htmlAttributes);
        }


        public static HtmlString AvatarForEvent(this HtmlHelper helper, int eventId, bool max = true, object htmlAttributes = null)
        {
            PictureNameModel picture = imageRepository.GetEventAvatar(eventId);
            string className = (max) ? "eventImage" : "miniIdeaImage";
            return DrawPicture(helper, picture, max, className, htmlAttributes);
        }


        public static HtmlString AvatarForCompany(this HtmlHelper helper, int companyId, bool max = true, object htmlAttributes = null)
        {
            PictureNameModel picture = imageRepository.GetCompanyAvatar(companyId);
            string className = (max) ? "companyImage" : "miniIdeaImage";
            return DrawPicture(helper, picture, max, className, htmlAttributes);
        }

        public static HtmlString AvatarForAdward(this HtmlHelper helper, PictureNameModel picture, 
            object htmlAttributes = null)
        {
            return DrawPicture(helper, picture, true, "awardImage", htmlAttributes);
        }

        public static HtmlString CompanyLogo(this HtmlHelper helper, CompanyModel company)
        {
            string img  = AvatarForCompany(helper, (int)company.Id, true).ToString();
            string link = string.Format("<a href='{0}'  class='withoutDecoration' target='_blank'>{1}</a>", company.Site, img);
            return new HtmlString(link);
        }

        #endregion //Avatar and Picture for different things

        #region Image Loader Utility

        public static HtmlString ImageLoader(this HtmlHelper helper, string name, string dataRemark, string imageLink,
            StandardRatioType ratio, object htmlAttributes = null, bool justResizer = false)
        {
            return new HtmlString(string.Format("<div id='div_{0}' data-pic='{1}' class='pictureLoaderDiv {4} {5}' {3}><img id='{0}' " +
                                                    "src='{2}' data-pic='{1}' ></img></div>"
                                                    , name, dataRemark, imageLink, AttributesEncode(htmlAttributes)
                                                    , ratio.GetCSSClass(),justResizer?"justLoader":""));
        }


        public static HtmlString ImageLoaderLink(this HtmlHelper helper, string title, string dataRemark,
            object htmlAttributes = null, bool isForUser = false, ImageSizeParams size = null)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return new HtmlString(string.Format("<a data-tag='{2}' data-pic='{0}' data-action='{4}' {1}>{3}</a>" +
                                                "<input type='hidden' name='x1' class='size_{0}' {5}>" +
                                                "<input type='hidden' name='x2' class='size_{0}' {6}>" +
                                                "<input type='hidden' name='y1' class='size_{0}' {7}>" +
                                                "<input type='hidden' name='y2' class='size_{0}' {8}>"
                                                , dataRemark
                                                , AttributesEncode(htmlAttributes, "imageLoaderLink"), isForUser, title
                                                , url.Action("LoadImage", "Widget")
                                                , (size != null && size.x1 > 0) ? "value='" + size.x1 + "'" : ""
                                                , (size != null && size.x2 > 0) ? "value='" + size.x2 + "'" : ""
                                                , (size != null && size.y1 > 0) ? "value='" + size.y1 + "'" : ""
                                                , (size != null && size.y2 > 0) ? "value='" + size.y2 + "'" : ""));
        }

        public static HtmlString SimpleImageSize(this HtmlHelper helper, string dataRemark, bool isForUser = false,
            ImageSizeParams size = null)
        {
            return new HtmlString(string.Format("<input type='hidden' name='x1' class='size_{0}' {1}>" +
                                                "<input type='hidden' name='x2' class='size_{0}' {2}>" +
                                                "<input type='hidden' name='y1' class='size_{0}' {3}>" +
                                                "<input type='hidden' name='y2' class='size_{0}' {4}>"
                                                , dataRemark
                                                , (size != null && size.x1 > 0) ? "value='" + size.x1 + "'" : ""
                                                , (size != null && size.x2 > 0) ? "value='" + size.x2 + "'" : ""
                                                , (size != null && size.y1 > 0) ? "value='" + size.y1 + "'" : ""
                                                , (size != null && size.y2 > 0) ? "value='" + size.y2 + "'" : ""));
        }

        public static HtmlString ImageLabel(this HtmlHelper helper, string title, string dataRemark,
            object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<span class='imageLabel' data-pic='{0}' {2}>{1}</span>",
                                    dataRemark, title, AttributesEncode(htmlAttributes)));
        }

        public static HtmlString ImageRotateLink(this HtmlHelper helper, string dataRemark, string title, bool isHidden = true,
            object htmlAttributes = null)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            return new HtmlString(string.Format("<a data-pic='{0}' {1} data-url='{3}'>{2}</a>"
                                                    , dataRemark, AttributesEncode(htmlAttributes, "rotateLink"+((isHidden)?
                                                    " imageLabel" : string.Empty)), title, url.Action("RotateImage", "Widget")));
        }

        
        public static HtmlString ImagePreview(this HtmlHelper helper, string name, string dataRemark, StandardRatioType ratio,
            object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<div id='{0}' data-pic='preview_{1}' class='imagePreview {3}' {2}>" +
                                                    "<img></img></div>"
                                                    , name, dataRemark, AttributesEncode(htmlAttributes)
                                                    , ratio.GetCSSClassForPreview()));
        }

        public static HtmlString ImageSizeSpan(this HtmlHelper helper, string dataRemark, object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<span {0} data-from='{1}'></span>", 
                AttributesEncode(htmlAttributes, "previewSize"),dataRemark));
        }

        public static HtmlString ImageCoordinateSpan(this HtmlHelper helper, string dataRemark, object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<span {0} data-from='{1}'></span>", 
                AttributesEncode(htmlAttributes, "previewCoordinates"),dataRemark));
        }

        public static HtmlString BorderSetter(this HtmlHelper helper,string id, string borderForId, string label,
            string dataRemark = null, object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<input type='checkbox' id='{3}' data-border-to='{0}' {1} {2}/><label for={3} {1} {2}>{4}</label>",
                borderForId, (dataRemark != null) ? ("data-pic='" + dataRemark + "'") : string.Empty,
                AttributesEncode(htmlAttributes, "borderSetter"),id, label));

        }


        //All method bellow this comment for work with photo in the gallery and chat
        
        public static HtmlString SimpleImageLoader(this HtmlHelper helper, string name, string title, bool multiple = true,
          object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<input type='file' name='{0}' class='pictureJustLoading' value='' {3}/><a " +
                                                "{1}>{2}</a>", name, AttributesEncode(htmlAttributes, 
                                                "pictureJustLoadingLink"), title,(multiple)?"multiple='multiple'":string.Empty));
        }

        public static HtmlString SimpleProgressLoader(this HtmlHelper helper, string tag, string name, object htmlAttributes = null)
        {
            return new HtmlString(string.Format("<{0} name='{1}' {2}></{0}>", tag, name
                , AttributesEncode(htmlAttributes, "pictureJustLoadingProgress")));
        }

        #endregion Image Loader Utility

        #region Image Loader Block (used Image Loader Utility - previous region)

        public static HtmlString LoaderBlock(this HtmlHelper helper, string dataRemark, string initAvatarFileName
            , StandardRatioType ratio, string linkToCancel)
        {
            return new HtmlString(string.Format("<h3>{0}</h3><div class='imageLoaderWrapper'><div class='aboveImage'></div> {1}" +
                                                " <div class='imageTools'> {2} " +
                                                "<a class = 'customButton mediumSizeButton grey withAjax imageLabel cancelLoad' " +
                                                "href='{3}' title='{5}' data-pic=" +
                                                "'{4}'>{5}</a>{6}<a title='{7}' class='submit customButton orange " +
                                                "mediumSizeButton imageLabel imageUpdate' data-pic='{4}'>{7}</a></div></div>",
                helper.ImageLabel(GlobalValuesAndStrings.PromptToResizeYourImage, dataRemark),
                helper.ImageLoader(dataRemark+GlobalValuesAndStrings.LoaderNameSuffix, dataRemark, initAvatarFileName, ratio),
                helper.ImageLoaderLink(GlobalValuesAndStrings.ChangeLinkForImageLoader, dataRemark, new
                    {
                        @class = "customButton mediumSizeButton grey",
                        title = GlobalValuesAndStrings.ChangeLinkForImageLoader
                    }, ratio==StandardRatioType.UserRatio),
                linkToCancel,
                dataRemark,
                GlobalValuesAndStrings.CancelLinkForImageLoader,
                helper.ImageRotateLink(dataRemark, GlobalValuesAndStrings.RotateLinkForImageLoader, true
                    , new { @class = "customButton mediumSizeButton grey", title = GlobalValuesAndStrings.RotateLinkForImageLoader }),
                GlobalValuesAndStrings.SaveLinkForImageLoader
                ));
        }

        public static HtmlString PreviewBlock(this HtmlHelper helper, string dataRemark, StandardRatioType ratio,
            ImagePreviewFeatures features = ImagePreviewFeatures.None)
        {
            return new HtmlString(string.Format("<h3>{0}</h3>{1}{2}{3}{4}", 
                                                helper.ImageLabel(GlobalValuesAndStrings.TipsToPreview,dataRemark),
                                                helper.ImagePreview(GlobalValuesAndStrings.PreviewCSSClass, dataRemark
                                                    , ratio),
                                                features.HasFlag(ImagePreviewFeatures.Size)?
                                                    string.Format("<div><span class='imageLabel' data-pic='{0}'>Размер: " +
                                                                "</span>{1}</div>", dataRemark,helper.ImageSizeSpan(dataRemark)):"",
                                                features.HasFlag(ImagePreviewFeatures.Coordinate) ?
                                                    string.Format("<div><span class='imageLabel' data-pic='{0}'>Координаты: " +
                                                                "</span>{1}</div>", dataRemark, helper.ImageSizeSpan(dataRemark)) : "",
                                                features.HasFlag(ImagePreviewFeatures.Border) ?
                                                    string.Format("<div>{0}</div>", helper.BorderSetter(
                                                        GlobalValuesAndStrings.BorderSetterId,
                                                        GlobalValuesAndStrings.PreviewCSSClass,
                                                        GlobalValuesAndStrings.TipsToBorderShow,
                                                        dataRemark, 
                                                        new{@class=GlobalValuesAndStrings.OnlyAfterUpoadingCSSClass})) : ""));
        }

        #endregion

        #region Working with Grid Class

        public static void ClearGridClass(this HtmlHelper helper)
        {
            ClassGridValue = 0;
        }

        public static HtmlString GridClass(this HtmlHelper helper, int gridSize, int suffix = 0)
        {
            int numberItemForGridClass = ClassGridValue;
            string addClass = " grid_" + gridSize;
            if (numberItemForGridClass % 12 == 0) addClass += " alpha";
            if ((numberItemForGridClass+gridSize+suffix) % 12 == 0) addClass += " omega";
            if (suffix != 0) addClass += " suffix_" + suffix.ToString();
            addClass += " ";
            ClassGridValue = numberItemForGridClass + gridSize+suffix;
            return new HtmlString(addClass);
        }

        public static HtmlString FloatTitle(this HtmlHelper helper, string title, bool isTopContainer = false,
            bool isSubTitle = false)
        {
            return new HtmlString(string.Format(
            @"<div class='fullBlock{0}'>
                <div class='container_12 titleFloat'>
                    <div class='grid_11 titleName'>
                        <span{1}>{2}</span>
                    </div>
                </div>
            </div>", (isTopContainer) ? " topContainer" : string.Empty, (isSubTitle) ? " class='subTitle'" : string.Empty, title));
        }

        #endregion //Working with Grid Class

        #region Tips

        public static HtmlString RenderTips(this HtmlHelper helper, StudentTips student, params Tip[] tips)
        {
            var user = ProfileModel.Current;
            if(user == null || user.Role!=RoleName.Student || (user.WishedRole!=null && user.WishedRole!=RoleName.Student
                && user.WishedRole != RoleName.Guest)) return new HtmlString(string.Empty);
            var currentTip = (StudentTips)user.Tips;
            if (currentTip.HasFlag(student)) return new HtmlString(string.Empty);
            currentTip = currentTip | student;
            userRepository.SetTips(user.Login, (int)currentTip);
            return RenderTips(helper, tips);
        }

        public static HtmlString RenderTips(this HtmlHelper helper, MentorTips mentor, params Tip[] tips)
        {
            var user = ProfileModel.Current;
            if (user == null || user.Role != RoleName.Mentor || (user.WishedRole != null && user.WishedRole != RoleName.Mentor
                && user.WishedRole != RoleName.Guest)) return new HtmlString(string.Empty);
            var currentTip = (MentorTips)user.Tips;
            if (currentTip.HasFlag(mentor)) return new HtmlString(string.Empty);
            currentTip = currentTip | mentor;
            userRepository.SetTips(user.Login, (int)currentTip);
            return RenderTips(helper, tips);
        }

        public static HtmlString RenderTips(this HtmlHelper helper, AdminTips admin, params Tip[] tips)
        {
            var user = ProfileModel.Current;
            if (user == null || user.Role != RoleName.Administrator || (user.WishedRole != null
                && user.WishedRole != RoleName.Administrator
                && user.WishedRole != RoleName.Guest)) return new HtmlString(string.Empty);
            var currentTip = (AdminTips)user.Tips;
            if (currentTip.HasFlag(admin)) return new HtmlString(string.Empty);
            currentTip = currentTip | admin;
            userRepository.SetTips(user.Login, (int)currentTip);
            return RenderTips(helper, tips);
        }

        public static HtmlString UnregisterUserRenderTips(this HtmlHelper helper, params Tip[] tips)
        {
            var user = ProfileModel.Current;
            if (user != null) return new HtmlString(string.Empty);
            if (HttpContext.Current.Session[GlobalValuesAndStrings.TipSessionName] != null) return new HtmlString(string.Empty);
            HttpContext.Current.Session[GlobalValuesAndStrings.TipSessionName] = true;
            return RenderTips(helper, tips);
        }


        public static HtmlString RenderTips(this HtmlHelper helper, params Tip[] tips)
        {
            var stringBuilder = new StringBuilder();
            for (int number = 0; number < tips.Count(); number++)
            {
                tips[number].Number = number + 1;
                stringBuilder.Append(tips[number].ToString());
            }
            return new HtmlString(stringBuilder.ToString());
        }

        public static HtmlString RenderNews(this HtmlHelper helper, int width = 300, string blockId = "welcomeTitle")
        {
            var user = ProfileModel.Current;
            if (user == null) return new HtmlString(string.Empty);
            var newsList = eventRepository.GetNews(user.Role);
            if (!newsList.Any())
            {
                return new HtmlString(string.Empty);
            }
            var news = newsList.First();
            var tip = new Tip(news.Description, blockId)
            {
                Type = TipType.Top,
                Width = width,
                OffsetY = 15,
                OffsetX = 100,
                TriangleOffset = 144,
                Number = 0
            };
            return new HtmlString(tip.ToString());
        }

        #endregion //Tips

        #region Helper

        private static string AttributesEncode( object attributes, string addClasses = null)
        {
            if (attributes == null)
            {
                if(addClasses==null) return string.Empty;
                return string.Format("class='{0}'", addClasses);
            }
            string attributesString = string.Empty;
            Type myType = attributes.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            bool isAddClassesAdded = false;
            foreach (PropertyInfo prop in props)
            {
                string propValue = prop.GetValue(attributes, null).ToString();
                string propName = prop.Name;
                if (propName == "class" && addClasses != null)
                {
                    propValue += " " + addClasses;
                    isAddClassesAdded = true;
                }
                attributesString += string.Format(" {0}='{1}' ", propName, propValue);
            }
            if (addClasses != null && !isAddClassesAdded)
            {
                attributesString += string.Format(" class='{0}' ", addClasses);
            }
            return attributesString + " ";
        }

        private static int ClassGridValue
        {
            get
            {
                string ocKey = GlobalValuesAndStrings.GridSessionName + HttpContext.Current.GetHashCode().ToString("x");
                if (!HttpContext.Current.Items.Contains(ocKey))
                {
                    HttpContext.Current.Items.Add(ocKey, 0);
                }
                return (HttpContext.Current.Items[ocKey] as int?)??0;
            }
            set
            {
                string ocKey = GlobalValuesAndStrings.GridSessionName + HttpContext.Current.GetHashCode().ToString("x");
                if (!HttpContext.Current.Items.Contains(ocKey))
                {
                    HttpContext.Current.Items.Add(ocKey, value);
                }
                else
                {
                    HttpContext.Current.Items[ocKey] = value;
                }
            }
        }
        #endregion //Helper
    }
}