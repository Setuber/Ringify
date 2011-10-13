namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Web;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Ringify.Web.Models;

    public class UserTablesServiceContext : TableServiceContext, IPushUserEndpointsRepository, IUserPrivilegesRepository, IUserRepository
    {
        public const string UserTableName = "Users";

        public const string PushUserTableName = "PushUserEndpoints";

        public const string UserPrivilegeTableName = "UserPrivileges";

        private const string PublicUserId = "00000000-0000-0000-0000-000000000000";

        public UserTablesServiceContext()
            : this(CloudStorageAccount.FromConfigurationSetting("DataConnectionString"))
        {
        }

        public UserTablesServiceContext(CloudStorageAccount account)
            : this(account.TableEndpoint.ToString(), account.Credentials)
        {
        }

        public UserTablesServiceContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
            this.IgnoreResourceNotFoundException = true;
            this.IgnoreMissingProperties = true;
        }

        public IQueryable<PushUserEndpoint> PushUserEndpoints
        {
            get
            {
                return this.CreateQuery<PushUserEndpoint>(PushUserTableName);
            }
        }

        public IQueryable<UserPrivilege> UserPrivileges
        {
            get
            {
                return this.CreateQuery<UserPrivilege>(UserPrivilegeTableName);
            }
        }

        public IQueryable<User> Users
        {
            get
            {
                return this.CreateQuery<User>(UserTableName);
            }
        }

        public void CreateUser(string userId, string userName, string email)
        {
            if (this.Users
                .Where(u => u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase) || u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable()
                .FirstOrDefault() != null)
            {
                throw new WebFaultException<string>("A user with the same id or email already exists.", HttpStatusCode.BadRequest);
            }

            this.AddObject(UserTableName, new User { UserId = userId, Name = userName, Email = email });

            this.SaveChanges();
        }

        public IEnumerable<User> GetAllUsers()
        {
            return this.Users.ToList();
        }

        [CLSCompliant(false)]
        public User GetUser(string userId)
        {
            return this.Users
                .Where(u => u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable()
                .FirstOrDefault();
        }

        [CLSCompliant(false)]
        public User GetUserByEmail(string email)
        {
            return this.Users
                .Where(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable()
                .FirstOrDefault();
        }

        public bool UserExists(string userId)
        {
            return this.GetUser(userId) != null;
        }

        public void AddPushUserEndpoint(string userId, Uri channelUri)
        {
            this.AddObject(PushUserTableName, new PushUserEndpoint { UserId = userId, ChannelUri = channelUri.ToString(), TileCount = 0 });

            this.SaveChanges();
        }

        public void RemovePushUserEndpoint(string userId, Uri channelUri)
        {
            var pushUserEnpoints = this.GetPushUsersByNameAndEndpoint(userId, channelUri);
            foreach (var pushUserEnpoint in pushUserEnpoints)
            {
                this.DeleteObject(pushUserEnpoint);
            }

            this.SaveChanges();
        }

        public IEnumerable<string> GetAllPushUsers()
        {
            return this.PushUserEndpoints
                .ToList()
                .GroupBy(u => u.UserId)
                .Select(g => g.Key);
        }

        public IEnumerable<PushUserEndpoint> GetPushUsersByName(string userId)
        {
            return this.PushUserEndpoints
                .Where(u => u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
        }

        public IEnumerable<PushUserEndpoint> GetPushUsersByNameAndEndpoint(string userId, Uri channelUri)
        {
            return this.PushUserEndpoints
                .Where(u => u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase) && u.ChannelUri.Equals(channelUri.ToString(), StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
        }

        [CLSCompliant(false)]
        public void UpdatePushUserEndpoint(PushUserEndpoint pushUserEndpoint)
        {
            this.UpdateObject(pushUserEndpoint);

            this.SaveChanges();
        }

        public IEnumerable<UserPrivilege> GetUsersWithPrivilege(string privilege)
        {
            return this.UserPrivileges
                .Where(p => p.Privilege.Equals(privilege, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable();
        }

        public void AddPrivilegeToUser(string userId, string privilege)
        {
            if (!this.HasUserPrivilege(userId, privilege))
            {
                this.AddObject(UserPrivilegeTableName, new UserPrivilege { UserId = userId, Privilege = privilege });

                this.SaveChanges();
            }
        }

        public void AddPublicPrivilege(string privilege)
        {
            this.AddPrivilegeToUser(PublicUserId, privilege);
        }

        public void RemovePrivilegeFromUser(string userId, string privilege)
        {
            var userPrivilege = this.GetUserPrivilege(userId, privilege);
            if (userPrivilege != null)
            {
                this.DeleteObject(userPrivilege);

                this.SaveChanges();
            }
        }

        public void DeletePublicPrivilege(string privilege)
        {
            this.RemovePrivilegeFromUser(PublicUserId, privilege);
        }

        public bool HasUserPrivilege(string userId, string privilege)
        {
            return this.GetUserPrivilege(userId, privilege) != null;
        }

        public bool PublicPrivilegeExists(string privilege)
        {
            return this.HasUserPrivilege(PublicUserId, privilege);
        }

        public void DeletePrivilege(string privilege)
        {
            var userPrivileges = this.GetUsersWithPrivilege(privilege);
            foreach (var userPrivilege in userPrivileges)
            {
                this.DeleteObject(userPrivilege);
            }

            this.SaveChanges();
        }

        private UserPrivilege GetUserPrivilege(string userId, string privilege)
        {
            return this.UserPrivileges
                .Where(p => p.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase) && p.Privilege.Equals(privilege, StringComparison.OrdinalIgnoreCase))
                .AsEnumerable()
                .FirstOrDefault();
        }
    }
}