namespace Ringify.Web.Services
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Web;

    [ServiceContract]
    public interface ISamplePushUserRegistrationService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "/register",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string Register(Uri channelUri);

        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "/unregister",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string Unregister(Uri channelUri);

        [OperationContract]
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/updates?channelUri={channelUri}")]
        string[] GetUpdates(Uri channelUri);
    }
}
