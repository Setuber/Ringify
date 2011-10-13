namespace Ringify.Web.Handlers
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Web;
    using Microsoft.WindowsAzure;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Serializers;

    public class AzureTablesProxyHandler : StorageProxyHandler
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public AzureTablesProxyHandler() :
            this(new UserTablesServiceContext(), CloudStorageAccount.FromConfigurationSetting("DataConnectionString"), new FormatSerializerFactory())
        {
        }

        [CLSCompliant(false)]
        public AzureTablesProxyHandler(IUserPrivilegesRepository userPrivilegesRepository, CloudStorageAccount cloudStorageAccount, IFormatSerializerFactory formatSerializerFactory)
            : base(cloudStorageAccount, formatSerializerFactory)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        protected override string AzureStorageUrl
        {
            get { return this.CloudStorageAccount.TableEndpoint.ToString().TrimEnd('/'); }
        }

        protected override void SignRequest(WebRequest webRequest)
        {
            var httpWebRequest = webRequest as HttpWebRequest;
            if (httpWebRequest != null)
            {
                this.CloudStorageAccount.Credentials.SignRequestLite(httpWebRequest);
            }
        }

        protected override void PostProcessProxyRequest(HttpContext context)
        {
            var tableName = StorageRequestAnalyzer.GetRequestedTable(context.Request);

            if ((context.Response.StatusCode == (int)HttpStatusCode.Created) && StorageRequestAnalyzer.IsCreatingTable(context.Request, tableName))
            {
                // A new table was created -> add permissions to the current user.
                this.AddTablePermissions(StorageRequestAnalyzer.GetTableToCreate(context.Request), this.UserId);
            }
            else if ((context.Response.StatusCode == (int)HttpStatusCode.NoContent) && StorageRequestAnalyzer.IsDeletingTable(context.Request, tableName))
            {
                // A table was deleted -> remove all permissions to that table.
                this.RemoveAllTablePermissions(tableName);
            }
        }

        private void AddTablePermissions(string tableName, string userName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.TablePrivilegeSuffix);
                this.userPrivilegesRepository.AddPrivilegeToUser(userName, accessTablePrivilege);
            }
        }

        private void RemoveAllTablePermissions(string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                var publicTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.PublicTablePrivilegeSuffix);
                this.userPrivilegesRepository.DeletePublicPrivilege(publicTablePrivilege);

                var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.TablePrivilegeSuffix);
                this.userPrivilegesRepository.DeletePrivilege(accessTablePrivilege);
            }
        }
    }
}
