namespace Ringify.Web.Infrastructure
{
    public static class PrivilegeConstants
    {
        public const string AdminPrivilege = "admin";

        public const string TablesUsagePrivilege = "TablesUsage";

        public const string BlobsUsagePrivilege = "BlobsUsage";

        public const string QueuesUsagePrivilege = "QueuesUsage";

        public const string QueuePrivilegeSuffix = "_queue_access";

        public const string PublicQueuePrivilegeSuffix = "_queue_publicaccess";

        public const string TablePrivilegeSuffix = "_table_access";

        public const string PublicTablePrivilegeSuffix = "_table_publicaccess";
    }
}