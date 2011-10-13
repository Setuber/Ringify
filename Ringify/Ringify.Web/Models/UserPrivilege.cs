namespace Ringify.Web.Models
{
    using System;
    using System.Globalization;
    using Microsoft.WindowsAzure.StorageClient;

    [CLSCompliant(false)]
    public class UserPrivilege : TableServiceEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The PartitionKey and RowKey properties are set to uniquely identify the UserPrivilege entity.")]
        public UserPrivilege()
        {
            this.PartitionKey = "a";
            this.RowKey = string.Format(CultureInfo.InvariantCulture, "{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
        }

        public string UserId { get; set; }

        public string Privilege { get; set; }
    }
}