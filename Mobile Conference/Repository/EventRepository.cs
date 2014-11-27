using Castle.Core.Internal;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Managers;
using MobileConference.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileConference.Repository
{
    /// <summary>
    /// Repository for manage Event in database (and also Platform, GroupIdea,  News, Location)
    /// </summary>
    public class EventRepository:IEventRepository
    {
        private Entities context
        {
            get { return ContextEF.Context; }
        }


        #region Work with Events

        public IQueryable<EventProfile> GetCurrentEvents()
        {
            var currentEvents = context.EventProfiles.Where(e => e.Id == EventModel.Current.Id);
            return currentEvents;
        }


        public IPagedList<EventProfile> GetNeededLevelEvents(int rootId, int level, EventListType eventListType,
            bool withDeleted = false, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var rootEvent = GetEventById(rootId);
            if(rootEvent == null) return null;

            //get children level 1 for the root parent
            IQueryable<EventProfile> events = withDeleted? rootEvent.EventProfile1.AsQueryable()
                                            : rootEvent.EventProfile1.Where(e=>e.IsDeleted!=true).AsQueryable();

            if (level > 1)
            {
                for (int curLevel = 1; curLevel < level; curLevel++)
                {
                    //get children curLevel+1 level for parents
                    events = withDeleted ? events.SelectMany(ev => ev.EventProfile1) 
                                : events.SelectMany(ev => ev.EventProfile1.Where(e => e.IsDeleted != true));
                }
            }

            IPagedList<EventProfile> orderedEvents;

            switch (eventListType)
            {
                case EventListType.Next:
                    orderedEvents = events.Where(ev => ev.DateFrom.Value.Add(new TimeSpan(1,0,0,0)) >= DateTime.Now)
                        .OrderBy(e=>e.DateFrom)
                        .ToPagedList(page, OptionModel.Current.ItemPerPage);
                    break;
                case EventListType.Previous:
                    orderedEvents = events.Where(ev => ev.DateFrom.Value.Add(new TimeSpan(1, 0, 0, 0)) < DateTime.Now)
                        .OrderByDescending(e => e.DateFrom)
                        .ToPagedList(page, OptionModel.Current.ItemPerPage);
                    break;
                default:
                    orderedEvents = events.OrderBy(e => e.DateFrom).ToPagedList(page, OptionModel.Current.ItemPerPage);
                    break;
            }

            return orderedEvents;
        }

        public IPagedList<EventProfile> GetEventForStage(int stageId, int page = GlobalValuesAndStrings.FirstPageCount)
        {
            var stage = GetEventById(stageId);
            if (stage == null || stage.EventProfile2 == null || stage.EventProfile2.EventProfile2 != null)
            {
                return null;
            }
            return context.EventProfiles.Where(e => e.Parent == stageId && e.IsDeleted!=true).OrderBy(e => e.DateFrom)
                        .ToPagedList(page, OptionModel.Current.ItemPerPage);
        }

        public EventModel GetEventModel(int eventId, bool withDeleted = false)
        {
            return GetEventById(eventId, withDeleted).ToModel();
        }


        public Dictionary<int,string> GetEventTypes()
        {
            return context.EventTypes.ToDictionary(types => types.Id, types => types.title);
        }


        public string GetEventTypeTitle(int eventTypeId)
        {
            EventType eventType = context.EventTypes.FirstOrDefault(t => t.Id == eventTypeId);
            return  eventType==null? string.Empty:eventType.title;
        }


        public IPagedList<EventProfile> GetAllEvents(DateTime? dateFrom = null, DateTime? dateTo = null, 
            int page = GlobalValuesAndStrings.FirstPageCount, bool withDeleted = false, bool onlyRoot = false)
        {
            IQueryable<EventProfile> events = (onlyRoot)
                ? context.EventProfiles.Where(ev => ev.Parent == null)
                : context.EventProfiles;
            if (dateFrom != null && dateTo != null)
            {
                events = events.Where(ev => ev.DateFrom >= dateFrom && ev.DateTo <= dateTo);
            }
            else if(dateFrom!=null)
            {
                events = events.Where(ev => ev.DateFrom >= dateFrom);
            }
            else if(dateTo != null)
            {
                events = events.Where(ev => ev.DateTo <= dateTo);
            }
            if (!withDeleted) events = events.Where(ev => ev.IsDeleted != true);
            return events.OrderBy(ev => ev.DateFrom).ToPagedList(page, OptionModel.Current.ItemPerPage);
        }


        public int[] GetChildsIdForEvent(int eventId, bool withDeleted = false)
        {
            var eventProfile = GetEventById(eventId, true);
            if (eventProfile == null) return new int[]{};

            //children for event
            var eventChildren = withDeleted?eventProfile.EventProfile1:eventProfile.EventProfile1.Where(e=>e.IsDeleted!=true);
            if (eventChildren == null) return new int[] { };
            return eventChildren.Select(e => e.Id).ToArray();
        }

        #endregion //Work with Events

        #region Manage to Experts

        public List<ProfileModel> GetExpertForEvent(int eventId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return new List<ProfileModel>();
            return eventProfile.ExpertForEvents.Select(e=>e.UserProfile).AsQueryable().ToModel();
        }

        #endregion //Manage to Experts

        #region News

        public IPagedList<News> GetAllNews(int page = GlobalValuesAndStrings.FirstPageCount, DateTime? dateFrom = null
            , DateTime? dateTo = null, bool withDeleted = false)
        {
            IQueryable<News> news = (withDeleted)?context.News:context.News.Where(n=>!n.IsDeleted);
            if (dateFrom != null)
            {
                news = news.Where(n => n.DateTo>=dateFrom);
            }
            if (dateTo != null)
            {
                dateTo = new DateTime(dateTo.Value.Year, dateTo.Value.Month, dateTo.Value.Day, 23, 59, 59);
                news = news.Where(n => n.Date<=dateTo);
            }
            return news.OrderBy(n => n.Date).ToPagedList(page, OptionModel.Current.ItemPerPage);
        }

        public  List<NewsModel> GetNews(RoleName roles)
        {
            DateTime now = DateTime.Now;
            return context.News.Where(n => !n.IsDeleted && n.Date <= now && n.DateTo >= now).AsEnumerable().ToModel()
                .Where(r=>r.RoleFor.IsRoleInSet(roles)).ToList();
        }


        public News GetNewsById(int id)
        {
            return context.News.FirstOrDefault(n => n.Id == id);
        }


        public void AddNews(NewsModel model)
        {
            var news = new News
            {
                Date = (DateTime)model.DateFrom,
                DateTo = (DateTime)model.DateTo,
                Description = model.Description,
                EventId = model.EventId,
                ForRoles = (int)model.RoleFor,
                IsDeleted = false,
                Title = model.Title
            };
            context.News.Add(news);
            context.SaveChanges();
        }

        public void RemoveNews(int newsId)
        {
            var news = GetNewsById(newsId);
            if (news == null) return;
            news.IsDeleted = true;
            context.SaveChanges();
        }


        public void RestoreNews(int newsId)
        {
            var news = GetNewsById(newsId);
            if (news == null) return;
            news.IsDeleted = false;
            context.SaveChanges();
        }


        public void UpdateNews(NewsModel model)
        {
            var news = GetNewsById(model.Id);
            if (news == null) return;
            news.Title = model.Title;
            news.Description = model.Description;
            news.Date = (DateTime)model.DateFrom;
            news.DateTo = (DateTime)model.DateTo;
            news.ForRoles = (int)model.RoleFor;
            context.SaveChanges();
        }


        //Unused method now
        public List<NewsModel> GetLastNews()
        {
            return null;
            //    return context.News.Where(n => !n.IsDeleted && n.EventId == OptionModel.Current.CurrentEventId).
            //        OrderByDescending(n => n.Date).Take(OptionModel.Current.RecentNewsCount).AsEnumerable().ToModel();
        }

        #endregion //News

        #region Changing params of Events
        public void AddGroupIdeaToEvent(int eventId, int groupIdeaId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            var groupIdea = eventProfile.GroupIdeas.FirstOrDefault(group => group.Id == groupIdeaId);
            if (groupIdea != null) return;
            groupIdea = context.GroupIdeas.FirstOrDefault(group => group.Id == groupIdeaId);
            if (groupIdea == null) return;
            eventProfile.GroupIdeas.Add(groupIdea);
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
        }


        public void RemoveGroupIdeaFromEvent(int eventId, int groupIdeaId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            var groupIdea = eventProfile.GroupIdeas.FirstOrDefault(group => group.Id == groupIdeaId);
            if (groupIdea == null) return;
            eventProfile.GroupIdeas.Remove(groupIdea);
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
        }


        public void AddPlatformToEvent(int eventId, int platformId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            var platform = eventProfile.Platforms.FirstOrDefault(platf => platf.Id == platformId);
            if (platform != null) return;
            platform = context.Platforms.FirstOrDefault(platf => platf.Id == platformId);
            if (platform == null) return;
            eventProfile.Platforms.Add(platform);
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
        }


        public void RemovePlatformFromEvent(int eventId, int platformId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            var platform = eventProfile.Platforms.FirstOrDefault(platf => platf.Id == platformId);
            if (platform == null) return;
            eventProfile.Platforms.Remove(platform);
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
        }


        public void AddRegionToEvent(int eventId, int regionId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            var region = eventProfile.Regions.FirstOrDefault(reg => reg.Id == regionId);
            if (region != null) return;
            region = context.Regions.FirstOrDefault(reg => reg.Id == regionId);
            if (region == null) return;
            eventProfile.Regions.Add(region);
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
        }


        public void RemoveRegionFromEvent(int eventId, int regionId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            var region = eventProfile.Regions.FirstOrDefault(reg => reg.Id == regionId);
            if (region == null) return;
            eventProfile.Regions.Remove(region);
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
        }


        public void SetTypeForEvent(int eventId, int eventTypeId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            var eventType = context.EventTypes.FirstOrDefault(ev => ev.Id == eventTypeId);
            if (eventType == null) return;
            eventProfile.Type = eventType.Id;
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
        }


        public int AddEvent(EventModel model)
        {
            var eventProfile = new EventProfile
            {
                DateFrom = model.DateFrom.ConvertToDate(),
                DateTo = model.DateTo.ConvertToDate(),
                Desctiption = model.Description,
                Type = model.EventType ?? 0,
                Title = model.Title,
                Parent = model.ParentId,
                IsDeleted = false
            };
            context.EventProfiles.Add(eventProfile);
            context.SaveChanges();
            if (model.ParentId != null)
            {
                EventModel.SinchronizeWithCache((int)model.ParentId);                
            }
            return eventProfile.Id;
        }


        public void UpdateEventModel(EventModel model)
        {
            if (model == null) return;
            var eventProfile = GetEventById(model.Id);
            if (!model.DateFrom.IsNullOrEmpty()) eventProfile.DateFrom = model.DateFrom.ConvertToDate();
            if (!model.DateTo.IsNullOrEmpty()) eventProfile.DateTo = model.DateTo.ConvertToDate();
            if (!model.Title.IsNullOrEmpty()) eventProfile.Title = model.Title;
            if (!model.Description.IsNullOrEmpty()) eventProfile.Desctiption = model.Description;
            if (model.EventType!=null) SetTypeForEvent(model.Id,(int)model.EventType);
            context.SaveChanges();
            EventModel.SinchronizeWithCache(model.Id);
        }


        public void RemoveEvent(int eventId)
        {
            var eventProfile = GetEventById(eventId);
            if (eventProfile == null) return;
            eventProfile.IsDeleted = true;
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
            if (eventProfile.Parent != null)
            {
                EventModel.SinchronizeWithCache((int)eventProfile.Parent);                
            }
        }

        public void RestoreEvent(int eventId)
        {
            var eventProfile = GetEventById(eventId,true);
            if (eventProfile == null) return;
            eventProfile.IsDeleted = false;
            context.SaveChanges();
            EventModel.SinchronizeWithCache(eventId);
            if (eventProfile.Parent != null)
            {
                EventModel.SinchronizeWithCache((int)eventProfile.Parent);
            }
        }

        #endregion //Changing params of Events

        #region Regions and Cities

        public string GetRegionTitleById(int id)
        {
            var region = context.Regions.FirstOrDefault(reg => reg.Id == id);
            if (region == null) return null;
            return region.Name;
        }


        private IQueryable<Region> GetCurrentRegions()
        {
            return GetCurrentEvents().SelectMany(eventProfile => eventProfile.Regions);
        }


        public string[] GetCurrentCitiesNames()
        {
            string[] cities;
            if (CacheManager.TryReadCitiesNames(out cities)) return cities;
            cities = GetCurrentRegions().SelectMany(region => region.Cities).Select(city => city.Name).ToArray();
            CacheManager.UpdateCitiesNames(cities);
            return cities;
        }


        public Dictionary<int, string> GetAllRegions(int? eventId = null, bool onlyRegionNotInEvent = false)
        {
            IQueryable<Region> regions = context.Regions;
            if (eventId != null)
            {
                var eventProfile = GetEventById((int) eventId);
                if (eventProfile != null)
                {
                    regions = onlyRegionNotInEvent ? context.Regions.AsEnumerable().Except(eventProfile.Regions.AsEnumerable()).AsQueryable() :
                                eventProfile.Regions.AsQueryable();
                }
            }
            return regions.OrderBy(r=>r.Name).ToDictionary(region=>region.Id,region=>region.Name);
        }


        public void RemoveRegion(string regionName)
        {
            var region = context.Regions.FirstOrDefault(reg => reg.Name == regionName);
            if (region == null) return;
            context.Regions.Remove(region);
            context.SaveChanges();
            CacheManager.UpdateCitiesNames(null);
        }

        public string[] GetCitiesForRegion(string regionName)
        {
            var region = context.Regions.FirstOrDefault(reg => reg.Name == regionName);
            if (region == null) return new string[]{};
            var cities = region.Cities;
            if (!cities.Any()) return new string[] { };
            return cities.OrderBy(city => city.Name).Select(city => city.Name).ToArray();
        }

        public void RemoveCityFromRegion(string regionName, string cityName)
        {
            var region = context.Regions.FirstOrDefault(reg => reg.Name == regionName);
            if (region == null || region.Cities==null) return;
            var city = region.Cities.FirstOrDefault(c => c.Name == cityName);
            if(city==null) return;
            region.Cities.Remove(city);
            city = context.Cities.FirstOrDefault(c => c.Name == cityName);
            if (city != null)
            {
                if (city.Regions == null || city.Regions.Count<=GlobalValuesAndStrings.MinRegionsForCityToRemove)
                {
                    context.Cities.Remove(city);
                }
            }
            context.SaveChanges();
            CacheManager.UpdateCitiesNames(null);
        }


        public void AddRegion(string regionName)
        {
            var region = context.Regions.FirstOrDefault(reg => reg.Name == regionName);
            if(region!=null) return;
            context.Regions.Add(new Region {Name = regionName});
            context.SaveChanges();
        }

        public void AddCity(string regionName, string cityName)
        {
            var region = context.Regions.FirstOrDefault(reg => reg.Name == regionName);
            if (region == null) return;
            var city = region.Cities.FirstOrDefault(c => c.Name == cityName);
            if (city != null) return;
            city = context.Cities.FirstOrDefault(c => c.Name == cityName);
            if (city == null)
            {
                city = new City {Name = cityName};
                context.Cities.Add(city);
            }
            region.Cities.Add(city);
            context.SaveChanges();
        }
        #endregion //Regions and Cities

        #region Options
        public string GetOption(string nameOption)
        {
            var option = context.Options.FirstOrDefault(o => o.Name == nameOption);
            return option != null ?option.Value:null;
        }

        public void SetOption(string nameOption, string value)
        {
            var option = context.Options.FirstOrDefault(o => o.Name == nameOption);
            if (option == null)
            {
                var newOption = new Option { Name = nameOption, Value = value };
                context.Options.Add(newOption);
            }
            else
            {
                option.Value = value;
            }
            context.SaveChanges();
        }
        #endregion //Options

        #region IdeaGroup and Platform

        public PlatformModel GetPlatformById(int id)
        {
            var platform = context.Platforms.FirstOrDefault(p => p.Id == id);
            if (platform == null) return null;
            return platform.ToModel();
        }


        public string GetTitleIdeaGroupById(int id)
        {
            var ideaGroup = context.GroupIdeas.FirstOrDefault(gr => gr.Id == id);
            if (ideaGroup == null) return null;
            return ideaGroup.Title;
        }


        private IQueryable<GroupIdea> GetCurrentIdeaGroups()
        {
            return GetCurrentEvents().SelectMany(ev => ev.GroupIdeas);
        }


        public Dictionary<int,PlatformModel> GetAllPlatform(int? eventId = null, bool withoutPlatformInThisEvent = false
            ,bool withDeleted = false)
        {
            var platforms = withDeleted ? context.Platforms : context.Platforms.Where(p => !p.IsDeleted);
            if (eventId == null)
            {
                return platforms.ToDictionary(platform => platform.Id, platform => platform.ToModel());                       
            }
            var eventProfile = GetEventById((int) eventId, true);
            if (eventProfile == null) return new Dictionary<int, PlatformModel>();
            if (withoutPlatformInThisEvent)
            {
                return platforms.AsEnumerable().Except(eventProfile.Platforms.AsEnumerable()).
                    ToDictionary(platform => platform.Id, platform => platform.ToModel());               
            }
            return withDeleted ? eventProfile.Platforms.ToDictionary(platform => platform.Id, platform => platform.ToModel())
                : eventProfile.Platforms.Where(p => !p.IsDeleted).ToDictionary(platform => platform.Id, platform => platform.ToModel());
        }


        public int? AddPlatform(PlatformModel model)
        {
            var platform = context.Platforms.FirstOrDefault(pl => pl.Name == model.Title);
            if (platform != null) return null;
            platform = new Platform
            {
                Name = model.Title,
                Description = model.Description,
                IsDeleted = false,
                Picture = null
            };
            context.Platforms.Add(platform);
            context.SaveChanges();
            return platform.Id;
        }


        public void ChangePlatform(PlatformModel model)
        {
            var platform = context.Platforms.FirstOrDefault(pl => pl.Id == model.Id);
            if (platform == null) return;
            platform.Name = model.Title;
            platform.Description = model.Description;
            context.SaveChanges();
            PlatformModel.SinchronizeWithCache(model.Id);
        }


        public void RemovePlatform(string platformName)
        {
            var platform = context.Platforms.FirstOrDefault(pl => pl.Name == platformName);
            if (platform == null) return;
            platform.IsDeleted = true;
            context.SaveChanges();
            PlatformModel.SinchronizeWithCache(platform.Id);

        }


        public void RemovePlatform(int platformId)
        {
            var platform = context.Platforms.FirstOrDefault(pl => pl.Id == platformId);
            if (platform == null) return;
            platform.IsDeleted = true;
            context.SaveChanges();
            PlatformModel.SinchronizeWithCache(platform.Id);
        }

        public void RestorePlatform(int platformId)
        {
            var platform = context.Platforms.FirstOrDefault(pl => pl.Id == platformId);
            if (platform == null) return;
            platform.IsDeleted = false;
            context.SaveChanges();
            PlatformModel.SinchronizeWithCache(platform.Id);
        }


        public Dictionary<int, string> GetAllIdeaGroups(int? eventId = null, bool withoutIdeaInThisEvent = false,
            bool deletedOnly = false)
        {
            var ideaGroups = (deletedOnly)
                ? context.GroupIdeas.Where(g => g.IsDeleted)
                : context.GroupIdeas.Where(g => !g.IsDeleted);
            if (eventId == null)
            {
                return ideaGroups.ToDictionary(ideas => ideas.Id, ideas => ideas.Title);
            }
            var eventProfile = GetEventById((int)eventId, true);
            if (eventProfile == null) return new Dictionary<int, string>();
            if (withoutIdeaInThisEvent)
            {
                return ideaGroups.AsEnumerable().Except(eventProfile.GroupIdeas.AsEnumerable()).
                    ToDictionary(ideas => ideas.Id, ideas => ideas.Title);
            }
            var ideaGroupForEvent = (deletedOnly)
                ? eventProfile.GroupIdeas.Where(g => g.IsDeleted)
                : eventProfile.GroupIdeas.Where(g => !g.IsDeleted);
            return ideaGroupForEvent.ToDictionary(ideas => ideas.Id, ideas => ideas.Title);
        }


        public void AddIdeaGroup(string ideaGroupName)
        {
            var ideaGroup = context.GroupIdeas.FirstOrDefault(gr => gr.Title == ideaGroupName);
            if (ideaGroup == null)
            {
                context.GroupIdeas.Add(new GroupIdea {Title = ideaGroupName});
            }
            else
            {
                if (ideaGroup.IsDeleted)
                {
                    ideaGroup.IsDeleted = false;
                }
                else
                {
                    return;
                }
            }
            context.SaveChanges();
        }


        public void RemoveIdeaGroup(string nameIdeaGroup)
        {
            var ideaGroup = context.GroupIdeas.FirstOrDefault(gr => gr.Title == nameIdeaGroup);
            if (ideaGroup == null) return;
            ideaGroup.IsDeleted = true;
            context.SaveChanges();
            CacheManager.UpdateGroupIdeaModel(ideaGroup.Id, string.Empty);
        }

        public void RestoreIdeaGroup(string nameIdeaGroup)
        {
            var ideaGroup = context.GroupIdeas.FirstOrDefault(gr => gr.Title == nameIdeaGroup);
            if (ideaGroup == null) return;
            ideaGroup.IsDeleted = false;
            context.SaveChanges();
            CacheManager.UpdateGroupIdeaModel(ideaGroup.Id, ideaGroup.Title);
        }


        public bool IsCorrectIdForGroupIdeas(int id)
        {
            return GetCurrentIdeaGroups().FirstOrDefault(group => group.Id == id) != null;
        }


        #endregion //IdeaGroup and Platform

        #region Materials
        public IQueryable<Material> GetMaterials(int? platformId = null, int? eventId = null, string search = "", 
             bool withDeleted = false)
        {
            IQueryable<Material> materials = (withDeleted)
                ? context.Materials
                : context.Materials.Where(mat => !mat.IsDeleted);
            if (platformId != null) materials = materials.Where(mat => mat.PlatformId == platformId);
            if (eventId != null) materials = materials.Where(mat => mat.EventProfiles.FirstOrDefault(ev=>ev.Id==eventId)!=null);
            if (!search.IsNullOrEmpty()) materials = materials.Filter(search);
            return materials;
        }

        public Material GetMaterialById(int materialId)
        {
            return context.Materials.FirstOrDefault(mat => mat.Id == materialId);
        }

        public int? AddMaterial(MaterialModel model)
        {
            var material = new Material
            {
                PlatformId = model.PlatformId,
                Title = model.Title,
                IsDeleted = false,
                Link = model.Link==GlobalValuesAndStrings.ExternalMaterialLink?model.Link:model.Link.LinkCorrect(),
                AddedDate = model.AddedDate
            };
            context.Materials.Add(material);
            context.SaveChanges();
            return material.Id;
        }

        public void ChangeMaterial(MaterialModel model)
        {
            var material = GetMaterialById(model.Id);
            if (material == null) return;
            material.Title = model.Title;
            material.PlatformId = model.PlatformId;
            material.Link = model.Link == GlobalValuesAndStrings.ExternalMaterialLink ? model.Link : model.Link.LinkCorrect();
            context.SaveChanges();
        }

        public void RemoveMaterial(int materialId)
        {
            var material = GetMaterialById(materialId);
            if (material == null) return;
            material.IsDeleted = true;
            context.SaveChanges();
        }

        public void RestoreMaterial(int materialId)
        {
            var material = GetMaterialById(materialId);
            if (material == null) return;
            material.IsDeleted = false;
            context.SaveChanges();
        }

        public void AddMaterialToEvent(int eventId, int materialId)
        {
            var material = GetMaterialById(materialId);
            var eventProfile = GetEventById(eventId);
            if (material == null || eventProfile == null) return;
            eventProfile.Materials.Add(material);
            context.SaveChanges();
        }

        public IQueryable<EventProfile> GetEventsForMaterial(int materialId)
        {
            var material = GetMaterialById(materialId);
            if (material == null) return new List<EventProfile>().AsQueryable();
            return material.EventProfiles.AsQueryable();
        }

        #endregion //Materials

        #region Award

        public List<int> GetAwardsForEvent(int eventId)
        {
            return context.Awards.Where(a => a.EventId == eventId && !a.IsDeleted).OrderBy(a => a.OrderInList)
                .Select(a=>a.Id).ToList();
        }

        public AwardModel GetAwardById(int awardId)
        {
            return context.Awards.FirstOrDefault(a => a.Id == awardId).ToModel();
        }

        public int AddAward(AwardModel model)
        {
            var award = new Award
            {
                Title = model.Title,
                IsDeleted = false,
                OrderInList = model.OrderInList,
                Subtitle = model.Subtitle,
                Description = model.Description,
                PostTitle = model.PostTitle,
                EventId = model.EventId
            };
            context.Awards.Add(award);
            context.SaveChanges();
            return award.Id;
        }

        public void UpdateAward(AwardModel model)
        {
            var award = context.Awards.FirstOrDefault(a => a.Id == model.Id);
            if (award == null)
            {
                AddAward(model);
                return;
            }
            award.Title = model.Title;
            award.IsDeleted = model.IsDeleted;
            award.OrderInList = model.OrderInList;
            award.Subtitle = model.Subtitle;
            award.Description = model.Description;
            award.PostTitle = model.PostTitle;
            award.EventId = model.EventId;
            context.SaveChanges();
            AwardModel.SinchronizeWithCache(model.Id);
        }

        public void RemoveAward(int awardId)
        {
            var award = context.Awards.FirstOrDefault(a => a.Id == awardId);
            if (award == null) return;
            award.IsDeleted = true;
            context.SaveChanges();
            AwardModel.SinchronizeWithCache(awardId);
        }

        public void SetPictureToAward(int awardId, int? pictureId)
        {
            var award = context.Awards.FirstOrDefault(a => a.Id == awardId);
            if (award == null) return;
            award.Picture = pictureId;
            context.SaveChanges();
            AwardModel.SinchronizeWithCache(awardId);
        }

        #endregion //Award

        #region Helpers

        private EventProfile GetEventById(int id, bool withDeleted = false)
        {
            if (withDeleted)
            {
                return context.EventProfiles.FirstOrDefault(ev => ev.Id == id);                
            }
            return context.EventProfiles.FirstOrDefault(ev => ev.Id == id && ev.IsDeleted!=true);
        }
        #endregion //Helpers

    }
}