using System;
using System.Collections.Generic;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Models;
using System.Linq;
using PagedList;

namespace MobileConference.Interface
{
    public interface IEventRepository
    {
        IPagedList<EventProfile> GetAllEvents(DateTime? dateFrom = null, DateTime? dateTo = null,
            int page = GlobalValuesAndStrings.FirstPageCount, bool withDeleted = false, bool onlyRoot = false);
        IPagedList<EventProfile> GetNeededLevelEvents(int rootId, int level, EventListType eventListType,
            bool withDeleted = false, int page = GlobalValuesAndStrings.FirstPageCount);
        IPagedList<EventProfile> GetEventForStage(int stageId, int page = GlobalValuesAndStrings.FirstPageCount);
        EventModel GetEventModel(int eventId, bool withDeleted = false);
        
        int AddEvent(EventModel model);
        void UpdateEventModel(EventModel model);
        void RemoveEvent(int eventId);
        void RestoreEvent(int eventId);
        void SetTypeForEvent(int eventId, int eventType);
        void AddGroupIdeaToEvent(int eventId, int groupIdea);
        void RemoveGroupIdeaFromEvent(int eventId, int groupIdea);
        void RemovePlatformFromEvent(int eventId, int platformId);
        void AddRegionToEvent(int eventId, int regionId);
        void RemoveRegionFromEvent(int eventId, int regionId);

        /// <summary>
        /// Get region (all in database or for needed event)
        /// </summary>
        /// <param name="eventId">id of needed region (if it's null, then to get all regions in database)</param>
        /// <param name="without">if it's true, then to get all regions, except in needed regions</param>
        /// <returns>Title of region</returns>
        Dictionary<int, string> GetAllRegions(int? eventId = null, bool without = false);
        Dictionary<int, PlatformModel> GetAllPlatform(int? eventId = null, bool without = false, bool withDeleted = false);
        Dictionary<int, string> GetAllIdeaGroups(int? eventId = null, bool without = false, bool deletedOnly = false);
        Dictionary<int, string> GetEventTypes();
        List<NewsModel> GetNews(RoleName roles);
        IPagedList<News> GetAllNews(int page = GlobalValuesAndStrings.FirstPageCount, DateTime? dateFrom = null
            , DateTime? dateTo = null, bool withDeleted = false);
        News GetNewsById(int id);
        List<NewsModel> GetLastNews();  
        int[] GetChildsIdForEvent(int eventId, bool withDeleted = false);
        string[] GetCitiesForRegion(string region);
        string[] GetCurrentCitiesNames();
        string GetRegionTitleById(int id);
        PlatformModel GetPlatformById(int id);
        string GetTitleIdeaGroupById(int id);
        string GetEventTypeTitle(int eventTypeId);
        bool IsCorrectIdForGroupIdeas(int id);

        void AddRegion(string region);
        void RemoveRegion(string region);
        void RemoveCityFromRegion(string region, string city);
        void AddCity(string region, string city);

        int? AddPlatform(PlatformModel model);
        void AddPlatformToEvent(int eventId, int platformId);
        void ChangePlatform(PlatformModel model);
        void RemovePlatform(string namePlatform);
        void RemovePlatform(int platformId);
        void RestorePlatform(int platformId);

        IQueryable<Material> GetMaterials(int? platformId = null, int? eventId = null, string search = "", 
            bool withDeleted = false); 
        Material GetMaterialById(int materialId);
        IQueryable<EventProfile> GetEventsForMaterial(int materialId);
        int? AddMaterial(MaterialModel model);
        void ChangeMaterial(MaterialModel model);
        void RemoveMaterial(int materialId);
        void RestoreMaterial(int materialId);
        void AddMaterialToEvent(int eventId, int materialId);

        void AddIdeaGroup(string nameIdeaGroup);
        void RemoveIdeaGroup(string nameIdeaGroup);
        void RestoreIdeaGroup(string nameIdeaGroup);

        List<int> GetAwardsForEvent(int eventId);
        AwardModel GetAwardById(int awardId);
        int AddAward(AwardModel model);
        void UpdateAward(AwardModel model);
        void RemoveAward(int awardId);
        void SetPictureToAward(int awardId, int? pictureId);

        void AddNews(NewsModel model);
        void RemoveNews(int newsId);
        void RestoreNews(int newsId);
        void UpdateNews(NewsModel model);

        List<ProfileModel> GetExpertForEvent(int eventId);

        string GetOption(string nameOption);
        void SetOption(string nameOption, string value);

        /// <summary>
        /// only for use in repositories
        /// </summary>
        IQueryable<EventProfile> GetCurrentEvents();
    }
}
