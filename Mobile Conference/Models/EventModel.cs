using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobileConference.CustomAttribute;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Managers;

namespace MobileConference.Models
{
    public class EventModel
    {
        private static readonly IEventRepository eventRepository;

        static EventModel()
        {
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();
        }

        [Required(ErrorMessage = "Введите название")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Введите описание")]
        [Display(Name = "Описание")]
        public string Description { get; set; }


        [Display(Name = "Дата начала")]
        [Required(ErrorMessage = "Введите дату начала")]
        [DataType(DataType.Date)]
        [DateFormat]
        public string DateFrom { get; set; }


        [Display(Name = "Дата окончания")]
        [Required(ErrorMessage = "Введите дату окончания")]
        [DataType(DataType.Date)]
        [DateFormat]
        public string DateTo { get; set; }

        public int Id { get; set; }
        public int? Avatar { get; set; }

        [Display(Name = "Тип")]
        public int? EventType { get; set; }

        public int? ParentId { get; set; }
        public bool IsDeleted { get; set; }
        public int[] ChildIds { get; set; }
        public int[] Regions { get; set; }
        public int[] Platforms { get; set; }
        public int[] IdeaTypes { get; set; }


        public int? DayFrom
        {
            get
            {
                var dateFrom = DateFrom.ConvertToDate();
                if (dateFrom == null) return null;
                return dateFrom.Value.Day;
            }
        }

        public int? DayTo
        {
            get
            {
                var dateTo = DateTo.ConvertToDate();
                if (dateTo == null) return null;
                return dateTo.Value.Day;
            }
        }

        /// <summary>
        /// Children with removed events
        /// </summary>
        public int[] ChildIdsAll
        {
            get { return eventRepository.GetChildsIdForEvent(Id, true); }
        }

        public string TitleWithRemoveLabel
        {
            get { return IsDeleted ? string.Format("{0} (удален)", Title) : Title; }
        }

        public int Level
        {
            get
            {
                if (ParentId == null) return GlobalValuesAndStrings.FirstLevelInEvent;
                EventModel parent = GetById((int) ParentId);
                if (parent == null) return GlobalValuesAndStrings.FirstLevelInEvent;
                return parent.Level + 1;
            }
        }

        public int NonRemovedPlatforms
        {
            get
            {
                return Platforms == null ? -1 : Platforms.Select(PlatformModel.ForPlatform).Count(i => !i.IsDeleted);
            }
        }

        public string TypeEventTitle
        {
            get
            {
                if (EventType == null)
                {
                    return string.Empty;
                }
                string title;
                if (CacheManager.TryReadEventTypeModel((int) EventType, out title))
                {
                    return title;
                }
                else
                {
                    title = eventRepository.GetEventTypeTitle((int) EventType);
                    CacheManager.UpdateEventTypeModel((int) EventType, title);
                }
                return title;
            }
        }


        public string DurationString
        {
            get
            {
                DateTime? dateFrom = DateFrom.ConvertToDate();
                DateTime? dateTo = DateTo.ConvertToDate();
                if (dateFrom == null)
                {
                    return dateTo == null ? string.Empty : "до " + dateTo.ConvertToDayMonthString();
                }
                if (dateTo == null ||
                    (dateTo.Value.Month == dateFrom.Value.Month && dateTo.Value.Day == dateFrom.Value.Day))
                {
                    return dateFrom.ConvertToDayMonthString();
                }
                return string.Format("{0} - {1}", dateFrom.ConvertToDayMonthString(), dateTo.ConvertToDayMonthString());
            }
        }

        public bool IsIncludeAllChildren
        {
            get
            {
                //if event hasn't got a children than this restiction doesn't need
                if (ChildIds == null || !ChildIds.Any()) return true;
                return ChildIds.Select(GetById).All(child => child.IsDateInsideIn(this));
            }
        }


        public bool IsDateInsideIn(EventModel outsideModel, bool withoutNull = true)
        {
            //if model is infinity, all children can be inside
            if (outsideModel.DateFrom == null && outsideModel.DateTo == null) return true;

            //model has both date constraints
            if (outsideModel.DateFrom != null && outsideModel.DateTo != null)
            {
                return DateFrom != null && DateFrom.ConvertToDate() >= outsideModel.DateFrom.ConvertToDate() &&
                       DateTo != null && DateTo.ConvertToDate() <= outsideModel.DateTo.ConvertToDate();
            }
            if (withoutNull) return true;

            //model has only finish date
            if (outsideModel.DateFrom == null)
            {
                return DateTo != null && DateTo.ConvertToDate() <= outsideModel.DateTo.ConvertToDate();
            }

            //model has only start date
            return DateFrom != null && DateFrom.ConvertToDate() >= outsideModel.DateFrom.ConvertToDate();
        }


        public List<EventModelWithOrder> GetCurrentChild(int count = 3)
        {
            if (ChildIds == null || !ChildIds.Any()) return new List<EventModelWithOrder>();

            List<EventModelWithOrder> children = ChildIds.Select(GetById).OrderBy(ev => ev.DateFrom.ConvertToDate()
                                                                                        ?? DateTime.MinValue)
                .Select((e, i) => new EventModelWithOrder {EventModel = e, Order = i + 1})
                .ToList();

            if (children.Count() < count) return children;

            var recentChildren = children.Where(e => e.EventModel.DateTo.ConvertToDate() != null &&
                                                     e.EventModel.DateTo.ConvertToDate() >= DateTime.Now);

            //if current children less than needed, return all last children
            if (recentChildren.Count() < count)
            {
                return children.Where(e => e.Order > (children.Count() - count)).ToList();
            }
            return recentChildren.Take(count).ToList();
        }

        #region static methods

        public static EventModel GetById(int eventId)
        {
            EventModel model;
            if (CacheManager.TryReadEventModel(eventId, out model)) return model;

            model = eventRepository.GetEventModel(eventId, true);

            CacheManager.UpdateEventModel(eventId, model);
            return model;
        }


        public static EventModel Current
        {
            get { return GetById(OptionModel.Current.CurrentEventId); }
        }


        public static PlatformModel GetPlatform(int platformId)
        {
            return PlatformModel.ForPlatform(platformId);
        }


        public static string GetTitleForIdeaGroup(int ideaPlatformId)
        {
            string title;
            if (CacheManager.TryReadGroupIdeaModel(ideaPlatformId, out title)) return title;
            title = eventRepository.GetTitleIdeaGroupById(ideaPlatformId);
            CacheManager.UpdateGroupIdeaModel(ideaPlatformId, title);
            return title;
        }


        public static string GetTitleForRegion(int regionId)
        {
            string title;
            if (CacheManager.TryReadRegionTitle(regionId, out title)) return title;
            title = eventRepository.GetRegionTitleById(regionId);
            CacheManager.UpdateRegionTitle(regionId, title);
            return title;
        }

        /// <summary>
        /// Using only in repositories
        /// </summary>
        public static void SinchronizeWithCache(int eventId)
        {
            EventModel model;
            //if (!CacheManager.TryReadEventModel(eventId, out model)) return;
            model = eventRepository.GetEventModel(eventId);
            CacheManager.UpdateEventModel(eventId, model);
        }

        #endregion //static methods
    }
}