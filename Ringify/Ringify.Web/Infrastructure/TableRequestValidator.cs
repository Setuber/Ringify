namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Globalization;
    using System.Web;

    public class TableRequestValidator : IStorageRequestValidator
    {
        private const string BaseUrl = "/AzureTablesProxy.axd";

        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public TableRequestValidator()
            : this(new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public TableRequestValidator(IUserPrivilegesRepository userPrivilegesRepository)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        public bool DoesRequestApply(Uri resourceUri)
        {
            return resourceUri.AbsolutePath.StartsWith(BaseUrl, StringComparison.OrdinalIgnoreCase);
        }

        public bool ValidateRequest(string userId, HttpRequest request)
        {
            if (!this.CanUseTables(userId))
            {
                return false;
            }

            var tableName = StorageRequestAnalyzer.GetRequestedTable(request);
            if (!this.CanUseTable(userId, tableName, request))
            {
                return false;
            }

            return true;
        }

        private bool CanUseTable(string userId, string tableName, HttpRequest request)
        {
            var publicTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.PublicTablePrivilegeSuffix);
            if (!this.userPrivilegesRepository.PublicPrivilegeExists(publicTablePrivilege))
            {
                var accessTablePrivilege = string.Format(CultureInfo.InvariantCulture, "{0}{1}", tableName, PrivilegeConstants.TablePrivilegeSuffix);
                if (!this.userPrivilegesRepository.HasUserPrivilege(userId, accessTablePrivilege))
                {
                    // Check if the user is listing the available tables or creating a new table.
                    return StorageRequestAnalyzer.IsListingTables(request) || StorageRequestAnalyzer.IsCreatingTable(request, tableName);
                }
            }

            return true;
        }

        private bool CanUseTables(string userId)
        {
            return this.userPrivilegesRepository.HasUserPrivilege(userId, PrivilegeConstants.TablesUsagePrivilege);
        }
    }
}