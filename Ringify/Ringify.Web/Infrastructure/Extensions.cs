namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Activation;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    using WindowsPhone.Recipes.Push.Messages;

    public static class Extensions
    {
        private const string ErrorResponse = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?><error {0}><code>{1}</code><message xml:lang=\"en-US\">{2}</message></error>";
        private const string DataServiceNamespace = "xmlns=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"";

        public static Models.CloudBlob ToModel(this IListBlobItem blobItem, string containerName, string accountName)
        {
            var blob = blobItem as Microsoft.WindowsAzure.StorageClient.CloudBlob;
            var sas = blob.GetSharedAccessSignature(new SharedAccessPolicy(), "readonly");
            var uriBuilder = new UriBuilder(blob.Uri) { Query = sas.TrimStart('?') };
            var blobUri = uriBuilder.Uri;
            var blobName = blobUri.LocalPath;

            // When using the Storage Emulator the first segment is the dev account, not the container.
            var devaccount = CloudStorageAccount.DevelopmentStorageAccount.Credentials.AccountName;
            if (accountName.Equals(devaccount, StringComparison.OrdinalIgnoreCase)
                && blobName.StartsWith(string.Format(CultureInfo.InvariantCulture, "/{0}", devaccount), StringComparison.OrdinalIgnoreCase))
            {
                blobName = blobName.Remove(0, devaccount.Length + 1);
            }

            return new Models.CloudBlob
            {
                // Remove container name.
                Name = blobName.Remove(0, containerName.Length + 1).TrimStart('/'),
                Uri = blobUri
            };
        }

        [CLSCompliant(false)]
        public static MessageSendResultLight SendAndHandleErrors(this PushNotificationMessage message, Uri uri)
        {
            var result = default(MessageSendResultLight);
            try
            {
                var sendResult = message.Send(uri);
                result = sendResult.NotificationStatus == NotificationStatus.Received
                    ? new MessageSendResultLight { Status = MessageSendResultLight.Success }
                    : new MessageSendResultLight { Status = MessageSendResultLight.Error, Description = "The notification was not received by the device." };
            }
            catch (Exception exception)
            {
                result = new MessageSendResultLight { Status = MessageSendResultLight.Error, Description = exception.Message };
            }

            return result;
        }

        public static string MenuItem(this HtmlHelper helper, string linkText, string actionName, string controllerName)
        {
            var currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];
            var currentActionName = (string)helper.ViewContext.RouteData.Values["action"];
            var builder = new StringBuilder();

            if (currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) && currentActionName.Equals(actionName, StringComparison.CurrentCultureIgnoreCase))
            {
                builder.Append("<li class=\"selected\">");
            }
            else
            {
                builder.Append("<li>");
            }

            builder.Append(helper.ActionLink(linkText, actionName, controllerName));
            builder.Append("</li>");

            return builder.ToString();
        }

        public static void EndWithDataServiceError(this HttpResponse response, int code, string error, string detail)
        {
            response.Clear();
            response.ContentType = HttpConstants.MimeApplicationAtomXml;
            response.StatusCode = code;
            response.StatusDescription = error;
            response.Write(string.Format(
                CultureInfo.InvariantCulture,
                ErrorResponse,
                DataServiceNamespace,
                error,
                detail));
            response.Flush();
            response.End();
            response.Clear();
        }

        public static void CopyRequestHeadersTo(this HttpRequest sourceRequest, WebRequest destinationRequest)
        {
            destinationRequest.ContentType = sourceRequest.ContentType;

            var finalHttpWebRequest = destinationRequest as HttpWebRequest;
            if (finalHttpWebRequest != null)
            {
                var connection = sourceRequest.Headers["Connection"];
                finalHttpWebRequest.KeepAlive = !string.IsNullOrEmpty(connection) && connection.Equals("Close", StringComparison.OrdinalIgnoreCase) ? false : true;
                finalHttpWebRequest.Accept = HttpConstants.MimeApplicationAtomXml;
                finalHttpWebRequest.Referer = sourceRequest.Headers["Referer"];
                finalHttpWebRequest.UserAgent = sourceRequest.Headers["User-Agent"];
            }

            string[] excludeHeaders =
                {
                    "Connection",
                    "Accept",
                    "User-Agent",
                    "Host",
                    "Authorization",
                    "AuthToken",
                    "x-ms-date",
                    "Content-Length",
                    "Content-Type",
                    "Referer"
                };

            foreach (var header in sourceRequest.Headers.AllKeys)
            {
                if (!excludeHeaders.Contains(header))
                {
                    destinationRequest.Headers.Add(header, sourceRequest.Headers[header]);
                }
            }
        }

        public static string ExtractBodyString(this WebResponse webResponse)
        {
            using (var stream = webResponse.GetResponseStream())
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public static void CopyResponseHeadersTo(this WebResponse sourceResponse, HttpResponse destinationResponse, string contentType)
        {
            destinationResponse.ContentType = contentType;

            var httpWebResponse = sourceResponse as HttpWebResponse;
            if (httpWebResponse != null)
            {
                destinationResponse.StatusCode = (int)httpWebResponse.StatusCode;
                destinationResponse.StatusDescription = httpWebResponse.StatusDescription;
            }

            string[] excludeHeaders = { "Content-Type", "Transfer-Encoding" };
            foreach (var header in sourceResponse.Headers.AllKeys)
            {
                if (!excludeHeaders.Contains(header))
                {
                    destinationResponse.Headers.Add(header, sourceResponse.Headers[header]);
                }
            }
        }

        public static void AddWcfServiceRoute(this RouteCollection routes, Type dataServiceType, string prefix)
        {
            routes.Add(new ServiceRoute(prefix, new AutomaticFormatServiceHostFactory(), dataServiceType));
        }

        public static void AddWcfServiceRoute<TService>(this RouteCollection routes, string prefix)
        {
            AddWcfServiceRoute(routes, typeof(TService), prefix);
        }
    }
}