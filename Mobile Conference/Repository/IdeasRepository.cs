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
    /// Repository for manage Ideas in database
    /// </summary>
    public class IdeasRepository : IIdeasRepository
    {
        private const string unknownPlatformStatus = "не известно";

        private Entities context
        {
            get { return ContextEF.Context; }
        }


        #region Getting methods (Ideas and User)

        public IPagedList<Idea> GetAllIdeas(int page = GlobalValuesAndStrings.FirstPageCount, string search = "",
            bool onlyCurrentEvents = true, bool withDeleted = false, int? platform = null, int? ideasGroup = null
            , int? takeMaxElements = null, List<int> topIds = null, int withEmptyProjects = 0, bool remix = false)
        {
            var ideas = GetAllIdeasAsQuery(onlyCurrentEvents, withDeleted);
            if (platform != null)
            {
                ideas = ideas.Where(i => i.RealizedPlatforms.FirstOrDefault(p => p.PlatformId == platform) != null);
            }
            if (ideasGroup != null)
            {
                ideas = ideas.Where(i => i.GroupIdeaId == ideasGroup);
            }
            if (!search.IsNullOrEmpty()) ideas = ideas.Filter(search);
            IOrderedQueryable<Idea> orderedIdeas;
            if (topIds != null && topIds.Any())
            {
                orderedIdeas = ideas.OrderByDescending(i => topIds.Contains(i.Id));
                orderedIdeas = (remix) ? orderedIdeas.ThenBy(x => Guid.NewGuid()) 
                    : orderedIdeas.ThenByDescending(x => x.CreatedDate);                
            }
            else
            {
                orderedIdeas = (remix) ? ideas.OrderBy(x => Guid.NewGuid()) : ideas.OrderByDescending(x => x.CreatedDate);
            }
            
            return orderedIdeas.ToPagedList(page, ((takeMaxElements==null)?OptionModel.Current.ItemPerPage - withEmptyProjects
                :(int)takeMaxElements));
        }


        public IdeasModel GetIdeasModel(int ideaId, bool onlyCurrentEvents = true)
        {
            Idea idea = IdeaById(ideaId, onlyCurrentEvents);
            return idea.ToModel();
        }


        public IPagedList<Idea> IdeasWithoutMentor(int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            var ideas = GetAllIdeasAsQuery().Where(idea => idea.Mentor == null);
            if (!search.IsNullOrEmpty()) ideas = ideas.Filter(search);
            return ideas.OrderByDescending(idea => idea.Id).ToPagedList(page, OptionModel.Current.ItemPerPage);
        }

        public int IdeasWithoutMentorCount()
        {
            return GetAllIdeasAsQuery().Count(idea => idea.Mentor == null);
        }

        public string GetGroupIdeasTitle(int groupIdeaId)
        {
            var group = context.GroupIdeas.FirstOrDefault(gr => gr.Id == groupIdeaId);
            return (group != null && !group.IsDeleted)? group.Title : string.Empty;
        }

        #endregion // Getting methods (Ideas and User)

        #region CRUD for Idea and Member in DB
        public bool AddIdeaToDb(string title, string description, int? groupIdea, int eventId, string userLogin)
        {
            var idea = new Idea
            {
                Title = title,
                Description = description,
                GroupIdeaId = groupIdea,
                EventId = eventId,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };
            context.Ideas.Add(idea);
            context.SaveChanges();
            if (AddMemberToIdea(idea.Id, userLogin, StudentStatusInGroup.Creator) &&
                AddMemberToIdea(idea.Id, userLogin, StudentStatusInGroup.Member))
            {
                SetLeader(idea.Id, userLogin);
                IdeasModel.SinchronizeWithCache(idea.Id);
                ProfileModel.SinchronizeWithCache(userLogin);
                return true;
            }
            return false;
        }


        public void UpdateIdea(IdeasModel model)
        {
            var idea = IdeaById(model.Id);
            if (idea == null) return;
            idea.Title = model.Title;
            idea.Description = model.Description;
            idea.GroupIdeaId = model.GroupIdea;
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(idea.Id);

        }

        public void RemoveIdea(int ideaId)
        {
            Idea idea = IdeaById(ideaId, false);
            if (idea == null) return;
            idea.IsDeleted = true;
            string mentor = null;
            if (idea.Mentor != null) mentor = idea.Mentor.UserProfile.Login;
            idea.Mentor = null;
            var members = GetStudentsForIdeas(idea);
            if (members.Any())
            {
                foreach (Student student in members)
                {
                    RemoveMemberFromIdea(idea.Id,student.UserProfile.Login);
                }
            }
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(ideaId);
            if (mentor!=null) ProfileModel.SinchronizeWithCache(mentor);
        }


        public void RestoreIdea(int ideaId)
        {
            Idea idea = IdeaById(ideaId,false, true);
            if (idea == null) return;
            idea.IsDeleted = false;
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(ideaId);
        }


        public bool AddMemberToIdea(int ideaId, string userLogin, StudentStatusInGroup status)
        {
            Idea idea = IdeaById(ideaId);
            Student student = context.Students.FirstOrDefault(user => user.UserProfile.Login == userLogin);
            if (student == null || idea == null) return false;
            if (IsStatusForUserInIdea(idea.Id, userLogin, status)) return true;
            var group = new StudentGroup {IdeaId = idea.Id, StatusStudentId = (int)status, StudentId = student.Id};
            if(!idea.StudentGroups.Contains(group)) idea.StudentGroups.Add(group);
            if (status == StudentStatusInGroup.Member)
            {
                RemoveMemberFromIdea(ideaId, userLogin, StudentStatusInGroup.ExMember);
            }
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(idea.Id);
            ProfileModel.SinchronizeWithCache(userLogin);
            return true;
        }


        public void RemoveMemberFromIdea(int ideaId, string userLogin, StudentStatusInGroup? status = null, 
            bool isRecursiveRemoving = false)
        {
            Idea idea = IdeaById(ideaId);
            Student student = context.Students.FirstOrDefault(user => user.UserProfile.Login == userLogin);
            if (student == null || idea == null) return;
            StudentStatusInGroup[] statuses = GetStatusesInIdea(idea.Id, userLogin);
            if(statuses==null || !statuses.Any()) return;
            if (status == null)
            {
                var isLeader = false;
                foreach (StudentStatusInGroup statusInIdea in statuses)
                {
                    if (statusInIdea == StudentStatusInGroup.Leader || statusInIdea == StudentStatusInGroup.Member ||
                        statusInIdea == StudentStatusInGroup.Wished || statusInIdea == StudentStatusInGroup.Invited)
                    {
                        RemoveMemberFromIdea(ideaId, userLogin, statusInIdea,true);
                        if (statusInIdea == StudentStatusInGroup.Leader) isLeader = true;
                    }
                }

                //if leader was removed, new leader must be appointed
                if (isLeader && !idea.IsDeleted)
                {
                    var newLeader = GetStudentsForIdeas(idea).FirstOrDefault();
                    if (newLeader != null)
                    {
                        SetLeader(ideaId, newLeader.UserProfile.Login);
                    }
                }
            }
            else
            {
                var groups = idea.StudentGroups;
                if (groups == null) return;
                StudentGroup group = groups.FirstOrDefault(gr => gr.StatusStudent!=null && 
                    gr.StatusStudent.Id == (int)status && gr.Student.UserProfile.Login == userLogin);
                if (group != null)
                {
                    idea.StudentGroups.Remove(group);
                    context.SaveChanges();
                    if (status == StudentStatusInGroup.Member)
                    {
                        AddMemberToIdea(ideaId, userLogin, StudentStatusInGroup.ExMember);
                        if (IsIdeaEmpty(idea)) RemoveIdea(ideaId);
                    }
                }
            }
            
            if (!isRecursiveRemoving)
            {
                IdeasModel.SinchronizeWithCache(ideaId);
                ProfileModel.SinchronizeWithCache(userLogin);
            }
        }


        public void RemoveStatusFromIdea(int ideaId, StudentStatusInGroup status)
        {
            var idea = context.Ideas.FirstOrDefault(i => i.Id == ideaId);
            if (idea == null) return;
            var groups = idea.StudentGroups.Where(group => group.StatusStudentId == (int)status).ToArray();
            foreach (var group in groups)
            {
                string login = group.Student.UserProfile.Login;
                RemoveMemberFromIdea(ideaId, login,(StudentStatusInGroup)group.StatusStudent.Id,true);
                ProfileModel.SinchronizeWithCache(login);
            }
            IdeasModel.SinchronizeWithCache(ideaId);
        }


        public void SetLeader(int ideaId, string userLogin)
        {
            if (!IsStatusForUserInIdea(ideaId, userLogin, StudentStatusInGroup.Member)) return;
            RemoveStatusFromIdea(ideaId, StudentStatusInGroup.Leader);
            AddMemberToIdea(ideaId, userLogin, StudentStatusInGroup.Leader);
        }


        public void SetMentor(int ideaId, string userLogin)
        {
            var userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            if (userRepository.GetRoleByUserLogin(userLogin) != RoleName.Mentor) return;
            Idea idea = IdeaById(ideaId);
            if (idea.Mentor != null) return;
            idea.MentorId = userRepository.GetUserByLogin(userLogin).Id;
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(ideaId);
            ProfileModel.SinchronizeWithCache(userLogin);
        }


        public void UnsetMentor(int ideaId, string userLogin)
        {
            var userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            if (userRepository.GetRoleByUserLogin(userLogin) != RoleName.Mentor) return;
            UserProfile user = userRepository.GetUserByLogin(userLogin);
            Idea idea = context.Ideas.FirstOrDefault(i=>i.Id == ideaId);
            if (idea==null || idea.MentorId != user.Id) return;
            idea.MentorId = null;
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(ideaId);
            ProfileModel.SinchronizeWithCache(userLogin);
        }


        public void SendRequestForIdea(int ideaId, string userLogin)
        {
            Idea idea = IdeaById(ideaId);
            Student student = context.Students.FirstOrDefault(user => user.UserProfile.Login == userLogin);
            if (student == null || idea == null) return;
            if (GetIdeasByUser(student.UserProfile.Login).Any()) return;
            StudentStatusInGroup[] statuses = GetStatusesInIdea(ideaId, userLogin);
            if (statuses != null && statuses.Any())
            {
                if (statuses.Contains(StudentStatusInGroup.Member)) return;
                if (statuses.Contains(StudentStatusInGroup.Invited))
                {
                    RemoveMemberFromIdea(ideaId, userLogin, StudentStatusInGroup.Invited);
                    AddMemberToIdea(ideaId, userLogin, StudentStatusInGroup.Member);
                    return;
                }
            }
            if (IsStatusForUserInIdea(ideaId, userLogin, StudentStatusInGroup.Member)) return;
            AddMemberToIdea(ideaId, userLogin, StudentStatusInGroup.Wished);
        }


        public void Invite(int ideaId, string userLogin)
        {
            StudentStatusInGroup[] statuses = GetStatusesInIdea(ideaId, userLogin);
            if (statuses != null && statuses.Contains(StudentStatusInGroup.Member)) return;
            if (statuses!=null && statuses.Contains(StudentStatusInGroup.Wished))
            {
                RemoveMemberFromIdea(ideaId,userLogin,StudentStatusInGroup.Wished);
                AddMemberToIdea(ideaId, userLogin, StudentStatusInGroup.Member);
            }
            else
            {
                AddMemberToIdea(ideaId, userLogin, StudentStatusInGroup.Invited);                
            }
        }
        #endregion //CRUD for Idea and Member in DB

        #region Working with Comments

        public int? AddComment(CommentModel commentModel)
        {
            UserProfile user = context.UserProfiles.FirstOrDefault(u => u.Login == commentModel.UserLogin);
            if (user == null || commentModel.Message == null) return null;
            if (commentModel.Message.IsNullOrEmpty()) commentModel.Message = " ";
            var comment = new Comment
            {
                Type = (int)commentModel.Type,
                Date = commentModel.CreationDate,
                User = user.Id,
                LinkId = commentModel.LinkId,
                Message = commentModel.Message,
                IsDeleted = false
            };
            context.Comments.Add(comment);
            context.SaveChanges();
            if (commentModel.Type == CommentModelType.Chat)
            {
                CacheManager.UpdateLastDateCommentForIdea(commentModel.LinkId, commentModel.CreationDate);
            }
            return comment.Id;
        }


        public void UpdateComment(CommentModel commentModel)
        {
            var comment = context.Comments.FirstOrDefault(c => c.Id == commentModel.Id);
            if (comment == null || comment.Type != (int)commentModel.Type) return;
            comment.LinkId = commentModel.LinkId;
            comment.Message = commentModel.Message;
            comment.IsDeleted = commentModel.IsDeleted;
            context.SaveChanges();
            if (commentModel.Type == CommentModelType.Chat)
            {
                CacheManager.UpdateLastDateCommentForIdea(commentModel.LinkId, commentModel.CreationDate);
            }
        }


        public IPagedList<Comment> GetCommentsForLink(int linkId, CommentModelType typeComment, 
            int page = GlobalValuesAndStrings.FirstPageCount)
        {
            return context.Comments.Where(comment => comment.CommentType!=null && comment.CommentType.Id == (int)typeComment &&
                        !comment.IsDeleted && comment.LinkId == linkId)
                .OrderByDescending(com => com.Date)
                .ToPagedList(page, OptionModel.Current.ItemPerPage);
        }


        public int? GetTextForMateralAsCommentId(int materialId)
        {
            var comment= context.Comments.FirstOrDefault(c => c.CommentType.Id == (int) CommentModelType.MaterialText
                                                                 && c.LinkId == materialId);
            return comment != null ? (int?)comment.Id : null;
        }

        public DateTime? GetLastUpdateForIdeaComments(int ideaId, CommentModelType typeComment)
        {
            DateTime? date;
            if (CacheManager.TryReadLastDateCommentForIdea(ideaId, out date)) return date;
            var lastComment = context.Comments.Where(comment => comment.CommentType.Id == (int)typeComment && !comment.IsDeleted 
                && comment.LinkId == ideaId).OrderByDescending(comment => comment.Date).FirstOrDefault();
            date = lastComment == null ? DateTime.MinValue : lastComment.Date;
            CacheManager.UpdateLastDateCommentForIdea(ideaId, (DateTime) date);            
            return date;
        }

        public void RemoveComment(int commentId, string loginUser)
        {
            var comment = context.Comments.FirstOrDefault(com => com.Id == commentId);
            if (comment == null) return;
            var userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            RoleName role = userRepository.GetRoleByUserLogin(loginUser);
            if (role != RoleName.Administrator && comment.UserProfile.Login != loginUser)
            {
                return;
            }
            if (comment.Type == (int) CommentModelType.Chat)
            {
                var imageRepository = ContainerDI.Container.Resolve<IImageRepository>();
                imageRepository.DeleteImageForComment(commentId);
            }
            comment.IsDeleted = true;
            context.SaveChanges();
            if (comment.Type == (int)CommentModelType.Chat)
            {
                CacheManager.UpdateLastDateCommentForIdea(comment.LinkId, DateTime.Now);                       
            }
        }

        
        public int GetCommentsCountForLink(int linkId, CommentModelType typeComment)
        {
            return context.Comments.Count(c => !c.IsDeleted && c.Type == (int) typeComment && c.LinkId == linkId);
        }


        public int? GetIdForCommentsType(string typeComment)
        {
            var comment = context.CommentTypes.FirstOrDefault(t => t.Title == typeComment);
            return (comment == null) ? (int?) null : comment.Id;
        }

        public CommentModel GetComment(int commentId)
        {
            var comment = context.Comments.FirstOrDefault(c => c.Id == commentId);
            if (comment == null) return null;
            return comment.ToModel();
        }

        #endregion //Working with Comments

        #region Realized Platform Status for Idea

        public Dictionary<int, string> GetPlatformStatuses()
        {
            Dictionary<int, string> statuses;
            if (CacheManager.TryReadPlatformStatusesList(out statuses)) return statuses;
            statuses = context.StatusRealizedPlatforms.
                ToDictionary(status => status.Id, status => status.Title);
            CacheManager.UpdatePlatformStatusesList(statuses);
            return statuses;
        }

        public string GetPlatformStatusTitle(StatusByRealizedPlatform idPlatformStatus)
        {
            Dictionary<int, string> statuses;
            if (!CacheManager.TryReadPlatformStatusesList(out statuses)) statuses = GetPlatformStatuses();
            if (!statuses.ContainsKey((int)idPlatformStatus)) return unknownPlatformStatus;
            return statuses[(int)idPlatformStatus];
        }

        public void AddPlatformToIdea(int ideaId, int platformId)
        {
            var idea = IdeaById(ideaId);
            if(idea == null) return;
            var realizedPlatfrom = idea.RealizedPlatforms.FirstOrDefault(i => i.PlatformId == platformId);
            if (realizedPlatfrom != null) return;  
            idea.RealizedPlatforms.Add(new RealizedPlatform{IdeaId = ideaId,PlatformId = platformId,
                Status = (int)StatusByRealizedPlatform.Started});
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(ideaId);
        }

        public void RemovePlatformFromIdea(int ideaId, int platformId)
        {
            var idea = IdeaById(ideaId);
            if (idea == null) return;
            var realizedPlatfrom = idea.RealizedPlatforms.FirstOrDefault(i=>i.PlatformId == platformId);
            if (realizedPlatfrom == null) return;
            context.RealizedPlatforms.Remove(realizedPlatfrom);
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(ideaId);
        }

        public void ChangePlatformStatusInIdea(int ideaId, int platformId, StatusByRealizedPlatform status)
        {
            var idea = IdeaById(ideaId);
            if (idea == null) return;
            var realizedPlatfrom = context.RealizedPlatforms.FirstOrDefault(i => i.PlatformId == platformId && i.IdeaId == ideaId);
            //var realizedPlatfrom = idea.RealizedPlatforms.FirstOrDefault(i => i.PlatformId == platformId);
            if (realizedPlatfrom == null) return;
            if (GetPlatformStatusTitle(status) == unknownPlatformStatus) return;
            realizedPlatfrom.Status = (int) status;
            context.SaveChanges();
            IdeasModel.SinchronizeWithCache(ideaId);
        }

        #endregion //Realized Platform Status for Idea

        #region Help methods
        private bool IsStatusForUserInIdea(int ideaId, string userLogin, StudentStatusInGroup? status = null)
        {
            Student student = context.Students.FirstOrDefault(user => user.UserProfile.Login == userLogin);
            if (student == null) return false;
            StudentStatusInGroup[] statuses = GetStatusesInIdea(ideaId, userLogin);
            return (status == null) ? statuses.Any() : statuses.Contains((StudentStatusInGroup)status);
        }


        private bool IsIdeaEmpty(Idea idea)
        {
            return idea.StudentGroups.
                FirstOrDefault(group => group.StatusStudentId == (int)StudentStatusInGroup.Member) == null;
        }


        /// <summary>
        /// Method for get Query to get real list (please, using pagination, because this list may be large)
        /// </summary>
        /// <returns>Query to get ideas list</returns>
        private IQueryable<Idea> GetAllIdeasAsQuery(bool onlyCurrentEvents = true, bool withDeleted = false)
        {
            var eventRepository = ContainerDI.Container.Resolve<IEventRepository>();

            IQueryable<Idea> ideaCollection;
            if (onlyCurrentEvents)
            {
                var currentEvents = eventRepository.GetCurrentEvents();
                if (currentEvents == null || !currentEvents.Any()) return null;
                ideaCollection = currentEvents.SelectMany(idea => idea.Ideas);
            }
            else
            {
                ideaCollection = context.Ideas;
            }
            return (withDeleted)?ideaCollection:ideaCollection.Where(idea => !idea.IsDeleted);
        }


        private Idea IdeaById(int ideaId, bool onlyCurrentEvents = false, bool withDeleted = false)
        {
            return GetAllIdeasAsQuery(onlyCurrentEvents,withDeleted).FirstOrDefault(idea => idea.Id == ideaId);
        }


        private StudentStatusInGroup[] GetStatusesInIdea(int ideaId, string userLogin)
        {
            var userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            UserProfile user = userRepository.GetUserByLogin(userLogin);
            if (user == null) return null;
            RoleName role = userRepository.GetRoleByUserLogin(userLogin);
            if (role == RoleName.Mentor)
            {
                if (user.Mentor == null) return null;
                var idea = user.Mentor.Ideas.FirstOrDefault(i => i.Id == ideaId);
                return (idea != null) ? new[] { StudentStatusInGroup.Mentor } : null;
            }
            if (role != RoleName.Student) return null;
            if (user.Student == null) return null;
            return
                user.Student.StudentGroups.Where(group => group.Idea.Id == ideaId && group.Student.Id == user.Id).
                    Select(group => (StudentStatusInGroup)group.StatusStudentId).ToArray();
        }


        private IQueryable<Student> GetStudentsForIdeas(Idea idea)
        {
            return idea.StudentGroups.Where(group => group.StatusStudentId == (int)StudentStatusInGroup.Member).
                Select(group => group.Student).Distinct().AsQueryable();
        }


        private IQueryable<Idea> GetIdeasByUser(string userName, bool onlyCurrentEvents = true, bool withWishes = false)
        {
            var userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            IQueryable<Idea> localIdea = GetAllIdeasAsQuery(onlyCurrentEvents);

            IQueryable<Idea> ideas = null;
            UserProfile user = userRepository.GetUserByLogin(userName);
            if (user == null) return null;
            switch (userRepository.GetRoleByUserLogin(userName))
            {
                case RoleName.Student:
                    if (user.Student == null || user.Student.StudentGroups == null) return null;
                    if (withWishes)
                    {
                        ideas = localIdea.Where(idea => idea.StudentGroups.
                            FirstOrDefault(group => group.Student.UserProfile.Login == userName &&
                            (group.StatusStudentId == (int)StudentStatusInGroup.Member ||
                            group.StatusStudentId == (int)StudentStatusInGroup.Wished ||
                            group.StatusStudentId == (int)StudentStatusInGroup.Invited)) != null).Distinct();
                        break;
                    }
                    ideas = localIdea.Where(idea => idea.StudentGroups.
                        FirstOrDefault(group => group.Student.UserProfile.Login == userName &&
                            group.StatusStudentId == (int)StudentStatusInGroup.Member) != null).Distinct();
                    break;
                case RoleName.Mentor:
                    if (user.Mentor == null || user.Mentor.Ideas == null) return null;
                    ideas = localIdea.Where(i => i.Mentor.UserProfile.Login == userName);
                    break;
            }

            return ideas;
        }
        #endregion //Help methods

    }
}