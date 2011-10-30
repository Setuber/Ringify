namespace Ringify.Web.Handlers
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Web;
    using Microsoft.WindowsAzure;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Serializers;

    public class AzureQueuesProxyHandler : StorageProxyHandler
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public AzureQueuesProxyHandler() :
            this(new UserTablesServiceContext(), CloudStorageAccount.FromConfigurationSetting("DataConnectionString"), new FormatSerializerFactory())
        {
        }

        [CLSCompliant(false)]
        public AzureQueuesProxyHandler(IUserPrivilegesRepository userPrivilegesRepository, CloudStorageAccount cloudStorageAccount, IFormatSerializerFactory formatSerializerFactory)
            : base(cloudStorageAccount, formatSerializerFactory)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        protected override string AzureStorageUrl
        {
            get { return this.CloudStorageAccount.QueueEndpoint.ToString().TrimEnd('/'); }
        }

        protected override void SignRequest(WebRequest webRequest)
        {
            var httpWebRequest = webRequest as HttpWebRequest;
            if (httpWebRequest != null)
            {
                this.CloudStorageAccount.Credentials.SignRequest(httpWebRequest);
            }
        }

        protected override void PostProcessProxyRequest(HttpContext context)
        {
            var queueName = StorageRequestAnalyzer.GetRequestedQueue(context.Request);

            if ((context.Response.StatusCode == (int)HttpStatusCode.Created) && StorageRequestAnalyzer.IsCreatingQueue(context.Request))
            {
                // A new queue was created -> add permissions to the current user.
                this.AddQueuePermissions(queueName, this.UserId);
            }
            else if ((context.Response.StatusCode == (int)HttpStatusCode.NoContent) && StorageRequestAnalyzer.IsDeletingQueue(context.Request))
            {
                // A queue was deleted -> remove all permissions to that queue.
                this.RemoveAllQueuePermissions(queueName);
            }
        }

        private void AddQueuePermissions(string queueName, string userName)
        {
            if (!string.IsNullOrEmpty(queueName))
            {
                var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.QueuePrivilegeSuffix);
                this.userPrivilegesRepository.AddPrivilegeToUser(userName, accessQueuePrivilege);
            }
        }

        private void RemoveAllQueuePermissions(string queueName)
        {
            if (!string.IsNullOrEmpty(queueName))
            {
                var publicQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.PublicQueuePrivilegeSuffix);
                this.userPrivilegesRepository.DeletePublicPrivilege(publicQueuePrivilege);

                var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.QueuePrivilegeSuffix);
                this.userPrivilegesRepository.DeletePrivilege(accessQueuePrivilege);
            }
        }
    }
}
