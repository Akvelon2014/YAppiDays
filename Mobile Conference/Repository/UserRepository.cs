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
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MobileConference.Repository
{
    /// <summary>
    /// Repository for manage Image in database
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private Entities context
        {
            get { return ContextEF.Context; }
        }


        #region Users Information and User Profile

        public IPagedList<UserProfile> GetAllUsers(int page = GlobalValuesAndStrings.FirstPageCount, string search = "", 
            bool withDeleted = false, RoleName? forRole = null)
        {
            var users = GetAllUsersAsQuery(withDeleted);
            if (forRole != null && forRole!=RoleName.Guest)
                users = users.Where(user => user.webpages_Roles1.FirstOrDefault(role => role.RoleId == (int)forRole) != null);
            if (!search.IsNullOrEmpty()) users = users.Filter(search);
            return users.OrderByDescending(user => user.DateActivity).ToPagedList(page, OptionModel.Current.ItemPerPage);
        }

        public IDictionary<string, string> GetEmailsForUsers(RoleName role)
        {
            var allUsers = GetAllUsersAsQuery();
            IQueryable<UserProfile> users = allUsers
                .Where(user => user.webpages_Roles1.FirstOrDefault(r => r.RoleId == (int)role) != null);
            var userDictionary = users.ToDictionary(u=>u.Login, u=>u.Email);
            return userDictionary;
        }

        public void UpdateDateUserLogin(string login)
        {
            var user = GetUserByLogin(login);
            if (user != null)
            {
                user.DateActivity = DateTime.Now;
                context.SaveChanges();
                ProfileModel.SinchronizeWithCache(login);
            }
        }

        public bool TrySimpleAddUser(string login)
        {
            UserProfile user = context.UserProfiles.FirstOrDefault(u => u.Login.ToLower() == login.ToLower());
            if (user == null)
            {
                // Insert name into the profile table
                context.UserProfiles.Add(new UserProfile { Login = login });
                context.SaveChanges();
                SetUserToRole(login, RoleName.Student);
                return true;
            }
            return false;
        }


        public ProfileModel GetProfileModel(string login, bool searchInDeleted = false)
        {
            return GetUserByLogin(login, searchInDeleted).ToModel();
        }


        public void SetProfileData(ProfileModel profile)
        {
            if (profile == null) return;
            UserProfile user = GetUserByLogin(profile.Login);
            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.SecondName = profile.SecondName;
            user.BirthDate = (profile.BirthDate != null) ? profile.BirthDate.ConvertToDate():null;
            user.Email = profile.Email;
            user.City = profile.City;
            user.PlaceJob = profile.PlaceJob;
            user.Description = profile.Description;
            user.WishedRole = (int?)profile.WishedRole;
            if (profile.WishedRole == RoleName.Guest) user.WishedRole = null;
            switch (GetRoleByUserLogin(profile.Login))
            {
                case RoleName.Sponsor:
                case RoleName.InfoPartner:
                    if (profile.Company != null) user.Sponsor.CompanyId = profile.Company;
                    break;
                case RoleName.Expert:
                    if (profile.Company != null) user.Sponsor.CompanyId = profile.Company;
                    var oldList = ProfileModel.GetByLogin(profile.Login).PlatformToEventDictionary;
                    var newList = profile.PlatformToEventDictionary;

                    //sinchronize old and new list of platform and event for expert
                    foreach (var oldItem in oldList)
                    {
                        foreach (var platform in oldItem.Value)
                        {
                            if (!newList.ContainsKey(oldItem.Key) || !newList[oldItem.Key].Contains(platform))
                            {
                                RemovePlatformForExpert(profile.Login, platform, oldItem.Key);
                            }
                        }
                    }
                    foreach (var newItem in newList)
                    {
                        foreach (var platform in newItem.Value)
                        {
                            if (!oldList.ContainsKey(newItem.Key) || !oldList[newItem.Key].Contains(platform))
                            {
                                AddPlatformForExpert(profile.Login, platform, newItem.Key);
                            }
                        }
                    }

                    break;
                case RoleName.Student:
                    user.Student.University = profile.University;
                    user.Student.Faculty = profile.Faculty;
                    user.Student.YearBegining = profile.YearBeginning;
                    user.Student.YearGraduating = profile.YearGraduating;
                    break;
            }
            context.SaveChanges();
            ProfileModel.SinchronizeWithCache(profile.Login);
        }


        public UserProfile GetUserByLogin(string login, bool searchInDeleted = false)
        {
            return GetAllUsersAsQuery(searchInDeleted).FirstOrDefault(user => user.Login == login);
        }


        public ProfileModel GetUserByEmail(string email, bool searchInDeleted = false)
        {
            return GetAllUsersAsQuery(searchInDeleted).FirstOrDefault(user => user.Email == email).ToModel();
        }

        public List<ProfileModel> GetExpertsToStartPage(int count)
        {

            return GetAllUsersAsQuery(false)
                .Where(u => u.webpages_Roles1.FirstOrDefault(r => r.RoleId == (int)RoleName.Expert) != null).Distinct()
                .OrderBy(x => Guid.NewGuid())
                .Take(count).AsEnumerable().ToModel();
        }

        #endregion //Users Information and User Profile

        #region Roles

        public Dictionary<int, string> GetRolesList()
        {
            var roles = context.webpages_Roles;
            if(!roles.Any()) return new Dictionary<int, string>();
            return roles.ToDictionary(role => role.RoleId, role => role.RoleName.GetRole().GetInRussian());
        }


        public void SetUserToRole(string login, RoleName role)
        {
            RoleName oldRole = GetRoleByUserLogin(login);
            if (oldRole == role || role == RoleName.Guest) return;
            try
            {
                UserProfile profile = context.UserProfiles.FirstOrDefault(user => user.Login == login);
                if (profile == null) return;
                if (oldRole != RoleName.Guest) ClearRoleForUser(login);
                Roles.AddUserToRole(login, role.GetName());
                int id = profile.Id;
                switch (role)
                {
                    case (RoleName.Mentor):
                        if (context.Mentors.FirstOrDefault(mentor => mentor.Id == id) == null)
                        {
                            context.Mentors.Add(new Mentor { Id = id });
                        }
                        break;
                    case (RoleName.Student):
                        if (context.Students.FirstOrDefault(student => student.Id == id) == null)
                        {
                            context.Students.Add(new Student { Id = id });
                        }
                        break;
                    case (RoleName.Sponsor):
                    case (RoleName.InfoPartner):
                    case (RoleName.Expert):
                        if (context.Sponsors.FirstOrDefault(sponsor => sponsor.Id == id) == null)
                        {
                            context.Sponsors.Add(new Sponsor { Id = id });
                        }
                        break;
                }
                context.SaveChanges();
                SetTips(login, 0);// reset tips when role changed for user
            }
            finally
            {
                ProfileModel.SinchronizeWithCache(login);
            }
        }


        public RoleName GetRoleByUserLogin(string login)
        {
            string[] roles = Roles.GetRolesForUser(login);
            if (roles.Count() == 1)
            {
                return roles[0].GetRole();
            }
            return RoleName.Guest;
        }


        public IQueryable<UserProfile> GetUsersWithRoleWished(RoleName role)
        {
            return GetAllUsersAsQuery().Where(user => user.webpages_Roles.RoleId == (int)role);
        }


        public RoleName AcceptOrDeclineUserToWishRole(string login, bool isAccept)
        {
            UserProfile user = GetUserByLogin(login);
            var wishedRole = (RoleName)user.webpages_Roles.RoleId;
            if (isAccept)
            {
                SetUserToRole(login, wishedRole);
            }
            user.WishedRole = null;
            context.SaveChanges();
            ProfileModel.SinchronizeWithCache(login);
            return wishedRole;
        }

        #endregion //Roles

        #region Remove and Restore

        public void ConfirmEmail(string login)
        {
            var user = GetUserByLogin(login);
            if (user == null) return;
            user.EmailConfirmation = true;
            context.SaveChanges();
            ProfileModel.SinchronizeWithCache(login);
        }


        public void RemoveUser(string userLogin, bool withAbilityRestore = true)
        {
            var user = GetUserByLogin(userLogin);
            if (user != null)
            {
                if (withAbilityRestore)
                {
                    try
                    {
                        SaveRestoreInfo(userLogin);
                    }
                    catch(Exception e)
                    {
                        var logManager = ContainerDI.Container.Resolve<ICustomLogManager>();
                        logManager.Error("Unsuccessfull sending message to "+user.Login,null);
                    }
                }
                ClearMemberDataForUser(userLogin);
                user.IsDeleted = true;
                context.SaveChanges();
                ProfileModel.SinchronizeWithCache(userLogin);
            }
        }


        public void RestoreUser(string userLogin)
        {
            var user = GetUserByLogin(userLogin,true);
            if (user != null)
            {
                user.IsDeleted = false;
                context.SaveChanges();
                ProfileModel.SinchronizeWithCache(userLogin);
            }
        }


        public bool ValidateRestoreInfo(string login, string code)
        {
            var cryptManager = ContainerDI.Container.Resolve<ICryptManager>();
            var user = GetUserByLogin(login, true);
            if (user == null) return false;
            var userInfo = user.UserRestoreInfo;
            if (userInfo == null) return false;
            return cryptManager.VerifyHash(code, userInfo.Code);
        }

        public void RemoveRestoreInfo(string login)
        {
            var user = GetUserByLogin(login, true);
            if (user == null) return;
            var userInfo = user.UserRestoreInfo;
            if (userInfo == null) return;
            context.UserRestoreInfoes.Remove(userInfo);
            context.SaveChanges();
        }

        public void SaveRestoreInfo(string login)
        {
            var user = GetUserByLogin(login);
            if (user == null || user.Email.IsNullOrEmpty()) return;
            var emailManager = ContainerDI.Container.Resolve<IEmailManager>();
            var cryptManager = ContainerDI.Container.Resolve<ICryptManager>();
            string input = Guid.NewGuid().ToString();
            string hash = cryptManager.GetHash(input);
            var userInfo = user.UserRestoreInfo;
            if (userInfo != null)
            {
                userInfo.Code = hash;
            }
            else
            {
                context.UserRestoreInfoes.Add(new UserRestoreInfo {Id = user.Id, Code = hash, Date = DateTime.Now});
            }
            context.SaveChanges();
            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string link = OptionModel.FullyQualifiedApplicationPath+url.Action("Restore", "Account", new { login = user.Login, code = input });
            string text = GlobalValuesAndStrings.MessageForUserToRestore(user.FullName(), link);
            emailManager.SendMessage(user.Email, GlobalValuesAndStrings.TitleToRestoreMessage, text);
        }

        #endregion //Remove and Restore

        #region Company

        public CompanyModel GetCompanyById(int companyId)
        {
            return context.Companies.FirstOrDefault(c => c.Id == companyId).ToModel();
        }

        public List<int> GetSponsorIds()
        {
            var sponsors =
                context.Sponsors.Where(s => !s.UserProfile.IsDeleted && s.UserProfile.webpages_Roles1
                    .FirstOrDefault(r => r.RoleId == (int) RoleName.Sponsor)!=null);
            return sponsors.Where(s => s.Company != null && s.Company.IsShowed).Select(s => s.Company)
                .OrderBy(c => c.Rank==null).ThenBy(c => c.Rank).Select(c=>c.Id).ToList();
        }

        public List<int> GetInfoPartnerIds()
        {
            var infoPartner =
                context.Sponsors.Where(s => !s.UserProfile.IsDeleted && s.UserProfile.webpages_Roles1
                    .FirstOrDefault(r => r.RoleId == (int)RoleName.InfoPartner) != null);
            return infoPartner.Where(s => s.Company != null && s.Company.IsShowed).Select(s => s.Company)
                .OrderBy(c => c.Rank == null).ThenBy(c => c.Rank).Select(c => c.Id).ToList();
        }

        /// <summary>
        /// Used only with paging or other things
        /// </summary>
        public IQueryable<Company> GetAllCompanies(string search = "", bool withHidden = false)
        {
            var companies = withHidden ? context.Companies : context.Companies.Where(c => c.IsShowed);
            return (search == "") ? companies
                : companies.Where(c => c.Name.ToLower().Contains(search.ToLower()));
        }
            

        public int AddCompany(CompanyModel model)
        {
            var company = new Company
            {
                Avatar = model.Avatar,
                Name = model.Name,
                Site = model.Site.LinkCorrect(),
                Creator = model.CreatorId,
                IsShowed = true,
                Rank = model.Rank
            };
            context.Companies.Add(company);
            context.SaveChanges();
            return company.Id;
        }

        public void UpdateCompany(CompanyModel model)
        {
            var company = context.Companies.FirstOrDefault(c => c.Id == model.Id);
            if (company == null) return;
            company.Name = model.Name;
            company.Site = model.Site;
            company.IsShowed = model.IsShowed;
            company.Rank = model.Rank;
            context.SaveChanges();
            CompanyModel.SinchronizeWithCache((int)model.Id);
        }

        public void AddCompanyToUser(int companyId, string userLogin)
        {
            var user = GetUserByLogin(userLogin);
            var company = GetCompanyById(companyId);
            if (user == null || company == null || user.Sponsor == null) return;

            user.Sponsor.CompanyId = companyId;
            user.PlaceJob = company.Name;
            context.SaveChanges();
            ProfileModel.SinchronizeWithCache(userLogin);
        }

        #endregion //Company

        #region Work with platforms for experts
        public void AddPlatformForExpert(string userLogin, int platformId, int? eventId = null)
        {
            var user = ProfileModel.GetByLogin(userLogin);
            if (user.Role != RoleName.Expert) return;
            if (eventId == null) eventId = EventModel.Current.Id;
            var expertWithPlatform = context.ExpertForEvents.FirstOrDefault(e => e.Event == eventId
                                                                                 && e.Expert == user.Id
                                                                                 && e.Platform == platformId);
            if (expertWithPlatform == null)
            {
                expertWithPlatform = new ExpertForEvent
                {
                    Event = (int)eventId,
                    Platform = platformId,
                    Expert = user.Id
                };
                context.ExpertForEvents.Add(expertWithPlatform);
                context.SaveChanges();
                ProfileModel.SinchronizeWithCache(userLogin);
            }
        }


        public void RemovePlatformForExpert(string userLogin, int platformId, int? eventId = null)
        {
            var user = ProfileModel.GetByLogin(userLogin);
            if (user.Role != RoleName.Expert) return;
            if (eventId == null) eventId = EventModel.Current.Id;
            var expertWithPlatform = context.ExpertForEvents.FirstOrDefault(e => e.Event == eventId
                                                                                 && e.Expert == user.Id
                                                                                 && e.Platform == platformId);
            if (expertWithPlatform != null)
            {
                context.ExpertForEvents.Remove(expertWithPlatform);
                context.SaveChanges();
                ProfileModel.SinchronizeWithCache(userLogin);
            }
        }
        #endregion Work with platforms for experts

        #region Manage Skill for User

        public bool AddSkillToUser(string userLogin, string skillName)
        {
            var skill = GetSkill(skillName);
            var user = GetUserByLogin(userLogin);
            if (!user.Skills.Contains(skill))
            {
                user.Skills.Add(skill);
                context.SaveChanges();
                return true;
            }
            return false;
        }


        public void RemoveSkillFromUser(string userLogin, string skillName)
        {
            var skill = GetSkill(skillName);
            var user = GetUserByLogin(userLogin);
            if (user.Skills.Contains(skill))
            {
                user.Skills.Remove(skill);
                context.SaveChanges();
            }
        }


        public List<string> GetAllSkills(string userName = null, bool withDeleted = false)
        {
            List<string> list; 
            if (userName == null)
            {
                if (CacheManager.TryReadSkillsList(out list)) return list;
                list = withDeleted? context.Skills.Select(s => s.Name).ToList()
                    : context.Skills.Where( s=> !s.IsDeleted).Select(s => s.Name).ToList();
                CacheManager.UpdateSkillsList(list);
                return list;
            }
            list = context.Skills.Where(s => s.UserProfiles.FirstOrDefault(u => u.Login == userName) != null)
                .Select(s=>s.Name).ToList();
            return list;
        }


        private Skill GetSkill(string skillName, bool withDeleted = false)
        {
            var skill = context.Skills.FirstOrDefault(s => s.Name == skillName);
            if (skill == null || (!withDeleted && skill.IsDeleted))
            {
                skill = new Skill
                {
                    Name = skillName
                };
                context.Skills.Add(skill);
                context.SaveChanges();
                CacheManager.UpdateSkillsList(null);
            }
            return skill;
        }

        #endregion Manage Skill for User

        #region Tips for User

        public void SetTips(string userLogin, int? tip)
        {
            if (tip == null) return;
            var user = GetUserByLogin(userLogin, true);
            if (user == null) return;
            user.Tips = (int)tip;
            context.SaveChanges();
            ProfileModel.SinchronizeWithCache(userLogin);
        }

        public int? GetTips(string userLogin)
        {
            var user = GetUserByLogin(userLogin, true);
            if (user == null) return null;
            return user.Tips;
        }

        #endregion //Tips for user

        #region Helpers

        private void ClearMemberDataForUser(string login)
        {
            var user = GetUserByLogin(login);
            if (user == null) return;
            user.WishedRole = null;
            IEnumerable<Idea> ideas;
            RoleName role = GetRoleByUserLogin(login);
            switch (role)
            {
                case RoleName.Mentor:
                    ideas = user.Mentor.Ideas;
                    if (ideas.Any())
                    {
                        foreach (var idea in ideas)
                        {
                            idea.Mentor = null;
                        }
                    }
                    break;
                case RoleName.Student:
                    var ideasRepository = ContainerDI.Container.Resolve<IIdeasRepository>();
                    if (user.Student != null)
                    {
                        var group = user.Student.StudentGroups;
                        if (group != null)
                        {
                            ideas = group.Select(gr => gr.Idea).Distinct().ToArray();
                            if (ideas.Any())
                            {
                                foreach (var idea in ideas)
                                {
                                    ideasRepository.RemoveMemberFromIdea(idea.Id, login);
                                }
                            }
                        }
                    }
                    break;
            }
            context.SaveChanges();
        }

        private void ClearRoleForUser(string login)
        {
            RoleName oldRole = GetRoleByUserLogin(login);
            ClearMemberDataForUser(login);
            if (oldRole != RoleName.Guest)
                Roles.RemoveUserFromRole(login, oldRole.GetName());
            ProfileModel.SinchronizeWithCache(login);
        }


        /// <summary>
        /// Method for get Query to get real list (please, using pagination, because this list may be large)
        /// </summary>
        /// <returns>Query to get users list</returns>
        private IQueryable<UserProfile> GetAllUsersAsQuery(bool searchInDeleted = false)
        {
            return searchInDeleted ? context.UserProfiles : context.UserProfiles.Where(profile => !profile.IsDeleted);
        }

        #endregion //Helpers

    }
}
