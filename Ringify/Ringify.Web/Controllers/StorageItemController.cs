namespace Ringify.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using Microsoft.WindowsAzure;
    using Ringify.Web.Infrastructure;

    [CustomAuthorize(Roles = PrivilegeConstants.AdminPrivilege)]
    public abstract class StorageItemController : Controller
    {
        [CLSCompliant(false)]
        public StorageItemController(IUserPrivilegesRepository userPrivilegesRepository)
        {
            this.UserPrivilegesRepository = userPrivilegesRepository;
        }

        [CLSCompliant(false)]
        protected IUserPrivilegesRepository UserPrivilegesRepository { get; private set; }

        protected static CloudStorageAccount GetStorageAccountFromConfigurationSetting()
        {
            CloudStorageAccount account = null;
            try
            {
                account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            }
            catch (InvalidOperationException)
            {
                account = null;
            }

            return account;
        }

        protected void RemovePrivilegeFromUser(string user, string privilege)
        {
            this.UserPrivilegesRepository.RemovePrivilegeFromUser(user, privilege);
        }

        protected void AddPrivilegeToUser(string user, string privilege)
        {
            this.UserPrivilegesRepository.AddPrivilegeToUser(user, privilege);
        }

        protected void SetPublicPrivilege(string privilege, bool isPublic)
        {
            if (isPublic)
            {
                this.UserPrivilegesRepository.AddPublicPrivilege(privilege);
            }
            else
            {
                this.UserPrivilegesRepository.DeletePublicPrivilege(privilege);
            }
        }
    }
}
