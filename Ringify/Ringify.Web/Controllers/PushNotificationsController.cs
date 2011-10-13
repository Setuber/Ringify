namespace Ringify.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Web.Mvc;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using WindowsPhone.Recipes.Push.Messages;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Models;
    using Ringify.Web.UserAccountWrappers;

    [CustomAuthorize(Roles = PrivilegeConstants.AdminPrivilege)]
    public class PushNotificationsController : Controller
    {
        private readonly CloudQueueClient cloudQueueClient;
        private readonly IPushUserEndpointsRepository pushUserEndpointsRepository;
        private readonly IUserRepository userRepository;

        public PushNotificationsController()
            : this(null, new UserTablesServiceContext(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public PushNotificationsController(CloudQueueClient cloudQueueClient, IPushUserEndpointsRepository pushUserEndpointsRepository, IUserRepository userRepository)
        {
            if (GetStorageAccountFromConfigurationSetting() == null)
            {
                if (cloudQueueClient == null)
                {
                    throw new ArgumentNullException("cloudQueueClient", "Cloud Queue Client cannot be null if no configuration is loaded.");
                }
            }

            this.cloudQueueClient = cloudQueueClient ?? GetStorageAccountFromConfigurationSetting().CreateCloudQueueClient();
            this.userRepository = userRepository;
            this.pushUserEndpointsRepository = pushUserEndpointsRepository;
        }

        public ActionResult Microsoft()
        {
            this.ViewData.Model = this.pushUserEndpointsRepository
                .GetAllPushUsers()
                .Select(userId => new UserModel { UserId = userId, UserName = this.userRepository.GetUser(userId).Name });

            return this.View();
        }



        [HttpPost]
        public ActionResult SendMicrosoftToast(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return this.Json("The notification message cannot be null, empty nor white space.", JsonRequestBehavior.AllowGet);
            }

            var resultList = new List<MessageSendResultLight>();
            var uris = this.pushUserEndpointsRepository.GetPushUsersByName(userId).Select(u => u.ChannelUri);
            var toast = new ToastPushNotificationMessage
            {
                SendPriority = MessageSendPriority.High,
                Title = message
            };

            foreach (var uri in uris)
            {
                var messageResult = toast.SendAndHandleErrors(new Uri(uri));
                resultList.Add(messageResult);
                if (messageResult.Status.Equals(MessageSendResultLight.Success))
                {
                    this.QueueMessage(message, new Uri(uri));
                }
            }

            return this.Json(resultList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SendMicrosoftTile(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return this.Json("The notification message cannot be null, empty nor white space.", JsonRequestBehavior.AllowGet);
            }

            var resultList = new List<MessageSendResultLight>();
            var pushUserEndpointList = this.pushUserEndpointsRepository.GetPushUsersByName(userId);
            foreach (var pushUserEndpoint in pushUserEndpointList)
            {
                var tile = new TilePushNotificationMessage
                {
                    SendPriority = MessageSendPriority.High,
                    Count = ++pushUserEndpoint.TileCount
                };

                var messageResult = tile.SendAndHandleErrors(new Uri(pushUserEndpoint.ChannelUri));
                resultList.Add(messageResult);
                if (messageResult.Status.Equals(MessageSendResultLight.Success))
                {
                    this.QueueMessage(message, new Uri(pushUserEndpoint.ChannelUri));

                    this.pushUserEndpointsRepository.UpdatePushUserEndpoint(pushUserEndpoint);
                }
            }

            return this.Json(resultList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SendMicrosoftRaw(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return this.Json("The notification message cannot be null, empty nor white space.", JsonRequestBehavior.AllowGet);
            }

            var resultList = new List<MessageSendResultLight>();
            var uris = this.pushUserEndpointsRepository.GetPushUsersByName(userId).Select(u => u.ChannelUri);
            var raw = new RawPushNotificationMessage
            {
                SendPriority = MessageSendPriority.High,
                RawData = Encoding.UTF8.GetBytes(message)
            };

            foreach (var uri in uris)
            {
                resultList.Add(raw.SendAndHandleErrors(new Uri(uri)));
            }

            return this.Json(resultList, JsonRequestBehavior.AllowGet);
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

        private void QueueMessage(string message, Uri uri)
        {
            var queueName = string.Format(CultureInfo.InvariantCulture, "notification{0}", uri.GetHashCode());
            var queue = this.cloudQueueClient.GetQueueReference(queueName);

            queue.CreateIfNotExist();
            queue.AddMessage(new CloudQueueMessage(message));
        }
    }
}
