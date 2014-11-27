using MobileConference.Helper;

namespace MobileConference.GlobalData
{
    /// <summary>
    /// Store for all global value
    /// </summary>
    public static class GlobalValuesAndStrings
    {
        //Int values
        public const int FirstPageCount = 1;
        public const int MinRegionsForCityToRemove = 1;
        public const int StartedStatusForRealizedPlatform = 0;        
        public const int OwnIdea = -1;
        public const int NeededCountOfMemberToChangeLeader = 2;
        public const int FirstLevelInEvent = 0;
        public const int NewsLevelInEvent = 2;
        public const int ImageWidth = 600;
        public const int ImageHeight = 600;
        public const int MiniImageWidth = 145;
        public const int MiniImageHeight = 145;
        public const int CapchaWidth = 130;
        public const int CapchaHeight = 30;
        public const int MinSelectedAreaSize = 3;
        public const int ExpertCountOnStartPage = 3;
        public const int ProjectCountOnStartPage = 4;
        public const int MaterialCountOnStartPage = 4;
        public const int DaysForStoreTemporaryImagesBeforeRemoving = 1;
        public const int ProjectsPerOneStudentCount = 1;
        

        //Photo size
        public const int IdeaWidth = 220;
        public const int IdeaHeight = 130;
        public const int UserProfileWidth = 220;
        public const int UserProfileHeight = 300;
        public const int AwardHeight = 120;
        public const int AwardWidth = 120;

        //Fields size
        public const int SkillLength = 100;
        public const int PasswordLength = 7;
        public const int NumberOfNunAlphaCharacterInPassword = 0;


        //Error messages
        public const string UnknownIdeaGroup = "Неизвестный вид проекта";
        public const string UnknownPictureFormat = "Неизвестный формат рисунка";
        public const string IncorrectUserOrPassword = "Неверное имя пользователя или пароль";
        public const string IncorrectUserByLogin = "Пользователь с таким логином уже существует";
        public const string IncorrectUserByEmail = "Пользователь с такой электронной почтой уже существует";
        public const string IncorrectDateFormat = "Неверно введена дата (формат даты дд.мм.год)!";
        public const string IncorrectOldPassword = "Неверно введен старый пароль";
        public const string IncorrectPlatformTitle = "Создание технологии было неудачным, возможно такая технология уже существует!";
        public const string IncorrectMaterialAdded = "Добавление материалов было неудачным!";
        public const string IncorrectChildEventStart = "Событие не должно начинаться раньше родительского и заканчиваться позже";
        public const string StartAfterFinish = "Дата начала события должна быть раньше даты его окончания!";
        public const string ParentMustIncludeChild = "Даты события должны включать все дочерние!";
        public const string CapchaIncorrect = "Решите пример правильно и повторите попытку";
        public const string SkillIsEmpty = "Введите название навыка";
        public const string SkillIsTooLarge = "Сократите название навыка до 100 символов";
        public const string SkillIsExists = "Такой навык уже добавлен, введите другой";
        public const string SelectImage = "Загрузите изображение и выделите область для сохранения";
        public const string SelectAreaForImage = "Выделите область для сохранения";
        public const string RegionIsEmpty = "Введите название региона";
        public const string InvalidRegion = "Выберите корректный регион";
        public const string CityIsEmpty = "Введите название города";
        public const string IdeaTypeIsEmpty = "Введите название типа проекта";
        public const string UniversityDataWithoutTitle = "Введите название университета, если хотите добавить данные о нем";
        public const string MaterialDescriptionNeeded = "Введите тест для материала";
        
        //Error labels
        public const string LoginError = "login";
        public const string PasswordError = "password";
        public const string EmailError = "email";
        public const string IdeaGroupError = "ideaGroup";
        public const string DateError = "date";
        public const string ImageFormatError = "imageFormat";
        public const string PlatformTitleError = "platformTitle";
        public const string MaterialAddedError = "material";
        public const string CapchaError = "capcha";
        public const string SelectImageError = "selectImage";
        public const string SelectAreaForImageError = "areaSelectImage";
        public const string UniversityDataWithoutTitleError = "universityData";
        public const string MaterialDescriptionError = "materialText";

        //Event levels
        public const string EventLevel1 = "глобальное событие";
        public const string EventLevel2 = "этап события";
        public const string EventLevel3 = "часть расписания";

        //Classes for tip type
        public const string LeftTipClass = "leftTriangle";
        public const string RightTipClass = "rightTriangle";
        public const string TopTipClass = "topTriangle";
        public const string BottomTipClass = "bottomTriangle";

        //Message for MembershipCreateStatus
        public const string ChangePasswordSuccessMessage = "Пароль успешно изменен";
        public const string SetPasswordSuccessMessage = "Пароль успешно установлен";
        public const string RemoveLoginSuccessMessage = "Ассоциация с внешней учетной записью успешно удалена";
        public const string IncorrectOrWrongPasswordMessage = "Текущий пароль введен неправильно или некорректен новый пароль";
        public const string DuplicateUserNameMessage = "Логин уже занят. Пожалуйста выберите другой";
        public const string InvalidPasswordMessage = "Пароль введен неправильно. Пожалуйста повторите попытку";
        public const string InvalidEmailMessage = "Адрес электронно почты введен неправильно. Пожалуйста повторите попытку";
        public const string InvalidAnswerMessage = "The password retrieval answer provided is invalid. Please check the value and try again.";
        public const string InvalidQuestionMessage = "The password retrieval question provided is invalid. Please check the value and try again.";
        public const string InvalidUserNameMessage = "Имя пользователя введено неправильно. Повторите попытку.";
        public const string ProviderErrorMessage = "Провайдер вернул ошибку. Проверьте введенные данные";
        public const string UserRejectedMessage = "Ваш запрос был отменен. Проверьте введенные данные и повторите попытку";
        public const string DuplicateEmailMessage = "Адрес электронной почты уже существует. Проверьте корректность адреса и повторите попытку ";
        public const string UnknownErrorMessage =
            "Произошла неизвестная ошибка. Проверьте корректность введенных данных и повторите попытку. Если ошибка повторится обратитесь в службу поддержки";

