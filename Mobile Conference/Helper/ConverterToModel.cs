using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Interface;
using MobileConference.Models;
using PagedList;
using System.Collections.Generic;
using System.Linq;

namespace MobileConference.Helper
{
    /// <summary>
    /// Extension to Entity for convert into Model 
    /// </summary>
    public static class ConverterToModel
    {
        private static readonly IUserRepository userRepository;
        private static readonly IEventRepository eventRepository;
        private static readonly IIdeasRepository ideasRepository;
        private static readonly IImageRepository imageRepository;

        static ConverterToModel()
        {
            userRepository = ContainerDI.Container.Resolve<IUserRepository>();
            eventRepository = ContainerDI.Container.Resolve<IEventRepository>();
            ideasRepository = ContainerDI.Container.Resolve<IIdeasRepository>();
            imageRepository = ContainerDI.Container.Resolve<IImageRepository>();
        }

        #region Idea convert

        public static IdeasModel ToModel(this Idea idea)
        {
            if (idea == null) return null;
            var model = new IdeasModel
            {
                Id = idea.Id,
                Title = idea.Title,
                Description = idea.Description,
                GroupIdea = idea.GroupIdeaId,
                IsDeleted = idea.IsDeleted,
                CreatedDate = idea.CreatedDate,
                EventId = idea.EventProfile==null?OptionModel.Current.CurrentEventId:idea.EventProfile.Id,
                MentorLogin = idea.Mentor == null ? null : idea.Mentor.UserProfile.Login
            };

            //Anonymous objects: users and user (because it's used only in this context)
            var users = idea.StudentGroups.Where(g=>g.Student!=null).Select(
                gr => new { student = gr.Student.UserProfile.Login, status = gr.StatusStudentId });
            foreach (var user in users)
            {
                model.AddUserForIdea(user.student,(StudentStatusInGroup)user.status);
            }
            model.RealizedPlatformWithStatus = idea.RealizedPlatforms.ToDictionary(platform => platform.PlatformId,
                platform => platform.Status);
            return model;
        }

        
        public static List<IdeasModel> ToModel(this IQueryable<Idea> ideas)
        {
            return ideas.Select(idea => idea.ToModel()).ToList();
        }


        public static List<IdeasModel> ToModel(this IPagedList<Idea> ideas)
        {
            return ideas.Select(idea => idea.ToModel()).ToList();
        }

        #endregion //Idea convert

        #region Users convert

        public static ProfileModel ToModel(this UserProfile user)
        {
            if (user == null) return null;
            RoleName role = userRepository.GetRoleByUserLogin(user.Login);
            var model = new ProfileModel
            {
                Id = user.Id,
                IsDeleted = user.IsDeleted,
                Avatar = user.Avatar,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SecondName = user.SecondName,
                Login = user.Login,
                BirthDate = user.BirthDate.ConvertToCustomString(),
                Email = user.Email,
                City = user.City,
                WishedRole = (RoleName?)user.WishedRole,
                Description = user.Description,
                DateActivity = user.DateActivity,
                Role = role,
                PlaceJob = user.PlaceJob,
                RoleInRussian = role.GetInRussian(),
                EmailConfirmation = user.EmailConfirmation,
                Tips = user.Tips
            };
            if (user.Student != null)
            {
                model.University = user.Student.University;
                model.YearBeginning = user.Student.YearBegining;
                model.YearGraduating = user.Student.YearGraduating;
                model.Faculty = user.Student.Faculty;
            }

            switch (model.Role)
            {
                case RoleName.Mentor:
                    var ideas = user.Mentor.Ideas.Where(idea=>!idea.IsDeleted && idea.EventId==OptionModel.Current.CurrentEventId)
                        .ToList();

                    foreach (var idea in ideas)
                    {
                        model.AddIdeaForUser(idea.Id, StudentStatusInGroup.Member);
                    }
                    break;
                case RoleName.Sponsor:
                case RoleName.InfoPartner:
                    model.Company = user.Sponsor.CompanyId;
                    break;
                case RoleName.Expert:
                    model.Company = user.Sponsor.CompanyId;
                    foreach (var eventAndPlatform in user.ExpertForEvents)
                    {
                        model.AddEventAndPlatform(eventAndPlatform.Event, eventAndPlatform.Platform);
                    }
                    break;
                case RoleName.Student:
                    var ideasWithStatus = user.Student.StudentGroups.Where(gr=>gr.Idea.EventId==OptionModel.Current.CurrentEventId 
                        && !gr.Idea.IsDeleted).Select(gr => new
                        {
                            idea = gr.Idea.Id,
                            status = gr.StatusStudentId
                        }).ToList();
                    foreach (var idea in ideasWithStatus)
                    {
                        model.AddIdeaForUser(idea.idea, (StudentStatusInGroup) idea.status);
                    }
                    break;
            }
            return model;
        }


