namespace Ringify.Web.Models
{
    using System;
    using System.Globalization;
    using Microsoft.WindowsAzure.StorageClient;

    [CLSCompliant(false)]
    public class PushUserEndpoint : TableServiceEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The PartitionKey and RowKey properties are set to uniquely identify the PushUserEndpoint entity.")]
        public PushUserEndpoint()
        {
            this.PartitionKey = "a";
            this.RowKey = string.Format(CultureInfo.InvariantCulture, "{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
        }

        public string UserId { get; set; }

        public string ChannelUri { get; set; }

        public int TileCount { get; set; }
    }
}