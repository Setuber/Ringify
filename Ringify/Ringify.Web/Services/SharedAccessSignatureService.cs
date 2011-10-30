namespace Ringify.Web.Services
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web;
    using System.Web.Security;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.UserAccountWrappers;

    [ServiceBehavior(IncludeExceptionDetailInFaults = false)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class SharedAccessSignatureService : ISharedAccessSignatureService
    {
        private const SharedAccessPermissions ContainerSharedAccessPermissions = SharedAccessPermissions.Write | SharedAccessPermissions.Delete | SharedAccessPermissions.List;

        private readonly CloudBlobClient cloudBlobClient;
        private readonly WebOperationContext webOperationContext;

        public SharedAccessSignatureService()
            : this(null, WebOperationContext.Current)
        {
        }

        [CLSCompliant(false)]
        public SharedAccessSignatureService(CloudBlobClient cloudBlobClient, WebOperationContext webOperationContext)
        {
            if ((cloudBlobClient == null) && (GetStorageAccountFromConfigurationSetting() == null))
            {
                throw new ArgumentNullException("cloudBlobClient", "The Cloud Blob Client cannot be null if no configuration is loaded.");
            }

            this.cloudBlobClient = cloudBlobClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudBlobClient();
            this.webOperationContext = webOperationContext;
        }

        private string UserId
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as Microsoft.IdentityModel.Claims.IClaimsIdentity;
                return identity.Claims.Single(c => c.ClaimType == Microsoft.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
            }
        }

        public Uri GetContainerSharedAccessSignature()
        {
            // Authenticate.
            var userId = this.UserId;

            try
            {
                // Each user has its own container.
                var container = this.GetUserContainer(userId);
                var containerSASExperiationTime = int.Parse(ConfigReader.GetConfigValue("ContainerSASExperiationTime"), NumberStyles.Integer, CultureInfo.InvariantCulture);
                var sas = container.GetSharedAccessSignature(new SharedAccessPolicy()
                {
                    Permissions = ContainerSharedAccessPermissions,
                    SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(containerSASExperiationTime)
                });

                if (this.webOperationContext != null)
                {
                    this.webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
                }

                var uriBuilder = new UriBuilder(container.Uri) { Query = sas.TrimStart('?') };
                return uriBuilder.Uri;
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        public Models.CloudBlobCollection GetBlobsSharedAccessSignatures(string blobPrefix, bool useFlatBlobListing)
        {
            // Authenticate.
            var userId = this.UserId;

            if (!string.IsNullOrEmpty(blobPrefix))
            {
                blobPrefix = blobPrefix.TrimStart('/', '\\').Replace('\\', '/');
            }

            try
            {
                // Each user has its own container.
                SetReadOnlySharedAccessPolicy(this.GetUserContainer(userId));
                var prefix = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", GetUserContainerName(userId), blobPrefix);

                var blobs = this.cloudBlobClient.ListBlobsWithPrefix(prefix, new BlobRequestOptions { UseFlatBlobListing = useFlatBlobListing });
                var result = new Models.CloudBlobCollection
                {
                    Blobs = blobs.Where(b => b is CloudBlob)
                                .Select(b => b.ToModel(GetUserContainerName(userId), this.cloudBlobClient.Credentials.AccountName))
                                .ToArray()
                };

                if (this.webOperationContext != null)
                {
                    this.webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
                }

                return result;
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(exception.Message, HttpStatusCode.InternalServerError);
            }
        }

        private static void SetReadOnlySharedAccessPolicy(CloudBlobContainer container)
        {
            var blobSASExperiationTime = int.Parse(ConfigReader.GetConfigValue("BlobSASExperiationTime"), NumberStyles.Integer, CultureInfo.InvariantCulture);
            var permissions = container.GetPermissions();
            var options = new BlobRequestOptions
            {
                // Fail if someone else has already changed the container before we do.
                AccessCondition = AccessCondition.IfMatch(container.Properties.ETag)
            };
            var sharedAccessPolicy = new SharedAccessPolicy
            {
                Permissions = SharedAccessPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromDays(blobSASExperiationTime)
            };

            permissions.SharedAccessPolicies.Remove("readonly");
            permissions.SharedAccessPolicies.Add("readonly", sharedAccessPolicy);

            container.SetPermissions(permissions, options);
        }

        private static CloudStorageAccount GetStorageAccountFromConfigurationSetting()
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

        private static string GetUserContainerName(string userId)
        {
            // The container name for the user contains a hash of the user Id.
            var containerName = string.Format(CultureInfo.InvariantCulture, "usercontainer{0}", userId.GetHashCode());

            return containerName.ToLowerInvariant();
        }

        private CloudBlobContainer GetUserContainer(string userId)
        {
            var container = this.cloudBlobClient.GetContainerReference(GetUserContainerName(userId));
            container.CreateIfNotExist();

            return container;
        }
    }
}
