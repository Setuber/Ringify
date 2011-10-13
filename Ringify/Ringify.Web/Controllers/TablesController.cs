namespace Ringify.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;
    using Microsoft.WindowsAzure.StorageClient;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Models;
    using Ringify.Web.UserAccountWrappers;

    public class TablesController : StorageItemController
    {
        private readonly CloudTableClient cloudTableClient;

        private readonly IUserRepository userRepository;

        public TablesController()
            : this(null, new UserTablesServiceContext(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public TablesController(CloudTableClient cloudTableClient, IUserPrivilegesRepository userPrivilegesRepository, IUserRepository userRepository)
            : base(userPrivilegesRepository)
        {
            if ((GetStorageAccountFromConfigurationSetting() == null) && (cloudTableClient == null))
            {
                throw new ArgumentNullException("cloudTableClient", "Cloud Table Client cannot be null if no configuration is loaded.");
            }

            this.cloudTableClient = cloudTableClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudTableClient();
            this.userRepository = userRepository;
        }

        public ActionResult Index()
        {
            var permissions = new List<StorageItemPermissionsModel>();
            var tables = this.cloudTableClient.ListTables();
            foreach (var tableName in tables)
            {
                if (!tableName.Equals(Microsoft.Samples.ServiceHosting.AspProviders.AspProvidersConfiguration.DefaultMembershipTableName, StringComparison.OrdinalIgnoreCase) &&
                    !tableName.Equals(UserTablesServiceContext.UserTableName, StringComparison.OrdinalIgnoreCase) &&
                    !tableName.Equals(UserTablesServiceContext.UserPrivilegeTableName, StringComparison.OrdinalIgnoreCase) &&
                    !tableName.Equals(UserTablesServiceContext.PushUserTableName, StringComparison.OrdinalIgnoreCase))
                {
                    var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.TablePrivilegeSuffix);
                    var publicTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.PublicTablePrivilegeSuffix);
                    permissions.Add(new StorageItemPermissionsModel
                    {
                        StorageItemName = tableName,
                        IsPublic = this.UserPrivilegesRepository.PublicPrivilegeExists(publicTablePrivilege),
                        AllowedUserIds = this.UserPrivilegesRepository.GetUsersWithPrivilege(accessTablePrivilege).Select(us => us.UserId)
                    });
                }
            }

            this.ViewData.Model = permissions;
            this.ViewData["users"] = this.userRepository.GetAllUsers()
                .Select(user => new UserModel { UserName = user.Name, UserId = user.UserId });

            return this.View();
        }

        [HttpPost]
        public void AddTablePermission(string table, string userId)
        {
            var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", table, PrivilegeConstants.TablePrivilegeSuffix);
            this.AddPrivilegeToUser(userId, accessTablePrivilege);
        }

        [HttpPost]
        public void RemoveTablePermission(string table, string userId)
        {
            var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", table, PrivilegeConstants.TablePrivilegeSuffix);
            this.RemovePrivilegeFromUser(userId, accessTablePrivilege);
        }

        [HttpPost]
        public void SetTablePublic(string table, bool isPublic)
        {
            var publicTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", table, PrivilegeConstants.PublicTablePrivilegeSuffix);
            this.SetPublicPrivilege(publicTablePrivilege, isPublic);
        }
    }
}
