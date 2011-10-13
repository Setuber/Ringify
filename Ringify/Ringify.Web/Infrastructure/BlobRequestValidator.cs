namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Web;

    public class BlobRequestValidator : IStorageRequestValidator
    {
        private const string BaseUrl = "/SharedAccessSignatureService";

        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public BlobRequestValidator()
            : this(new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public BlobRequestValidator(IUserPrivilegesRepository userPrivilegesRepository)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        public bool DoesRequestApply(Uri resourceUri)
        {
            return resourceUri.AbsolutePath.StartsWith(BaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        public bool ValidateRequest(string userId, HttpRequest request)
        {
            return this.CanUseBlobs(userId);
        }

        private bool CanUseBlobs(string userId)
        {
            return this.userPrivilegesRepository.HasUserPrivilege(userId, PrivilegeConstants.BlobsUsagePrivilege);
        }
    }
}