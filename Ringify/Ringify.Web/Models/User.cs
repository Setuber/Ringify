namespace Ringify.Web.Models
{
    using System;
    using System.Globalization;
    using Microsoft.WindowsAzure.StorageClient;

    [CLSCompliant(false)]
    public class User : TableServiceEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The PartitionKey and RowKey properties are set to uniquely identify the User entity.")]
        public User()
        {
            this.PartitionKey = "a";
            this.RowKey = string.Format(CultureInfo.InvariantCulture, "{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
        }

        public string UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}