        public static List<ProfileModel> ToModel(this IQueryable<UserProfile> users)
        {
            return users.Select(user => user.ToModel()).ToList();
        }


        public static List<ProfileModel> ToModel(this IEnumerable<UserProfile> users)
        {
            return users.Select(user => user.ToModel()).ToList();
        }


        public static List<ProfileModel> ToModel(this IPagedList<UserProfile> users)
        {
            return users.Select(user => user.ToModel()).ToList();
        }

        #endregion //Users convert

        #region Event convert

        public static EventModel ToModel(this EventProfile eventProfile)
        {
            if (eventProfile == null) return null;
            var model = new EventModel
            {
                Title = eventProfile.Title,
                Description = eventProfile.Desctiption,
                DateFrom = eventProfile.DateFrom.ConvertToCustomString(),
                DateTo = eventProfile.DateTo.ConvertToCustomString(),
                EventType = eventProfile.EventType.Id,
                IsDeleted = eventProfile.IsDeleted ?? false,
                ParentId = eventProfile.Parent,
                Id = eventProfile.Id,
                ChildIds = eventRepository.GetChildsIdForEvent(eventProfile.Id),
                Regions = eventRepository.GetAllRegions(eventProfile.Id).Keys.ToArray(),
                Platforms = eventRepository.GetAllPlatform(eventProfile.Id).Keys.ToArray(),
                IdeaTypes = eventRepository.GetAllIdeaGroups(eventProfile.Id).Keys.ToArray(),
                Avatar = eventProfile.Avatar
            };
            return model;
        }


        public static List<EventModel> ToModel(this IQueryable<EventProfile> eventModels)
        {
            return eventModels.Select(ev => ev.ToModel()).ToList();
        }


        public static List<EventModel> ToModel(this IPagedList<EventProfile> eventModels)
        {
            return eventModels.Select(ev => ev.ToModel()).ToList();
        }

        #endregion //Event convert

        #region Comment convert

        public static CommentModel ToModel(this Comment comment)
        {
            if (comment == null) return null;
            var model = new CommentModel
            {
                CreationDate = comment.Date,
                Id = comment.Id,
                IsDeleted = comment.IsDeleted,
                UserLogin = (comment.UserProfile != null) ? comment.UserProfile.Login : null,
                Message = comment.Message,
                LinkId = comment.LinkId,
                Type = (CommentModelType)comment.Type
            };
            return model;
        }


        public static List<CommentModel> ToModel(this IQueryable<Comment> comment)
        {
            return comment.Select(ev => ev.ToModel()).ToList();
        }


        public static List<CommentModel> ToModel(this IPagedList<Comment> comment)
        {
            return comment.Select(ev => ev.ToModel()).ToList();
        }

        #endregion //Comment convert

        #region Platform convert

        public static PlatformModel ToModel(this Platform platform)
        {
            if (platform == null) return null;
            var model = new PlatformModel
            {
                Id = platform.Id,
                IsDeleted = platform.IsDeleted,
                Title = platform.Name,
                Description = platform.Description,
                PictureId = platform.Picture
            };
            return model;
        }


        public static List<PlatformModel> ToModel(this IQueryable<Platform> platform)
        {
            return platform.Select(n => n.ToModel()).ToList();
        }


        public static List<PlatformModel> ToModel(this IEnumerable<Platform> platform)
        {
            return platform.Select(n => n.ToModel()).ToList();
        }


        public static List<PlatformModel> ToModel(this IPagedList<Platform> platform)
        {
            return platform.Select(n => n.ToModel()).ToList();
        }

