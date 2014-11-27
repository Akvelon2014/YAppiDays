using System.Linq;
using System.Web.Routing;
using Castle.Core.Internal;
using DotNetOpenAuth.AspNet;
using Microsoft.Ajax.Utilities;
using Microsoft.Web.WebPages.OAuth;
using MobileConference.Auxiliary;
using MobileConference.Enums;
using MobileConference.Filters;
using MobileConference.GlobalData;
using MobileConference.Helper;
using MobileConference.Interface;
using MobileConference.Managers;
using MobileConference.Models;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace MobileConference.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private readonly IUserRepository userRepository;
        private readonly IImageManager imageManager;
        private readonly IImageRepository imageRepository;
        private readonly ICapchaManager capchaManager;
        private readonly IEmailManager emailManager;
        private readonly ICryptManager cryptManager;


        public AccountController(IUserRepository userRepository, IImageManager imageManager,IImageRepository imageRepository,
            ICapchaManager capchaManager, IEmailManager emailManager, ICryptManager cryptManager)
        {
            this.userRepository = userRepository;
            this.imageManager = imageManager;
            this.imageRepository = imageRepository;
            this.capchaManager = capchaManager;
            this.emailManager = emailManager;
            this.cryptManager = cryptManager;
        }

        #region Resolve Command from account drop down

        [AllowAnonymous]
        public ActionResult ResolveProfileCommand(string profileCommand)
        {
            if (!User.Identity.IsAuthenticated)
            {
                switch ((ProfileCommands)int.Parse(profileCommand))
                {
                    case ProfileCommands.Login:
                        return RedirectToAction("LoginPartial");
                    case ProfileCommands.Register:
                        return RedirectToAction("RegisterPartial");
                    default:
                        return RedirectToAction("LoginPartial");
                }
            }

            switch ((ProfileCommands) int.Parse(profileCommand))
            {
                case ProfileCommands.ProfileChange:
                    return RedirectToAction("ProfileChangeFromPopup");
                case ProfileCommands.Logoff:
                    return RedirectToAction("LogOff");
                case ProfileCommands.ChangePassword:
                    return RedirectToAction("ChangePassword");
                case ProfileCommands.AccountsManage:
                    return RedirectToAction("AccountManageFromPopup");
                case ProfileCommands.Admin:
                    return RedirectToAction("IndexPartial", "Admin");
                case ProfileCommands.MyIdea:
                    return RedirectToAction("IndexFromPopup", "Ideas");
                case ProfileCommands.MentorPage:
                    return RedirectToAction("MentorPageFromPopup", "Ideas");
                case ProfileCommands.IdeasWithoutMentor:
                    return RedirectToAction("IdeasWithoutMentorFromPopup", "Ideas");
                default:
                    return RedirectToAction("ProfileChangeFromPopup");
            }
        }


        [AllowAnonymous]
        public ActionResult LoginPartial(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("LoginPopup");
        }


        [AllowAnonymous]
        public ActionResult RegisterPartial()
        {
            return PartialView("RegisterPopup");
        }


        public ActionResult AccountManageFromPopup()
        {
            if (Request.IsAjaxRequest()) return Json(new {data = GlobalValuesAndStrings.EmptyAnswerInAction},JsonRequestBehavior.AllowGet);
            return RedirectToAction("Manage");
        }

        #endregion //Resolve Command from account drop down

        #region Local login, logoff and registration

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = userRepository.GetUserByLogin(model.Login);
                if (user != null && !user.IsDeleted && 
                        WebSecurity.Login(model.Login, model.Password, persistCookie: model.RememberMe))
                {
                    if (!user.EmailConfirmation)
                    {
                        WebSecurity.Logout();
                        return View("ConfirmationMessage", model: user.Email);
                    }
                    userRepository.UpdateDateUserLogin(model.Login);
                    return RedirectToLocal(returnUrl);
                }
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError(string.Empty, GlobalValuesAndStrings.IncorrectUserOrPassword);
            return View(model);
        }


        public ActionResult LogOff()
        {
            if (Request.IsAjaxRequest()) return Json(new { data = GlobalValuesAndStrings.EmptyAnswerInAction }, JsonRequestBehavior.AllowGet);
            WebSecurity.Logout();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            //if (Request.IsAjaxRequest()) return PartialView("RegisterPopup");
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (!model.Capcha.IsNullOrEmpty() && Session[GlobalValuesAndStrings.CapchaSessionName].ToString().Trim() != model.Capcha.Trim())
            {
                ModelState.AddModelError(GlobalValuesAndStrings.CapchaError, GlobalValuesAndStrings.CapchaIncorrect);
            }

            if (!model.Email.IsNullOrEmpty() && userRepository.GetUserByEmail(model.Email, true) != null)
            {
                ModelState.AddModelError(GlobalValuesAndStrings.EmailError, GlobalValuesAndStrings.IncorrectUserByEmail);
            }

            if (!model.Login.IsNullOrEmpty() && userRepository.GetUserByLogin(model.Login, true) != null)
            {
                ModelState.AddModelError(GlobalValuesAndStrings.LoginError, GlobalValuesAndStrings.IncorrectUserByLogin);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Attempt to register the user
            try
            {
                if (model.WishedRole == RoleName.Student)
                {
                    model.WishedRole = null;
                }

                WebSecurity.CreateUserAndAccount(model.Login, model.Password, new
                {
                    model.FirstName, model.LastName, model.Email,
                    WishedRole = (model.WishedRole!=RoleName.Guest)?model.WishedRole:null,
                    EmailConfirmation = false
                });

                userRepository.SetUserToRole(model.Login, RoleName.Student);
                //WebSecurity.Login(model.Login, model.Password);

                return RedirectToAction("SendConfirmationMessage", "Account", new {email = model.Email});
            }
            catch (MembershipCreateUserException e)
            {
                ModelState.AddModelError(string.Empty, AdditionalExtender.ErrorCodeToString(e.StatusCode));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult SendConfirmationMessage(string email)
        {
            emailManager.SendEmailAfterRegistration(email);
            ViewBag.email = email;
            return View();
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult ChangeEmailAfterRegistration()
        {
            return View();
        }
        
        [AllowAnonymous]
        [HttpPost]
        public ActionResult ChangeEmailAfterRegistration(EmailChanging model)
        {
            ProfileModel user = ProfileModel.GetByLogin(model.Login);
            if (!ModelState.IsValid)
            {
                return View(model);                
            }
            if (user != null && WebSecurity.Login(model.Login, model.Password))
            {
                if (user.EmailConfirmation)
                {
                    return RedirectToAction("Index", "Home");
                }
                WebSecurity.Logout();
                user.Email = model.Email;
                userRepository.SetProfileData(user);
                return RedirectToAction("SendConfirmationMessage","Account",new{model.Email});
            }
            ModelState.AddModelError(GlobalValuesAndStrings.LoginError,GlobalValuesAndStrings.IncorrectUserOrPassword);
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ConfirmMessage(string login, string code)
        {
            var user = ProfileModel.GetByLogin(login);
            if (user != null && !user.EmailConfirmation)
            {
                var hash = cryptManager.GetHash(user.EmailConfirmData);
                bool isConfirm = cryptManager.VerifyHash(hash, code);
                if (isConfirm)
                {
                    userRepository.ConfirmEmail(login);
                    return View();
                }
            }
            return RedirectToAction("NotFoundError", "Home");
        }

        #endregion //Local login, logoff and registration

        #region Profile Change and Profile Restore

        public ActionResult ProfileChangeFromPopup()
        {
            if (Request.IsAjaxRequest()) return Json(new { data = GlobalValuesAndStrings.EmptyAnswerInAction }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("ChangeProfile");
        }

        [HttpGet]
        public ActionResult ChangeProfile()
        {
            ProfileModel model = ProfileModel.Current;
            if (model != null) return View(model);
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeProfile(ProfileModel profile)
        {
            var oldProfile = ProfileModel.Current;
            profile.Login = oldProfile.Login;
            profile.Email = oldProfile.Email;
            profile.Role = oldProfile.Role;

            profile.University = profile.University.NullOrTrim();
            profile.Faculty = profile.Faculty.NullOrTrim();
            profile.FirstName = profile.FirstName.NullOrTrim();
            profile.LastName = profile.LastName.NullOrTrim();
            profile.SecondName = profile.SecondName.NullOrTrim();
            profile.City = profile.City.NullOrTrim();
            profile.PlaceJob = profile.PlaceJob.NullOrTrim();

            //Uncomment the code bellow if you would like to add the email change capability
            //if (!profile.Email.IsNullOrEmpty())
            //{
            //    var user = userRepository.GetUserByEmail(profile.Email, true);
            //    if (user != null && user.Login != profile.Login)
            //    {
            //        ModelState.AddModelError(GlobalValuesAndStrings.EmailError, GlobalValuesAndStrings.IncorrectUserByEmail);
            //    }
            //}

            if ((!profile.Faculty.IsNullOrEmpty() || profile.YearBeginning != null || profile.YearGraduating != null)
                && profile.University.IsNullOrEmpty())
            {
                ModelState.AddModelError(GlobalValuesAndStrings.UniversityDataWithoutTitleError
                    , GlobalValuesAndStrings.UniversityDataWithoutTitle);
            }
            ViewBag.error = false;
            if (!ModelState.IsValid)
            {
                ViewBag.error = true;
                return View(profile);
            }            
            userRepository.SetProfileData(profile);
            return View(profile);  
        }


        [HttpGet]
        public ActionResult ChangeAvatar()
        {
            return PartialView(ProfileModel.Current);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeAvatar(string picture, ImageSizeParams cropSize = null)
        {
            if (picture != null && cropSize != null)
            {
                if (cropSize.y2 != 0 && cropSize.x2 != 0)
                {
                    var avatar = imageManager.Save(picture, cropSize);
                    imageRepository.SaveAvatar(avatar, ProfileModel.Current.Id);
                }
            }
            return PartialView(ProfileModel.Current);
        }


        public ActionResult ChangeSkill()
        {
            var user = ProfileModel.Current;
            if (user == null) return RedirectToAction("Index", "Home");
            return View(user);
        }

        public ActionResult ChangeSkillsPartial(string message = null)
        {
            var user = ProfileModel.Current;
            if (user == null) return PartialView();
            if (message != null)
            {
                ModelState.AddModelError(string.Empty,message);
            }
            return PartialView(user);
        }

        public ActionResult AddSkill(string skillName)
        {
            var user = ProfileModel.Current;
            if (user == null) return new EmptyResult();
            if (skillName.IsNullOrEmpty())
            {
                return RedirectToAction("ChangeSkillsPartial", new {message = GlobalValuesAndStrings.SkillIsEmpty});                
            }
            if (skillName.Length>GlobalValuesAndStrings.SkillLength)
            {
                return RedirectToAction("ChangeSkillsPartial", new { message = GlobalValuesAndStrings.SkillIsTooLarge });
            }
            if (!userRepository.AddSkillToUser(user.Login, skillName))
            {
                return RedirectToAction("ChangeSkillsPartial", new { message = GlobalValuesAndStrings.SkillIsExists });                   
            }
            return RedirectToAction("ChangeSkillsPartial");
        }


        public ActionResult RemoveSkill(string skillName)
        {
            var user = ProfileModel.Current;
            if (user == null) return new EmptyResult();
            userRepository.RemoveSkillFromUser(user.Login, skillName);
            return RedirectToAction("ChangeSkillsPartial");
        }


        /// <summary>
        /// Attempt to user restoring (it will should used only from email, which was sending to user)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Restore(string login = "", string code="")
        {
            ViewBag.message = (code.IsNullOrEmpty()) ? string.Empty : GlobalValuesAndStrings.UnsuccessedAttemptToRestore;
            if (userRepository.ValidateRestoreInfo(login, code))
            {
                userRepository.RestoreUser(login);
                var membershipUser = Membership.GetUser(login);
                var user = ProfileModel.GetByLogin(login);
                if (membershipUser != null && user!=null)
                {
                    var newPassword = Membership.GeneratePassword(GlobalValuesAndStrings.PasswordLength
                        , GlobalValuesAndStrings.NumberOfNunAlphaCharacterInPassword);
                    if (OAuthWebSecurity.GetAccountsFromUserName(login).Any() &&
                        !OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(login)))
                    {
                        WebSecurity.CreateAccount(login, newPassword);
                        userRepository.RemoveRestoreInfo(login);
                        return View(model: newPassword);
                    }
                    string token = WebSecurity.GeneratePasswordResetToken(login);
                    WebSecurity.ResetPassword(token, newPassword);
                    userRepository.RemoveRestoreInfo(login);
                    emailManager.SendMessage(user.Email,GlobalValuesAndStrings.TitleForRestorePasswordMessage,
                        GlobalValuesAndStrings.MessageWithNewPassword(user.FullName(),login, newPassword));
                    ViewBag.email = user.Email;
                    return View(model: user.FullName());
                }
            }
            return View(model: string.Empty);
        }


        [AllowAnonymous]
        public ActionResult SendRestoreMessage(string email)
        {
            var user = userRepository.GetUserByEmail(email, true);
            if (user == null) return View("ErrorRestore");
            userRepository.SaveRestoreInfo(user.Login);
            ViewBag.email = email;
            return View();
        }

        //CODE BELLOW FOR WORK WITH COMPANY (ONLY FOR INFO PARTNERS, SPONSORS AND EXPERTS

        [ForbiddenElse("InfoPartner", "Sponsor", "Expert")]
        public ActionResult SelectCompany(int page = GlobalValuesAndStrings.FirstPageCount, string search = "")
        {
            ViewBag.page = page;
            ViewBag.search = search;
            return View();
        }

        [HttpPost]
        [ForbiddenElse("InfoPartner", "Sponsor", "Expert")]
        public ActionResult AddToCompany(int companyId, int page, string search = "")
        {
            var company = CompanyModel.GetById(companyId);
            if (company != null)
            {
                userRepository.AddCompanyToUser(companyId, ProfileModel.Current.Login);
                return RedirectToAction("ChangeProfile");
            }
            return RedirectToAction("SelectCompany", new { page, search });
        }


        #endregion //Profile Change and Profile Restore

        #region Unused actions (password change)
        // If you need to remove the capability to register via social network you should remove NonAction attribute 
        // bellow. Than you should set NonAction attribute to actions after label "ALL CODE AFTER THIS COMMENT USED FOR
        // EXTERNAL REGISTRATION" or remove ones.
        [NonAction]
        public ActionResult ChangePassword(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (Request.IsAjaxRequest()) return PartialView("PasswordChangePopup");
            return View();
        }


        [NonAction]
        [HttpPost]
        [ValidateAntiForgeryToken]                
        public ActionResult ChangePassword(LocalPasswordModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var membershipUser = Membership.GetUser();
                    if (membershipUser != null && 
                        membershipUser.ChangePassword(model.OldPassword, model.NewPassword) == false)
                    {
                        ModelState.AddModelError(GlobalValuesAndStrings.PasswordError, GlobalValuesAndStrings.IncorrectOldPassword);
                    }
                    else
                    {
                        return RedirectToLocal(returnUrl);
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError(string.Empty, AdditionalExtender.ErrorCodeToString(e.StatusCode));
                }
            }
            return View(model);
        }
        #endregion //Unused actions (password change)

        //ALL CODE AFTER THIS COMMENT USED FOR EXTERNAL LOGIN AND REGISTER

        #region External login and register
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? GlobalValuesAndStrings.ChangePasswordSuccessMessage
                : message == ManageMessageId.SetPasswordSuccess ? GlobalValuesAndStrings.SetPasswordSuccessMessage
                : message == ManageMessageId.RemoveLoginSuccess ? GlobalValuesAndStrings.RemoveLoginSuccessMessage
                : string.Empty;
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    ModelState.AddModelError(string.Empty, GlobalValuesAndStrings.IncorrectOrWrongPasswordMessage);
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError(string.Empty, e);
                    }
                }
            }
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }


        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                string name = OAuthWebSecurity.GetUserName(result.Provider, result.ProviderUserId);
                var user = ProfileModel.GetByLogin(name);
                if (user.IsDeleted)
                {
                    WebSecurity.Logout();
                    return RedirectToAction("Login");
                }
                if (!user.EmailConfirmation)
                {
                    WebSecurity.Logout();
                    return View("ConfirmationMessage", model: user.Email);
                }
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                if (ViewBag.ProviderDisplayName == "Facebook")
                {
                    return View("ConfirmExternalLogin", new RegisterExternalLoginModel { Login = result.UserName, ExternalLoginData = loginData, ExtraData = result.ExtraData
                        ,PictureLink = string.Format("https://graph.facebook.com/{0}/picture?width=400", result.ProviderUserId)});
                }
                return View("ConfirmExternalLogin", new RegisterExternalLoginModel { Login = result.UserName, ExternalLoginData = loginData, ExtraData = result.ExtraData});
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmExternalLogin(RegisterExternalLoginModel model, string returnUrl, ImageSizeParams size = null)
        {
            ViewBag.size = size;
            ModelState.ClearMeasure();

            string provider;
            string providerUserId;
            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (!model.Email.IsNullOrEmpty() && userRepository.GetUserByEmail(model.Email, true) != null)
            {
                ModelState.AddModelError(GlobalValuesAndStrings.EmailError, GlobalValuesAndStrings.IncorrectUserByEmail);
            }

            if (ModelState.IsValid)
            {

                if(userRepository.TrySimpleAddUser(model.Login))
                {
                    OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.Login);

                    if (size != null && !model.PictureLink.IsNullOrEmpty())
                    {
                        var avatar = imageManager.LoadFromThirdSite(model.PictureLink, size);
                        ProfileModel profile = ProfileModel.GetByLogin(model.Login);
                        profile.FirstName = model.FirstName;
                        profile.LastName = model.LastName;
                        profile.Email = model.Email;
                        profile.EmailConfirmation = false;
                        profile.BirthDate = string.Empty;
                        userRepository.SetProfileData(profile);
                        imageRepository.SaveAvatar(avatar, profile.Id);
                        CacheManager.UpdateUserProfileModel(model.Login, null);
                        CacheManager.UpdateAvatar(profile.Id, null);
                    }

                    return RedirectToAction("SendConfirmationMessage", "Account", new { email = model.Email });
                    //OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);
                    //return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(GlobalValuesAndStrings.IncorrectUserByLogin,
                        GlobalValuesAndStrings.DuplicateUserNameMessage);
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }
            return RedirectToAction("Manage", new { Message = message });
        }


        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            var externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);
                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #endregion //External login and register

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion //Helpers
    }
}
