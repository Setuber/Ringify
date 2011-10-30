using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace Ringify
{
    public delegate void ContainerListUpdatedEventHandler(object sender, EventArgs e);
    public delegate void BlobListUpdatedEventHandler(object sender, EventArgs e);

    public class BlobStorage
    {
        public string Name;
        public string Url;
        public string LastModified;
        public string Etag;
        public string ContentType;
        public string ContentEncoding;
        public string ContentLanguage;
        public int Size;

        public BlobStorage()
        {
            Name = "";
            Url = "";
            LastModified = "";
            Etag = "";
            ContentType = "";
            ContentEncoding = "";
            ContentLanguage = "";
            Size = 0;
        }
    }

    public class BlobStorageContainer
    {
        public string Name;
        public string Url;
        public string LastModified;
        public string Etag;

        public BlobStorageContainer()
        {
            Name = "";
            Url = "";
            LastModified = "";
            Etag = "";
        }
    }

    public class BlobStorageClient
    {
        private const string CloadBlobHost = "blob.core.windows.net";

        private const string HeaderPrefixMS = "x-ms-";
        private const string HeaderPrefixProperties = "x-ms-prop-";
        private const string HeaderPrefixMetadata = "x-ms-meta-";
        private const string HeaderData = HeaderPrefixMS + "date";

        private const string CarriageReturnLinefeed = "\r\n";

        private string AccountName { get; set; }
        private byte[] SecretKey { get; set; }
        private string Host { get; set; }

        public BlobStorageClient(string i_AccountName, string i_Base64SecretKey)
        {
            this.AccountName = i_AccountName;
            this.SecretKey = Convert.FromBase64String(i_Base64SecretKey);
            this.Host = CloadBlobHost; // Pick default blob storage host
        }

        public BlobStorageClient(string i_AccountName, string i_Base64SecretKey, string i_Host)
        {
            this.AccountName = i_AccountName;
            this.SecretKey = Convert.FromBase64String(i_Base64SecretKey);
            this.Host = i_Host;
        }

        public void CreateBlob(string i_ContainerName, string i_BlobName)
        {
            Dictionary<string, string> Headers = new Dictionary<string, string>();
            Headers.Add("x-ms-blob-type", "PageBlob");
            Headers.Add("x-ms-version", "2009-09-19");

            // To construct a container, make a PUT request to 
            // http://<ACCOUNT>.blob.windows.net/<CONTAINER>
            HttpWebResponse Response = DoStorageRequest(
                    i_ContainerName + "/" + i_BlobName, "PUT", Headers, null /* no data */, null /* no content type */);

        }

        public event ContainerListUpdatedEventHandler ContainerListUpdated;
        public void ListContainers()
        {
            Dictionary<string, string> Headers = new Dictionary<string, string>();

            // To construct a container, make a PUT request to 
            // http://<ACCOUNT>.blob.windows.net/<CONTAINER>
            HttpWebResponse Response = DoStorageRequest(
                    "?comp=list", "GET", Headers, null /* no data */, null /* no content type */);

        }

        public List<BlobStorageContainer> ContainerList = new List<BlobStorageContainer>();

        private void UpdateContainerList(HttpWebResponse i_Response)
        {
            ContainerList.Clear();

            if (i_Response != null)
            {
                switch (i_Response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        // Returned HTTP 201. Containers OK
                        // Send the event
                        Stream ResponseStream = i_Response.GetResponseStream();
                        XmlReader Reader = XmlReader.Create(ResponseStream);
                        if (Reader.Read() && Reader.IsStartElement())
                        {
                            if (Reader.Name != "EnumerationResults")
                                return;

                            if (!Reader.Read() || Reader.Name != "Containers")
                                return;

                            while (Reader.Read() && Reader.Name == "Container")
                            {
                                BlobStorageContainer Container = new BlobStorageContainer();
                                Reader.Read();
                                while (Reader.Name != "Container")
                                {
                                    switch (Reader.Name)
                                    {
                                        case "Name":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Container.Name = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "Url":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Container.Url = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "LastModified":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Container.LastModified = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "Etag":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Container.Etag = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        default:
                                            {
                                                Reader.Read();
                                            }
                                            break;
                                    }
                                }

                                ContainerList.Add(Container);
                            }
                        }
                        ContainerListUpdated(this, EventArgs.Empty);
                        break;
                    default:
                        // Unexpected Status code
                        throw new Exception(i_Response.StatusDescription);
                }
            }
        }

        public event BlobListUpdatedEventHandler BlobListUpdated;
        public void ListBlobs(string i_Container)
        {
            Dictionary<string, string> Headers = new Dictionary<string, string>();

            // To construct a container, make a PUT request to 
            // http://<ACCOUNT>.blob.windows.net/<CONTAINER>
            HttpWebResponse Response = DoStorageRequest(
                    i_Container + "?comp=list", "GET", Headers, null /* no data */, null /* no content type */);

        }

        public List<BlobStorage> BlobList = new List<BlobStorage>();

        private void UpdateBlobList(HttpWebResponse i_Response)
        {
            BlobList.Clear();

            if (i_Response != null)
            {
                switch (i_Response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        // Returned HTTP 201. Containers OK
                        // Send the event
                        Stream ResponseStream = i_Response.GetResponseStream();
                        XmlReader Reader = XmlReader.Create(ResponseStream);
                        if (Reader.Read() && Reader.IsStartElement())
                        {
                            if (Reader.Name != "EnumerationResults")
                                return;

                            if (!Reader.Read() || Reader.Name != "Blobs")
                                return;

                            while (Reader.Read() && Reader.Name == "Blob")
                            {
                                BlobStorage Blob = new BlobStorage();
                                Reader.Read();
                                while (Reader.Name != "Blob")
                                {
                                    switch (Reader.Name)
                                    {
                                        case "Name":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.Name = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "Url":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.Url = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "LastModified":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.LastModified = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "Etag":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.Etag = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "Size":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.Size = Reader.ReadElementContentAsInt();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "ContentType":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.ContentType = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "ContentEncoding":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.ContentEncoding = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        case "ContentLanguage":
                                            {
                                                if (!Reader.IsEmptyElement)
                                                    Blob.ContentLanguage = Reader.ReadElementContentAsString();
                                                else
                                                    Reader.Read();
                                            }
                                            break;
                                        default:
                                            {
                                                Reader.Read();
                                            }
                                            break;
                                    }
                                }

                                BlobList.Add(Blob);
                            }
                        }
                        BlobListUpdated(this, EventArgs.Empty);
                        break;
                    default:
                        // Unexpected Status code
                        throw new Exception(i_Response.StatusDescription);
                }
            }
        }

        public bool CreateContainer(string i_ContainerName, bool i_PublicAccess)
        {
            Dictionary<string, string> Headers = new Dictionary<string, string>();

            if (i_PublicAccess)
            {
                // Public access for container. Set x-ms-prop-publicaccess to true
                Headers[HeaderPrefixProperties + "publicaccess"] = "true";
            }

            // To construct a container, make a PUT request to 
            // http://<ACCOUNT>.blob.windows.net/<CONTAINER>
            HttpWebResponse Response = DoStorageRequest(
                    i_ContainerName, "PUT", Headers, null /* no data */, null /* no content type */);

            bool RetVal = false;
            if (Response != null)
            {
                switch (Response.StatusCode)
                {
                    case HttpStatusCode.Created:
                        // Returned HTTP 201. Container was created
                        RetVal = true;
                        break;
                    default:
                        // Unexpected Status code
                        throw new Exception(Response.StatusDescription);
                }
            }

            return RetVal;
        }

        public static ManualResetEvent AllDone = new ManualResetEvent(false);

        private HttpWebResponse DoStorageRequest(string i_ResourcePath, string i_HttpMethod,
            Dictionary<string, string> i_MetadataHeaders, byte[] i_Data, string i_ContentType)
        {
            // Create request object for http://<ACCOUNT>.blob.core.windows.net/<RESOURCE_PATH>
            string URL = "http://" + this.AccountName + "." + this.Host + "/" + i_ResourcePath;

            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(URL);
            Request.Method = i_HttpMethod;

            //Request.ContentLength = (i_Data == null) ? 0 : i_Data.Length;
            Request.ContentType = i_ContentType;

            // Add x-ms-data header. This should be in RFC 1123 format,i.e. 
            // of the form Sun, 28 Jan 2008 12:11:37 GMT
            // This is done by calling DataTime.ToString("R")
            Request.Headers[HeaderData] = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);

            // Add custom headers to request's headers
            if (i_MetadataHeaders != null)
            {
                foreach (string Key in i_MetadataHeaders.Keys)
                {
                    Request.Headers[Key] = i_MetadataHeaders[Key];
                }
            }

            // Get authorization header value by signing request
            string AuthHeader = SignRequest(Request);

            // Add authorization header
            Request.Headers["Authorization"] = "SharedKey " + this.AccountName + ":" + AuthHeader;

            RequestState State = new RequestState();
            State.Request = Request;
            State.RequestData = i_Data;
            State.Client = this;
            // Write data if any
            if (i_Data != null)
            {
                IAsyncResult StreamResult = Request.BeginGetRequestStream(new AsyncCallback(RespStreamCallback), State);

                //  AllDone.WaitOne();
            }

            IAsyncResult t = Request.BeginGetResponse(new AsyncCallback(RespCallback), State);

            return (HttpWebResponse)State.Response;
        }

        private static void RespCallback(IAsyncResult i_AsyncResult)
        {
            try
            {
                RequestState State = (RequestState)i_AsyncResult.AsyncState;
                HttpWebRequest Request = (HttpWebRequest)State.Request;
                State.Response = Request.EndGetResponse(i_AsyncResult);

                ((BlobStorageClient)State.Client).UpdateBlobList((HttpWebResponse)State.Response);

                //return (HttpWebResponse)State.Response;
                //AllDone.Set();
            }
            catch (Exception ex)
            {
                // AllDone.Set();
                Debugger.Trace(ex);
                //return null;
            }
        }

        private static void RespStreamCallback(IAsyncResult i_AsyncResult)
        {
            try
            {
                RequestState State = (RequestState)i_AsyncResult.AsyncState;
                HttpWebRequest Request = (HttpWebRequest)State.Request;
                State.RequestStream = Request.EndGetRequestStream(i_AsyncResult);
                State.RequestStream.Write(State.RequestData, 0, State.RequestData.Length);

                // AllDone.Set();
            }
            catch (Exception ex)
            {
                Debugger.Trace(ex);
            }
        }


        private string SignRequest(HttpWebRequest i_Request)
        {
            StringBuilder StringToSign = new StringBuilder();

            // First element is the HTTP method - GET/PUT/POST/...
            StringToSign.Append(i_Request.Method + "\n");

            // Second element is the MD5 hash of the data (optional so use empty line)
            StringToSign.Append("\n");

            // Append the content type of the request
            StringToSign.Append(i_Request.ContentType + "\n");

            // Append the data (x-ms-data header handles this)
            StringToSign.Append("\n");

            // Construct canonicalized header

            // Note that this doesn't implement
            // parts of the spec. like combining header fields with the same name,
            // unfolding long lines or trimming white space

            // look for header names that start with x-ms
            // Then sort them in case-insensitive manner
            List<string> HttpStorageHeaderNameArray = new List<string>();
            foreach (string Key in i_Request.Headers.AllKeys)
            {
                if (Key.ToLowerInvariant().StartsWith(HeaderPrefixMS, StringComparison.Ordinal))
                {
                    HttpStorageHeaderNameArray.Add(Key.ToLowerInvariant());
                }
            }

            HttpStorageHeaderNameArray.Sort();

            // Now go through each header's values in sorted order and 
            // append them to the canonicalized string.
            // At the end of this, you should have a bunch of headers of the form
            // x-ms-someotherhey:othervalue
            foreach (string Key in HttpStorageHeaderNameArray)
            {
                StringToSign.Append(Key + ":" + i_Request.Headers[Key] + "\n");
            }

            // Finally, add canonicalized resouces 
            // This is done by prepending a '/' to the account name and resource path
            StringToSign.Append("/" + this.AccountName + i_Request.RequestUri.AbsolutePath + i_Request.RequestUri.Query);

            // We have now a constructed string to sign.
            // We now need to generate a HMAC SHA256 hash using our secret hey and base64 encode it
            byte[] DataToMAC = System.Text.Encoding.UTF8.GetBytes(StringToSign.ToString());
            using (HMACSHA256 hmacsha256 = new HMACSHA256(this.SecretKey))
            {
                return Convert.ToBase64String((hmacsha256.ComputeHash(DataToMAC)));
            }
        }
    }
}
