namespace Ringify.Web.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Microsoft.Samples.WindowsPhoneCloud.StorageClient", Name = "Blob")]
    public class CloudBlob
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember(Name = "Url")]
        public Uri Uri { get; set; }
    }
}