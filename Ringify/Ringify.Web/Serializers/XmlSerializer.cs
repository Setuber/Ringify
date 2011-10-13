namespace Ringify.Web.Serializers
{
    using System.Xml;
    using Ringify.Web.Infrastructure;

    public class XmlSerializer : IFormatSerializer
    {
        public string SerializeReply(object originalReply, out string contentType)
        {
            contentType = HttpConstants.MimeApplicationAtomXml;

            return (originalReply as XmlDocument).InnerXml;
        }
    }
}