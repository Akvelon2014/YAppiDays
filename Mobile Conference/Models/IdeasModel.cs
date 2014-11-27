using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Managers;

namespace MobileConference.Models
{
    public class IdeasModel
    {
        private static readonly IIdeasRepository ideasRepository;

        static IdeasModel()
        {
            ideasRepository = ContainerDI.Container.Resolve<IIdeasRepository>();
        }

        public IdeasModel()
        {
            RealizedPlatformWithStatus = new Dictionary<int, int>();
        }

        private Dictionary<StudentStatusInGroup,List<string>> userDictionary =
                                                    new Dictionary<StudentStatusInGroup, List<string>>();

        [Required(ErrorMessage = "Введите название")]
        [StringLength(200, ErrorMessage = "Пожалуйста сократите название проекта до 200 символов")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Введите описание")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Группа")]
        public int? GroupIdea { get; set; }

        public bool IsDeleted { get; set; }
        public int Id { get; set; }

        public int EventId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Dictionary<int, int> RealizedPlatformWithStatus { get; set; }

        public int NonRemovedRealizedPlatformCount
        {
            get { return RealizedPlatformWithStatus.Select(i => PlatformModel.ForPlatform(i.Key)).Count(i => !i.IsDeleted); }
        }

        public string GroupIdeaTitle
        {
            get
            {
                if (GroupIdea == null) return string.Empty;
                string title;
                if (CacheManager.TryReadGroupIdeaModel((int)GroupIdea,out title)) return title;
                title = ideasRepository.GetGroupIdeasTitle((int) GroupIdea);
                CacheManager.UpdateGroupIdeaModel((int)GroupIdea, title);
                return title;
            }
        }

        public string MentorLogin { get; set; }


        public ProfileModel MentorProfile
        {
            get
            {
                if (MentorLogin == null) return null;
                return ProfileModel.GetByLogin(MentorLogin);
            }
        }


        public ProfileModel LeaderProfile
        {
            get
            {
                if (!userDictionary.ContainsKey(StudentStatusInGroup.Leader) ||
                    !userDictionary[StudentStatusInGroup.Leader].Any()) return null;
                return ProfileModel.GetByLogin(userDictionary[StudentStatusInGroup.Leader].First());
            }
        }


        public List<ProfileModel> MemberUsers
        {
            get
            {
                if (!userDictionary.ContainsKey(StudentStatusInGroup.Member) ||
                    !userDictionary[StudentStatusInGroup.Member].Any()) return new List<ProfileModel>();
                return userDictionary[StudentStatusInGroup.Member].Select(user=>ProfileModel.GetByLogin(user)).ToList();
            }
        }


        public List<ProfileModel> WishedToJoinUsers
        {
            get
            {
                if (!userDictionary.ContainsKey(StudentStatusInGroup.Wished) ||
                    !userDictionary[StudentStatusInGroup.Wished].Any()) return new List<ProfileModel>();
                return userDictionary[StudentStatusInGroup.Wished].Select(user => ProfileModel.GetByLogin(user)).ToList();
            }
        }


        public List<ProfileModel> InvitedUsers
        {
            get
            {
                if (!userDictionary.ContainsKey(StudentStatusInGroup.Invited) ||
                    !userDictionary[StudentStatusInGroup.Invited].Any()) return new List<ProfileModel>();
                return userDictionary[StudentStatusInGroup.Invited].Select(user => ProfileModel.GetByLogin(user)).ToList();
            }
        }


        public IdeaWithPermission GetWithPermissionFor(string userLogin, bool displayAsMain = false, bool displayOnly = false)
        {
            var user = ProfileModel.GetByLogin(userLogin);
            if (user == null) return null;
            var permission = PermissionType.None;

            switch (user.Role)
            {
                case RoleName.Administrator:
                    permission  |= PermissionType.AsAdmin | PermissionType.DeletePhotos | PermissionType.ShowChat
                                | PermissionType.ShowPhotos | PermissionType.ShowOfficialReports;
                    break;
                case RoleName.Mentor:
                    if (MentorLogin != null && MentorLogin == user.Login)
                    {
                        permission  |= PermissionType.AsMentor | PermissionType.ShowPhotos | PermissionType.AsMember
                                    | PermissionType.ShowChat | PermissionType.ShowOfficialReports | PermissionType.UseChat;
                    }
                    break;
                case RoleName.Student:
                    if (userDictionary.ContainsKey(StudentStatusInGroup.Member) &&
                        userDictionary[StudentStatusInGroup.Member].Contains(user.Login))
                    {
                        permission  |=  PermissionType.UseChat  | PermissionType.ShowPhotos   | PermissionType.AsMember
                                    |   PermissionType.ShowChat | PermissionType.ChangeProfile| PermissionType.DeletePhotos
                                    |   PermissionType.AddPhotos | PermissionType.ShowOfficialReports;

                        if (userDictionary.ContainsKey(StudentStatusInGroup.Leader) &&
                            userDictionary[StudentStatusInGroup.Leader].Contains(user.Login))
                        {
                            permission |= PermissionType.AsLeader;
                        }
                    }
                    break;
                case RoleName.Sponsor:
                    permission |= PermissionType.ShowChat | PermissionType.ShowOfficialReports |
                                  PermissionType.ShowPhotos;
                    break;
            }
            
            var ideaWithPermission = new IdeaWithPermission(permission)
            {
                Idea = this,
                DisplayAsMain = displayAsMain,
                DisplayOnly = displayOnly,
                Invited = InvitedUsers.FirstOrDefault(u=>u.Login == user.Login)!=null,
                Wished = WishedToJoinUsers.FirstOrDefault(u => u.Login == user.Login) != null
            };
            return ideaWithPermission;
        }


        public IdeaWithPlatform GetIdeaWithPlatform(int? priorityPlatformId = null)
        {
            PlatformModel platform = null;
            if (RealizedPlatformWithStatus.Any())
            {
                if (priorityPlatformId != null)
                {
                    platform = PlatformModel.ForPlatform(RealizedPlatformWithStatus
                        .FirstOrDefault(p=>p.Key==priorityPlatformId).Key);
                }
                if (platform == null)
                {
                    platform = PlatformModel.ForPlatform(RealizedPlatformWithStatus.First().Key);
                }
            }
            return new IdeaWithPlatform {Idea = this, Platform = platform};
        }


        /// <summary>
        /// Using only in repositories
        /// </summary>
        public void AddUserForIdea(string login, StudentStatusInGroup status)
        {
            if (!userDictionary.ContainsKey(status))
            {
                userDictionary.Add(status, new List<string>());
            }
            userDictionary[status].Add(login);
        }


        public static IdeasModel GetById(int ideaId)
        {
            IdeasModel model;
            if (CacheManager.TryReadIdeaModel(ideaId, out model)) return model;

            model = ideasRepository.GetIdeasModel(ideaId,false);

            CacheManager.UpdateIdeaModel(ideaId,model);
            return model;
        }


        /// <summary>
        /// Using only in repositories
        /// </summary>
        public static void SinchronizeWithCache(int ideaId)
        {
            IdeasModel model;
            //if (!CacheManager.TryReadIdeaModel(ideaId, out model)) return;
            model = ideasRepository.GetIdeasModel(ideaId);
            CacheManager.UpdateIdeaModel(ideaId, model);
        }
    }
}