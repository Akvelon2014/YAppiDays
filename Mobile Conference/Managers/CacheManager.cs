using System;
using System.Collections.Generic;
using System.Web;
using MobileConference.Models;

namespace MobileConference.Managers
{
    /// <summary>
    /// Class with all cache methods
    /// </summary>
    public static class CacheManager
    {
        #region Avatar

        public static void UpdateAvatar(int userId, PictureNameModel picture)
        {
            Update("avatar" + userId.ToString(), picture);
        }


        public static bool TryReadAvatar(int userId, out PictureNameModel picture)
        {
            picture = Read("avatar" + userId.ToString()) as PictureNameModel;
            return picture != null;
        }

        #endregion

        #region Avatar for Idea

        public static void UpdateIdeaAvatar(int ideaId, PictureNameModel picture)
        {
            Update("ideaAvatar" + ideaId.ToString(), picture);
        }


        public static bool TryReadIdeaAvatar(int ideaId, out PictureNameModel picture)
        {
            picture = Read("ideaAvatar" + ideaId.ToString()) as PictureNameModel;
            return picture != null;
        }

        #endregion

        #region Picture for Idea

        public static void UpdateIdeaPictures(int ideaId, List<PictureNameModel> pictures)
        {
            Update("ideaPhoto" + ideaId.ToString(), pictures);
        }


        public static bool TryReadIdeaPictures(int ideaId, out List<PictureNameModel> pictures)
        {
            pictures = Read("ideaPhoto" + ideaId.ToString()) as List<PictureNameModel>;
            return pictures != null;
        }

        public static void ClearPicturesForIdea(int ideaId)
        {
            UpdateIdeaPictures(ideaId, null);
        }

        #endregion

        #region IdeaProfile

        public static void UpdateIdeaModel(int ideaId, IdeasModel idea)
        {
            Update("idea" + ideaId.ToString(), idea);
        }


        public static bool TryReadIdeaModel(int ideaId,out IdeasModel idea)
        {
            idea = Read("idea" + ideaId.ToString()) as IdeasModel;
            return idea != null;
        }

        #endregion

        #region Company

        public static void UpdateCompany(int companyId, CompanyModel company)
        {
            Update("company" + companyId.ToString(), company);
        }


        public static bool TryReadCompany(int companyId, out CompanyModel company)
        {
            company = Read("company" + companyId.ToString()) as CompanyModel;
            return company != null;
        }

        #endregion

        #region EventProfile

        public static void UpdateEventModel(int eventId, EventModel model)
        {
            Update("event" + eventId.ToString(), model);
        }


        public static bool TryReadEventModel(int eventId, out EventModel model)
        {
            model = Read("event" + eventId.ToString()) as EventModel;
            return model != null;
        }

        #endregion
        
        #region UserProfile

        public static void UpdateUserProfileModel(string login, ProfileModel profile)
        {
            Update("user-" + login, profile);
        }


        public static bool TryReadUserProfileModel(string login, out ProfileModel profile)
        {
            profile = Read("user-" + login) as ProfileModel;
            return profile != null;
        }

        #endregion

        #region GroupIdeaProfile

        public static void UpdateGroupIdeaModel(int groupId, string title)
        {
            Update("groupIdea" + groupId.ToString(), title);
        }


        public static bool TryReadGroupIdeaModel(int groupId, out string title)
        {
            title = Read("groupIdea" + groupId) as string;
            return title != null;
        }

        #endregion

        #region Platform

        public static void UpdatePlatform(int platformId, PlatformModel model)
        {
            Update("platform" + platformId.ToString(), model);
        }


        public static bool TryReadPlatform(int platformId, out PlatformModel model)
        {
            model = Read("platform" + platformId) as PlatformModel;
            return model != null;
        }

        #endregion

        #region Award

        public static void UpdateAward(int awardId, AwardModel model)
        {
            Update("award" + awardId.ToString(), model);
        }


        public static bool TryReadAward(int awardId, out AwardModel model)
        {
            model = Read("award" + awardId) as AwardModel;
            return model != null;
        }

        #endregion

        #region AwardList

        public static void UpdateAwardList(int eventId, List<int> model)
        {
            Update("awardList" + eventId.ToString(), model);
        }


        public static bool TryReadAwardList(int eventId, out List<int> model)
        {
            model = Read("awardList" + eventId) as List<int>;
            return model != null;
        }

        #endregion

        #region RegionTitle

        public static void UpdateRegionTitle(int regionId, string title)
        {
            Update("region" + regionId.ToString(), title);
        }


        public static bool TryReadRegionTitle(int regionId, out string title)
        {
            title = Read("region" + regionId) as string;
            return title != null;
        }

        #endregion

        #region EventTypeProfile

        public static void UpdateEventTypeModel(int eventTypeId, string title)
        {
            Update("typeEvent" + eventTypeId, title);
        }


        public static bool TryReadEventTypeModel(int eventTypeId, out string title)
        {
            title = Read("typeEvent" + eventTypeId) as string;
            return title != null;
        }

        #endregion

        #region LastDate For Comment

        public static void UpdateLastDateCommentForIdea(int ideaId, DateTime lastDate)
        {
            Update("lastDate" + ideaId, lastDate);
        }


        public static bool TryReadLastDateCommentForIdea(int ideaId, out DateTime? lastDate)
        {
            lastDate = Read("lastDate" + ideaId) as DateTime?;
            return lastDate != null;
        }


        public static void ClearLastDateCommentForIdea(int ideaId)
        {
            Update("lastDate"+ideaId, null);
        }

        #endregion

        #region CitiesName

        public static void UpdateCitiesNames(string[] cityNames)
        {
            Update("citiesName", cityNames);
        }


        public static bool TryReadCitiesNames(out string[] cityNames)
        {
            cityNames = Read("citiesName") as string[];
            return cityNames != null;
        }
        #endregion

        #region News

        public static void UpdateNews(List<NewsModel> news)
        {
            Update("news", news);
        }


        public static bool TryReadNews(out List<NewsModel> news)
        {
            news = Read("news") as List<NewsModel>;
            return news != null;
        }
        #endregion

        #region List of realized platform statuses

        public static void UpdatePlatformStatusesList(Dictionary<int,string> list)
        {
            Update("platStatus", list);
        }


        public static bool TryReadPlatformStatusesList(out Dictionary<int, string> list)
        {
            list = Read("platStatus") as Dictionary<int, string>;
            return list != null;
        }
        #endregion

        #region List of Skills

        public static void UpdateSkillsList(List<string> list)
        {
            Update("skills", list);
        }


        public static bool TryReadSkillsList(out List<string> list)
        {
            list = Read("skills") as List<string>;
            return list != null;
        }
        #endregion


        #region Helper

        private static void Update(string name, object value)
        {
            if (value == null)
            {
                HttpContext.Current.Cache.Remove(name);
                return;
            }
            HttpContext.Current.Cache[name] = value;
        }

        private static object Read(string name)
        {
            if (HttpContext.Current.Cache.Get(name)==null || HttpContext.Current.Cache[name] == null)
            {
                return null;
            }
            return HttpContext.Current.Cache[name];
        }

#endregion

    }
}