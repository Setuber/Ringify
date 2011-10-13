namespace Ringify.Web.Services
{
    using System;
    using System.Collections.Generic;
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
    using WindowsPhone.Recipes.Push.Messages;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.UserAccountWrappers;

    [ServiceBehavior(IncludeExceptionDetailInFaults = false)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class SamplePushUserRegistrationService : ISamplePushUserRegistrationService
    {
        private readonly HttpContextBase context;
        private readonly IPushUserEndpointsRepository pushUserEndpointsRepository;
        private readonly CloudQueueClient cloudQueueClient;
        private readonly WebOperationContext webOperationContext;

        public SamplePushUserRegistrationService()
            : this(new HttpContextWrapper(HttpContext.Current), WebOperationContext.Current, new UserTablesServiceContext(), null)
        {
        }

        [CLSCompliant(false)]
        public SamplePushUserRegistrationService(HttpContextBase context, WebOperationContext webOperationContext, IPushUserEndpointsRepository pushUserEndpointsRepository, CloudQueueClient cloudQueueClient)
        {
            if ((context == null) && (HttpContext.Current == null))
            {
                throw new ArgumentNullException("context", "Context cannot be null if not running on a Web context.");
            }

            if (pushUserEndpointsRepository == null)
            {
                throw new ArgumentNullException("pushUserEndpointsRepository", "PushUserEndpoints repository cannot be null.");
            }

            if ((cloudQueueClient == null) && (GetStorageAccountFromConfigurationSetting() == null))
            {
                throw new ArgumentNullException("cloudQueueClient", "Cloud Queue Client cannot be null if no configuration is loaded.");
            }

            this.cloudQueueClient = cloudQueueClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudQueueClient();
            this.webOperationContext = webOperationContext;
            this.context = context;
            this.pushUserEndpointsRepository = pushUserEndpointsRepository;
        }

        private string UserId
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as Microsoft.IdentityModel.Claims.IClaimsIdentity;
                return identity.Claims.Single(c => c.ClaimType == Microsoft.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value;
            }
        }

        public string Register(Uri channelUri)
        {
            // Authenticate.
            var userId = this.UserId;

            try
            {
                if (this.pushUserEndpointsRepository.GetPushUsersByNameAndEndpoint(userId, channelUri).Count() == 0)
                {
                    this.pushUserEndpointsRepository.AddPushUserEndpoint(userId, channelUri);
                }
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(string.Format(CultureInfo.InvariantCulture, "There was an error registering the Push Notification Endpoint: {0}", exception.Message), HttpStatusCode.InternalServerError);
            }

            return "Success";
        }

        public string Unregister(Uri channelUri)
        {
            // Authenticate.
            var userId = this.UserId;

            try
            {
                this.pushUserEndpointsRepository.RemovePushUserEndpoint(userId, channelUri);
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(string.Format(CultureInfo.InvariantCulture, "There was an error unregistering the Push Notification Endpoint: {0}", exception.Message), HttpStatusCode.InternalServerError);
            }

            return "Success";
        }

        public string[] GetUpdates(Uri channelUri)
        {
            // Authenticate.
            var userId = this.UserId;

            this.webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");

            try
            {
                var queueName = string.Format(CultureInfo.InvariantCulture, "notification{0}", channelUri.GetHashCode());
                var queue = this.cloudQueueClient.GetQueueReference(queueName);
                var messages = new List<string>();
                if (queue.Exists())
                {
                    var message = queue.GetMessage();
                    while (message != null)
                    {
                        messages.Add(message.AsString);
                        queue.DeleteMessage(message);
                        message = queue.GetMessage();
                    }
                }

                this.ResetTileNotificationCount(channelUri);

                return messages.ToArray();
            }
            catch (Exception exception)
            {
                throw new WebFaultException<string>(string.Format(CultureInfo.InvariantCulture, "There was an error getting the push notification updates: {0}", exception.Message), HttpStatusCode.InternalServerError);
            }
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

        private void ResetTileNotificationCount(Uri channelUri)
        {
            var pushUserEndpointList = this.pushUserEndpointsRepository.GetPushUsersByNameAndEndpoint(this.UserId, channelUri);

            foreach (var pushUserEndpoint in pushUserEndpointList)
            {
                pushUserEndpoint.TileCount = 0;

                var tile = new TilePushNotificationMessage
                {
                    SendPriority = MessageSendPriority.High,
                    Count = pushUserEndpoint.TileCount
                };

                // Send a new tile notification message to reset the count in the phone application.
                tile.SendAndHandleErrors(new Uri(pushUserEndpoint.ChannelUri));

                // Save the updated count.
                this.pushUserEndpointsRepository.UpdatePushUserEndpoint(pushUserEndpoint);
            }
        }
    }
}