        //Restore message
        public const string UnsuccessedAttemptToRestore = "Не удалось восстановить ваш профиль. Возможно вы забыли пароль, случайно удалили письмо или срок действия письма закончился.";

        //EventListType
        public const string PreviousEvents = "Прошедшие";
        public const string NextEvents = "Предстоящие";
        public const string AllEvents = "Все";

        //String for Image Loader
        public const string PromptToResizeYourImage = "Пожалуйста выделите область для сохранения";
        public const string TipsToPreview = "Вы можете посмотреть, как будет выглядеть ваша фотография на сайте";
        public const string TipsToBorderShow = "Показать рамку";
        public const string LoaderNameSuffix = "_loader";
        public const string PreviewCSSClass = "preview";
        public const string OnlyAfterUpoadingCSSClass = "imageLabel";
        public const string BorderSetterId = "borderSetter";
        public const string ChangeLinkForImageLoader = "Поменять";
        public const string CancelLinkForImageLoader = "Отменить";
        public const string RotateLinkForImageLoader = "Повернуть";
        public const string SaveLinkForImageLoader = "Сохранить";

        //Other string
        public const string Guest = "Гость";
        public const string TitleToRestoreMessage = "Восстановление на сайте YAppiDays";
        public const string TitleToAfterRegistrationMessage = "Подтверждение регистрации на сайте YAppiDays";
        public const string TitleForFeedback = "Feedback YAppiDays";
        public const string TitleForRestorePasswordMessage = "YAppiDays new password";
        public const string EmptyAnswerInAction = "no"; 
        public const string StatusOK = "ok";
        public const string WishedStatus = "wished";
        public const string CapchaSessionName = "Capcha";
        public const string TipSessionName = "Tip";
        public const string GridSessionName = "_grid";
        public const string ExternalMaterialLink = "external";
        public const string EmailForInternalUser = "internal_user@internal_user.int";
        public const string DefaultWelcome = "Добро пожаловать";
        public const string DefaultDescription = "YAPPi Days – крупнейшее в Ярославле и Северо-Западе России обучающее " +
                                                 "мероприятие/интенсив/хакатон, в ходе которого все желающие будут учиться " +
                                                 "и разрабатывать мобильные приложения под присмотром ведущих экспертов.";

        //Pretty date time string
        public const string JustNow = "только что";
        public const string MinuteAgo = "минуту назад";
        public const string SomeMinutes = " минуты назад";//less than 5
        public const string ManyMinutes = " минут назад";
        public const string HourAgo = "час назад";
        public const string SomeHour = " часa назад";//less than 5
        public const string ManyHour = " часов назад";
        public const string Yesterday = "вчера";
        public const string SomeDays = " дня назад";//less than 5
        public const string ManyDays = " дней назад";
        public const string SomeWeeks = " недели назад";//less than 5
        public const string Month = "месяц назад";
        public const string SomeMonth = " месяца назад";//less than 5
        public const string ManyMonth = " месяцев назад";
        public const string OneYear = "больше года назад";
        public const string MoreThan = "больше ";
        public const string ManyYears = " лет назад";

        //Title for word comment in Russian
        public const string OneComment = "комментарий";//1,21.... n1 (n>1)
        public const string SomeComments = "комментария";//2,3,4,... n2,n3,n4 (n>1)
        public const string ManyComments = "комментариев";//5-20, n0,...n5...n9 (n>1)
        public const string WithoutComments = "Без комментариев";//0

        public const string ShowCommentsTitle = "показать";
        public const string AddCommentsTitle = "добавить";
        public const string CloseCommentsTitle = "скрыть";

        //Get text of message for user which can to restore his account
        public static string MessageForUserToRestore(string userName, string linkToRestore)
        {
            return  string.Format("Уважаемый, {0}!<BR> Вы можете восстановить свою учетную запись. " +
                "Для восстановления перейдите по ссылке {1}<BR> С уважением Администрация Сайта.",userName,linkToRestore.AsLink());
        }

        //Get text of message for user after registration
        public static string MessageForUserAfterRegistration(string userName, string link)
        {
            return string.Format("Уважаемый, {0}!<BR> " +
                "Для завершения регистрации перейдите по ссылке {1}<BR> С уважением Администрация Сайта.", userName, link.AsLink());
        }

        //Get text of message for user with new password
        public static string MessageWithNewPassword(string userName, string login, string newPassword)
        {
            return string.Format("Уважаемый, {0}!<BR> Ваш логин: {1}<BR> Временный пароль: {2} <BR> Зайдите на сайт и " +
                "измените пароль.<BR> С уважением Администрация Сайта.", userName, login, newPassword);
        }
    }
}