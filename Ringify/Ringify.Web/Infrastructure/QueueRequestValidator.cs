namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Globalization;
    using System.Web;

    public class QueueRequestValidator : IStorageRequestValidator
    {
        private const string BaseUrl = "/AzureQueuesProxy.axd";

        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public QueueRequestValidator()
            : this(new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public QueueRequestValidator(IUserPrivilegesRepository userPrivilegesRepository)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        public bool DoesRequestApply(Uri resourceUri)
        {
            return resourceUri.AbsolutePath.StartsWith(BaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        public bool ValidateRequest(string userId, HttpRequest request)
        {
            if (!this.CanUseQueues(userId))
            {
                return false;
            }

            var queueName = StorageRequestAnalyzer.GetRequestedQueue(request);
            if (!this.CanUseQueue(userId, queueName, request))
            {
                return false;
            }

            return true;
        }

        private bool CanUseQueue(string userId, string queueName, HttpRequest request)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                return true;
            }

            var publicQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.PublicQueuePrivilegeSuffix);
            if (!this.userPrivilegesRepository.PublicPrivilegeExists(publicQueuePrivilege))
            {
                var accessQueuePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", queueName, PrivilegeConstants.QueuePrivilegeSuffix);
                if (!this.userPrivilegesRepository.HasUserPrivilege(userId, accessQueuePrivilege))
                {
                    // Check if the user is creating a new queue.
                    return StorageRequestAnalyzer.IsCreatingQueue(request);
                }
            }

            return true;
        }

        private bool CanUseQueues(string userId)
        {
            return this.userPrivilegesRepository.HasUserPrivilege(userId, PrivilegeConstants.QueuesUsagePrivilege);
        }
    }
}