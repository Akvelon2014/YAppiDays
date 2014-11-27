using System;
using System.Collections.Generic;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Models;
using PagedList;

namespace MobileConference.Interface
{
    public interface IIdeasRepository
    {
        IdeasModel GetIdeasModel(int ideaId, bool onlyCurrentEvents = true);
        int IdeasWithoutMentorCount();
        IPagedList<Idea> IdeasWithoutMentor(int page = GlobalValuesAndStrings.FirstPageCount, string search = "");
        IPagedList<Idea> GetAllIdeas(int page = GlobalValuesAndStrings.FirstPageCount, string search = ""
            , bool onlyCurrentEvents = true, bool withDeleted = false, int? platform = null, int? ideasGroup = null
            , int? take = null, List<int> topIds = null, int withEmptyProjects = 0, bool remix = false);

        string GetGroupIdeasTitle(int groupIdeaId);

        /// <summary>
        /// Add and save idea in database
        /// </summary>
        /// <returns>is adding successfull</returns>
        bool AddIdeaToDb(string title, string description, int? groupIdea, int eventId, string userLogin);

        void UpdateIdea(IdeasModel model);

        /// <summary>
        /// Add user to idea as member
        /// </summary>
        /// <returns>is adding successfull</returns>
        bool AddMemberToIdea(int ideaId, string userLogin, StudentStatusInGroup status);

        void RemoveMemberFromIdea(int ideaId, string userLogin, StudentStatusInGroup? status = null, bool isRecursiveRemoving = false);

        /// <summary>
        /// Remove all member with status from idea
        /// </summary>
        void RemoveStatusFromIdea(int ideaId, StudentStatusInGroup status);
        void SetLeader(int ideaId, string userLogin);
        void SendRequestForIdea(int ideaId, string userLogin);
        void Invite(int ideaId, string userLogin);

        void SetMentor(int ideaId, string userLogin);
        void UnsetMentor(int ideaId, string userLogin);

        void RemoveIdea(int ideaId);
        void RestoreIdea(int ideaId);

        int? AddComment(CommentModel commentModel);
        void UpdateComment(CommentModel commentModel);
        IPagedList<Comment> GetCommentsForLink(int linkId, CommentModelType typeComment
            , int page = GlobalValuesAndStrings.FirstPageCount);
        int GetCommentsCountForLink(int linkId, CommentModelType typeComment);
        CommentModel GetComment(int commentId);
        int? GetIdForCommentsType(string typeComment);
        int? GetTextForMateralAsCommentId(int materialId);
        DateTime? GetLastUpdateForIdeaComments(int linkId, CommentModelType typeComment);
        void RemoveComment(int commentId, string loginUser);

        Dictionary<int, string> GetPlatformStatuses();
        string GetPlatformStatusTitle(StatusByRealizedPlatform idPlatformStatus);
        void AddPlatformToIdea(int ideaId, int platformId);
        void RemovePlatformFromIdea(int ideaId, int platformId);
        void ChangePlatformStatusInIdea(int ideaId, int platformId, StatusByRealizedPlatform status);
    }
}