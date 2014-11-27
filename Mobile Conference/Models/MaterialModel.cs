using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Managers;
using PagedList;

namespace MobileConference.Models
{
    public class MaterialModel
    {
        private static readonly IEventRepository eventRepository;
        private static readonly IIdeasRepository ideasRepository;

         static MaterialModel()
        {
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();
            ideasRepository = ContainerDI.Container.Resolve<IIdeasRepository>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = "Введите технологию")]
        [Display(Name = "Технология")]
        public int? PlatformId { get; set; }
        public DateTime? AddedDate { get; set; }

        private int? descriptionId;
        public int? DescriptionId
        {
            get { return descriptionId ?? (descriptionId = ideasRepository.GetTextForMateralAsCommentId(Id)); }
        }
        
        public string GetDescription()
        {

                if (DescriptionId == null) return string.Empty;
                var comment = ideasRepository.GetComment((int)descriptionId);
                if (comment == null) return string.Empty;
                return comment.Message;
        }

        [Required(ErrorMessage = "Введите ссылку на материал")]
        [Display(Name = "Ссылка")]
        public string Link { get; set; }

        /// <summary>
        /// Auxiliary field to save this text in the comment
        /// </summary>
        [AllowHtml]
        public string Text { get; set; }

        public string LinkToResource
        {
            get
            {
                UrlHelper urlHelper;
                if (LinkManager.HasDomain(Link,"youtube.com"))
                {
                    var query = LinkManager.GetQuery(Link);
                    if (query.ContainsKey("v"))
                    {
                        urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                        return urlHelper.Action("ShowVideo", "Home", new {materialId = Id
                            , link = query["v"]});
                    }
                }
                if (Link == GlobalValuesAndStrings.ExternalMaterialLink)
                {
                    urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                    return urlHelper.Action("ShowMaterial", "Home", new {materialId = Id});
                }
                return Link;
            }    
        }

        public int CommentCount { get; set; }

        public PlatformModel Platform
        {
            get { return (PlatformId!=null)?PlatformModel.ForPlatform((int)PlatformId):null; }
        }


        public string MaterialCountPrettyTitle
        {
            get
            {
                if (CommentCount == 0) return GlobalValuesAndStrings.WithoutComments;
                int lastFigure = CommentCount%10;
                if (CommentCount > 4 && (CommentCount < 21 || lastFigure > 4 || lastFigure == 0))
                    return string.Format("{0} {1}",CommentCount,GlobalValuesAndStrings.ManyComments);
                if (CommentCount == 1 || lastFigure == 1) 
                    return string.Format("{0} {1}", CommentCount, GlobalValuesAndStrings.OneComment);
                return string.Format("{0} {1}", CommentCount, GlobalValuesAndStrings.SomeComments);
            }
        }

        public string ShowMaterialMessage
        {
            get
            {
                if (CommentCount == 0) return GlobalValuesAndStrings.AddCommentsTitle;
                return GlobalValuesAndStrings.ShowCommentsTitle;
            }
        }

        public bool IsInCurrentEvent
        {
            get
            {
                List<EventProfile> listEvents = eventRepository.GetEventsForMaterial(Id).ToList();
                return listEvents.FirstOrDefault(ev => ev.Id == OptionModel.Current.CurrentEventId) != null;
            }
        }


        public IPagedList<Comment> GetComments(int page = GlobalValuesAndStrings.FirstPageCount)
        {
            return ideasRepository.GetCommentsForLink(Id, CommentModelType.Material, page);
        }


        public static MaterialModel ForMaterial(int id)
        {
            return eventRepository.GetMaterialById(id).ToModel();
        }

    }
}