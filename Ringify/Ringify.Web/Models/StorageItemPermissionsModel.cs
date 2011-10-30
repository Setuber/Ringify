namespace Ringify.Web.Models
{
    using System.Collections.Generic;

    public class StorageItemPermissionsModel
    {
        public string StorageItemName { get; set; }

        public IEnumerable<string> AllowedUserIds { get; set; }

        public bool IsPublic { get; set; }
    }
}