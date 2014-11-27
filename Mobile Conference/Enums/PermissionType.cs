using System;

namespace MobileConference.Enums
{
    /// <summary>
    /// Flag (if you would added new type, you shoul used power of two)
    /// </summary>
    [Flags]
    public enum PermissionType:int
    {
        None = 0,
        ChangeProfile = 1,
        UseChat =2,
        ShowPhotos = 4,
        DeletePhotos = 8,
        AsLeader = 16,
        AsAdmin = 32,
        AsMember = 64,
        AsMentor = 128,
        ShowChat = 256,
        AddPhotos = 512,
        ShowOfficialReports = 1024
    }
}