        #endregion //Platform convert

        #region Material convert

        public static MaterialModel ToModel(this Material material)
        {
            if (material == null) return null;
            var model = new MaterialModel
            {
                Id = material.Id,
                IsDeleted = material.IsDeleted,
                Title = material.Title,
                PlatformId = material.PlatformId,
                AddedDate = material.AddedDate,
                Link = material.Link,
                CommentCount = ideasRepository.GetCommentsCountForLink(material.Id, CommentModelType.Material) 
            };
            return model;
        }


        public static List<MaterialModel> ToModel(this IQueryable<Material> material)
        {
            return material.Select(n => n.ToModel()).ToList();
        }


        public static List<MaterialModel> ToModel(this IEnumerable<Material> material)
        {
            return material.Select(n => n.ToModel()).ToList();
        }


        public static List<MaterialModel> ToModel(this IPagedList<Material> material)
        {
            return material.Select(n => n.ToModel()).ToList();
        }

        #endregion // Material convert

        #region Company convert

        public static CompanyModel ToModel(this Company company)
        {
            if (company == null) return null;
            var model = new CompanyModel
            {
                Id = company.Id,
                Name = company.Name,
                Avatar = company.Avatar,
                Site = company.Site,
                CreatorId = company.Creator,
                IsShowed = company.IsShowed,
                Rank = company.Rank
            };
            return model;
        }


        public static List<CompanyModel> ToModel(this IQueryable<Company> companies)
        {
            return companies.Select(c => c.ToModel()).ToList();
        }


        public static List<CompanyModel> ToModel(this IPagedList<Company> companies)
        {
            return companies.Select(c => c.ToModel()).ToList();
        }
        #endregion //Company convert

        #region Award convert

        public static AwardModel ToModel(this Award award)
        {
            if (award == null) return null;
            var model = new AwardModel
            {
                Id = award.Id,
                Title = award.Title,
                Description = award.Description,
                PictureId = award.Picture,
                PostTitle = award.PostTitle,
                Subtitle = award.Subtitle,
                IsDeleted = award.IsDeleted,
                OrderInList = award.OrderInList,
                EventId = award.EventId
            };
            if (model.PictureId != null)
            {
                model.PictureModel = imageRepository.GetPictureById((int)model.PictureId);                
            }
            return model;
        }


        public static List<AwardModel> ToModel(this IQueryable<Award> companies)
        {
            return companies.Select(c => c.ToModel()).ToList();
        }


        public static List<AwardModel> ToModel(this IPagedList<Award> companies)
        {
            return companies.Select(c => c.ToModel()).ToList();
        }
        #endregion // Award convert

        #region Picture convert

        public static PictureNameModel ToModel(this PictureStore picture)
        {
            if (picture == null) return null;
            var model = new PictureNameModel
            {
                Id = picture.Id,
                CreationDate = picture.Date,
                GuidString = picture.Picture
            };
            return model;
        }


        public static List<PictureNameModel> ToModel(this IQueryable<PictureStore> pictures)
        {
            return pictures.Select(c => c.ToModel()).ToList();
        }

        public static List<PictureNameModel> ToModel(this IEnumerable<PictureStore> pictures)
        {
            return pictures.Select(c => c.ToModel()).ToList();
        }

        #endregion //Picture convert

        #region News convert

        public static NewsModel ToModel(this News news)
        {
            if (news == null) return null;
            var model = new NewsModel
            {
                DateFrom = news.Date,
                DateTo = news.DateTo,
                RoleFor = (RoleSet)news.ForRoles,
                Id = news.Id,
                IsDeleted = news.IsDeleted,
                Title = news.Title,
                EventId = news.EventId,
                Description = news.Description
            };
            return model;
        }


        public static List<NewsModel> ToModel(this IQueryable<News> news)
        {
            return news.Select(n => n.ToModel()).ToList();
        }


        public static List<NewsModel> ToModel(this IEnumerable<News> news)
        {
            return news.Select(n => n.ToModel()).ToList();
        }


        public static List<NewsModel> ToModel(this IPagedList<News> news)
        {
            return news.Select(n => n.ToModel()).ToList();
        }

        #endregion //News convert 

    }
}