using MobileConference.Enums;
using MobileConference.GlobalData;
using MobileConference.Models;
using System.Collections.Generic;
using System.Linq;
using PagedList;

namespace MobileConference.Interface
{
    public interface IUserRepository
    {
        IPagedList<UserProfile> GetAllUsers(int page = GlobalValuesAndStrings.FirstPageCount, string search = ""
            , bool withDeleted = false, RoleName? forRole = null);
        IDictionary<string, string> GetEmailsForUsers(RoleName role);

        IQueryable<UserProfile> GetUsersWithRoleWished(RoleName role);
        Dictionary<int, string> GetRolesList();
        ProfileModel GetUserByEmail(string email, bool withDeleted = false);
        List<ProfileModel> GetExpertsToStartPage(int count);
        bool TrySimpleAddUser(string login);
        void UpdateDateUserLogin(string login);
        void SetUserToRole(string login, RoleName role);
        void RemoveUser(string userLogin, bool withAbilityRestore = true);
        void RestoreUser(string userLogin);
        void ConfirmEmail(string login);
        void SaveRestoreInfo(string login);
        bool ValidateRestoreInfo(string login, string code);
        void RemoveRestoreInfo(string login);
        RoleName AcceptOrDeclineUserToWishRole(string login, bool accept);
        RoleName GetRoleByUserLogin(string login);


        /// <summary>
        /// Only for use in repositories!!!!
        /// </summary>
        UserProfile GetUserByLogin(string login, bool withDeleted = false);
        ProfileModel GetProfileModel(string login, bool withDeleted = false);
        void SetProfileData(ProfileModel profile);

        //Work with company
        CompanyModel GetCompanyById(int companyId);
        List<int> GetSponsorIds();
        List<int> GetInfoPartnerIds();
        IQueryable<Company> GetAllCompanies(string search = "", bool withHidden = false);
        int AddCompany(CompanyModel model);
        void UpdateCompany(CompanyModel model);
        void AddCompanyToUser(int companyId, string userLogin);

        //Work with platform for experts
        void AddPlatformForExpert(string userLogin, int platformId, int? eventId = null);
        void RemovePlatformForExpert(string userLogin, int platformId, int? eventId = null);

        //Work with skills
        bool AddSkillToUser(string userLogin, string skill);
        void RemoveSkillFromUser(string userLogin, string skill);
        List<string> GetAllSkills(string userName = null, bool withDeleted = false);

        //Tips
        void SetTips(string userLogin, int? tip);
        int? GetTips(string userLogin);
    }
}
