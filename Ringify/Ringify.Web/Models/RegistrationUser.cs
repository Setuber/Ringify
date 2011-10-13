namespace Ringify.Web.Models
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Samples.WindowsPhoneCloud.StorageClient.Credentials")]
    public class RegistrationUser
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string EMail { get; set; }
    }
}