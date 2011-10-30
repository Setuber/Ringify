namespace Ringify.Web.Handlers
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Xml;
    using Microsoft.IdentityModel.Claims;
    using Microsoft.WindowsAzure;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Serializers;

    public abstract class StorageProxyHandler : IHttpHandler
    {
        private readonly IFormatSerializerFactory formatSerializerFactory;
        private IWebRequestCreate webRequestCreator;

        protected StorageProxyHandler(CloudStorageAccount cloudStorageAccount, IFormatSerializerFactory formatSerializerFactory)
        {
            this.formatSerializerFactory = formatSerializerFactory;
            this.CloudStorageAccount = cloudStorageAccount;
        }

        public bool IsReusable
        {
            get { return true; }
        }

        internal IWebRequestCreate WebRequestCreator
        {
            get
            {
                if (this.webRequestCreator == null)
                {
                    this.webRequestCreator = new WebRequestCreator();
                }

                return this.webRequestCreator;
            }

            set
            {
                this.webRequestCreator = value;
            }
        }

        protected string UserId
        {
            get
            {
                var identity = HttpContext.Current.User.Identity as IClaimsIdentity;
                return identity.Claims.Single(c => c.ClaimType == ClaimTypes.NameIdentifier).Value;
            }
        }

        protected CloudStorageAccount CloudStorageAccount { get; private set; }

        protected abstract string AzureStorageUrl { get; }

        public void ProcessRequest(HttpContext context)
        {
            var azureResponse = this.GetAzureStorageResponse(this.GetAzureStorageRequestUri(context.Request), context.Request);

            var serializer = this.formatSerializerFactory.GetSerializer(context.Request.Headers, context.Request.QueryString);
            var azureStorageResponseBody = azureResponse.ExtractBodyString();
            var serializedContentType = string.Empty;
            if (!string.IsNullOrWhiteSpace(azureStorageResponseBody))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(azureStorageResponseBody);

                azureStorageResponseBody = serializer.SerializeReply(xmlDoc, out serializedContentType);
            }

            azureResponse.CopyResponseHeadersTo(context.Response, serializedContentType);

            context.Response.Write(this.GetProxyResponseBody(azureStorageResponseBody, context.Request));

            this.PostProcessProxyRequest(context);

            context.Response.End();
        }

        protected virtual string GetAzureStorageRequestBody(string proxyRequestBody, HttpRequest proxyRequest)
        {
            var oldValue = string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}",
                proxyRequest.Url.GetComponents(UriComponents.SchemeAndServer & ~UriComponents.Port, UriFormat.SafeUnescaped),
                proxyRequest.FilePath);
            var newValue = this.AzureStorageUrl;

            return proxyRequestBody.Replace(oldValue, newValue);
        }

        protected virtual string GetProxyResponseBody(string azureStorageResponseBody, HttpRequest proxyRequest)
        {
            var oldValue = this.AzureStorageUrl;
            var newValue = string.Format(
                                     CultureInfo.InvariantCulture,
                                     "{0}{1}",
                                     proxyRequest.Url.GetComponents(UriComponents.SchemeAndServer & ~UriComponents.Port, UriFormat.SafeUnescaped),
                                     proxyRequest.FilePath);

            return azureStorageResponseBody.Replace(oldValue, newValue);
        }

        protected abstract void SignRequest(WebRequest webRequest);

        protected abstract void PostProcessProxyRequest(HttpContext context);

        private Uri GetAzureStorageRequestUri(HttpRequest request)
        {
            var relativeUrl = (request.QueryString.Count > 0)
                                ? string.Format(CultureInfo.InvariantCulture, "{0}?{1}", request.PathInfo, request.QueryString.ToString())
                                : request.PathInfo;

            return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.AzureStorageUrl, relativeUrl), UriKind.Absolute);
        }

        private WebResponse GetAzureStorageResponse(Uri azureStorageRequestUri, HttpRequest proxyRequest)
        {
            var azureStorageRequest = this.WebRequestCreator.Create(azureStorageRequestUri);
            var azureStorageRequestBody = this.GetAzureStorageRequestBody(new StreamReader(proxyRequest.InputStream).ReadToEnd(), proxyRequest);

            azureStorageRequest.Method = proxyRequest.HttpMethod;
            if (azureStorageRequestBody.Length > 0 ||
                (!proxyRequest.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) && !proxyRequest.HttpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase)))
            {
                azureStorageRequest.ContentLength = azureStorageRequestBody.Length;
            }

            proxyRequest.CopyRequestHeadersTo(azureStorageRequest);

            this.SignRequest(azureStorageRequest);

            if (!proxyRequest.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) && !proxyRequest.HttpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase) && proxyRequest.ContentLength > 0)
            {
                var azureStorageRequestStream = azureStorageRequest.GetRequestStream();
                using (var writer = new StreamWriter(azureStorageRequestStream))
                {
                    azureStorageRequestStream = null;
                    writer.Write(azureStorageRequestBody);
                }
            }

            try
            {
                return azureStorageRequest.GetResponse();
            }
            catch (WebException webException)
            {
                return webException.Response;
            }
        }
    }
}