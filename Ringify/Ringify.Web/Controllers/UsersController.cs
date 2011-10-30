namespace Ringify.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Models;
    using Ringify.Web.UserAccountWrappers;

    [CustomAuthorize(Roles = PrivilegeConstants.AdminPrivilege)]
    public class UsersController : Controller
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        private readonly IUserRepository userRepository;

        public UsersController()
            : this(new UserTablesServiceContext(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public UsersController(IUserPrivilegesRepository userPrivilegesRepository, IUserRepository userRepository)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
            this.userRepository = userRepository;
        }

        public ActionResult Index()
        {
            this.ViewData.Model = this.userRepository.GetAllUsers()
                .Select(user => new UserPermissionsModel
                {
                    UserName = user.Name,
                    UserId = user.UserId,
                    TablesUsage = this.userPrivilegesRepository.HasUserPrivilege(user.UserId, PrivilegeConstants.TablesUsagePrivilege),
                    BlobsUsage = this.userPrivilegesRepository.HasUserPrivilege(user.UserId, PrivilegeConstants.BlobsUsagePrivilege),
                    QueuesUsage = this.userPrivilegesRepository.HasUserPrivilege(user.UserId, PrivilegeConstants.QueuesUsagePrivilege)
                });

            return this.View();
        }

        [HttpPost]
        public void SetUserPermissions(string userId, bool useTables, bool useBlobs, bool useQueues)
        {
            this.SetStorageItemUsagePrivilege(useTables, userId, PrivilegeConstants.TablesUsagePrivilege);
            this.SetStorageItemUsagePrivilege(useBlobs, userId, PrivilegeConstants.BlobsUsagePrivilege);
            this.SetStorageItemUsagePrivilege(useQueues, userId, PrivilegeConstants.QueuesUsagePrivilege);
        }

        private void SetStorageItemUsagePrivilege(bool allowAccess, string user, string privilege)
        {
            if (allowAccess)
            {
                this.userPrivilegesRepository.AddPrivilegeToUser(user, privilege);
            }
            else
            {
                this.userPrivilegesRepository.RemovePrivilegeFromUser(user, privilege);
            }
        }
    }
}
