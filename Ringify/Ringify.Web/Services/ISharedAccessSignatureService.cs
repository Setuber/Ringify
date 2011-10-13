namespace Ringify.Web.Services
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Ringify.Web.Models;

    [ServiceContract]
    public interface ISharedAccessSignatureService
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/container")]
        Uri GetContainerSharedAccessSignature();

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/blob?blobPrefix={blobPrefix}&useFlatBlobListing={useFlatBlobListing}")]
        CloudBlobCollection GetBlobsSharedAccessSignatures(string blobPrefix, bool useFlatBlobListing);
    }
}
