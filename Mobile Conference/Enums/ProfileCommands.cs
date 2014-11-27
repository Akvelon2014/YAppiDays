namespace MobileConference.Enums
{
    public enum ProfileCommands
    {
        Login = 0,
        Logoff = 1,
        Register = 2,
        ProfileChange = 3,
        Admin = 4,
        MyIdea = 5,
        IdeasWithoutMentor = 6,
        MentorPage = 7,
        ChangePassword = 8,
        AccountsManage = 9
    }


    public static class ExtensionForProfileCommand
    {
        public static string GetValue(this ProfileCommands profileCommand)
        {
            return ((int) profileCommand).ToString();
        }
    }
}