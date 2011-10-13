namespace Ringify.Web.Services
{
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Ringify.Web.Models;

    [ServiceContract]
    public interface IRegistrationService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = "/register",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string CreateUser(RegistrationUser user);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "/validate",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        string CheckUserRegistration();
    }
}
