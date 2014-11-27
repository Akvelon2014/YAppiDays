using MobileConference.Enums;

namespace MobileConference.Models
{
    public class IdeaWithPermission
    {
        private PermissionType permission;

        public PermissionType Permission { get { return permission; } }
        public IdeasModel Idea {get; set; }
        public bool DisplayOnly { get; set; }
        public bool DisplayAsMain { get; set; }
        public bool Wished { get; set; }
        public bool Invited { get; set; }

        public IdeaWithPermission(PermissionType permission)
        {
            this.permission = permission;
        }
    }
}