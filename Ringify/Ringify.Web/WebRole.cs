namespace Ringify.Web
{
    using System.Linq;
    using System.Security.Permissions;
    using System.Xml.Linq;
    using Microsoft.Web.Administration;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Models;

    public class WebRole : RoleEntryPoint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method initializes the Web role.")]
        public override bool OnStart()
        {
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString");

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += this.RoleEnvironmentChanging;

            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                // Provide the configSetter with the initial value
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));

                RoleEnvironment.Changed += (sender, arg) =>
                {
                    if (arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any((change) => (change.ConfigurationSettingName == configName)))
                    {
                        // The corresponding configuration setting has changed, propagate the value
                        if (!configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)))
                        {
                            // In this case, the change to the storage account credentials in the
                            // service configuration is significant enough that the role needs to be
                            // recycled in order to use the latest settings. (for example, the 
                            // endpoint has changed)
                            RoleEnvironment.RequestRecycle();
                        }
                    }
                };
            });

            // If no valid WIF settings are found in the Web Role configuration, then the Web Role shouldn't start
            if (!UpdateWifSettings())
            {
                return false;
            }

            var account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            CreateSilverlightClientAccessPolicy(account.CreateCloudBlobClient());
            CreateCloudTables(account.CreateCloudTableClient());

            return base.OnStart();
        }

        private static void CreateSilverlightClientAccessPolicy(CloudBlobClient cloudBlobClient)
        {
            var container = cloudBlobClient.GetContainerReference("$root");
            container.CreateIfNotExist();
            container.SetPermissions(
                new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });

            var blob = cloudBlobClient.GetBlobReference("clientaccesspolicy.xml");
            blob.Properties.ContentType = "text/xml";
            blob.UploadText(
                @"<?xml version=""1.0"" encoding=""utf-8""?>
                <access-policy>
                  <cross-domain-access>
                    <policy>
                      <allow-from http-methods=""*"" http-request-headers=""*"">
                        <domain uri=""*"" />
                        <domain uri=""http://*"" />
                      </allow-from>
                      <grant-to>
                        <resource path=""/"" include-subpaths=""true"" />
                      </grant-to>
                    </policy>
                  </cross-domain-access>
                </access-policy>");
        }

        private static void CreateCloudTables(CloudTableClient cloudTableClient)
        {
            CreatePushNotificationTable(cloudTableClient);
            CreateUserTable(cloudTableClient);
            CreateUserPrivilegeTable(cloudTableClient);
        }

        private static void CreatePushNotificationTable(CloudTableClient cloudTableClient)
        {
            cloudTableClient.CreateTableIfNotExist(UserTablesServiceContext.PushUserTableName);

            // Execute conditionally for development storage only.
            if (cloudTableClient.BaseUri.IsLoopback)
            {
                var context = cloudTableClient.GetDataServiceContext();
                var entity = new PushUserEndpoint { UserId = "UserName", ChannelUri = "http://tempuri", TileCount = 0 };

                context.AddObject(UserTablesServiceContext.PushUserTableName, entity);
                context.SaveChangesWithRetries();
                context.DeleteObject(entity);
                context.SaveChangesWithRetries();
            }
        }

        [EnvironmentPermission(SecurityAction.LinkDemand)]
        private static bool UpdateWifSettings()
        {
            using (var server = new ServerManager())
            {
                var siteNameFromServiceModel = "Web";
                var siteName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1}", RoleEnvironment.CurrentRoleInstance.Id, siteNameFromServiceModel);

                var configFilePath = string.Format(System.Globalization.CultureInfo.InvariantCulture, @"{0}\Web.config", server.Sites[siteName].Applications[0].VirtualDirectories[0].PhysicalPath);
                var xml = XElement.Load(configFilePath);
                var identityModelService = xml.Element("microsoft.identityModel").Element("service");

                if (UpdateAttributeWithRoleSetting(identityModelService.Element("audienceUris").Element("add").Attribute("value"), "realm") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerTokenResolver").Element("serviceKeys").Element("add").Attribute("serviceName"), "realm") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerTokenResolver").Element("serviceKeys").Element("add").Attribute("serviceKey"), "serviceKey") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerNameRegistry").Element("trustedIssuers").Element("add").Attribute("issuerIdentifier"), "trustedIssuersIdentifier") &&
                    UpdateAttributeWithRoleSetting(identityModelService.Element("issuerNameRegistry").Element("trustedIssuers").Element("add").Attribute("name"), "trustedIssuerName"))
                {
                    xml.Save(configFilePath);
                    return true;
                }

                return false;
            }
        }

        private static bool UpdateAttributeWithRoleSetting(XAttribute attribute, string settingName)
        {
            var settingValue = ConfigReader.GetConfigValue(settingName, false);
            if (!string.IsNullOrEmpty(settingValue))
            {
                attribute.Value = settingValue;
            }
            else if (string.IsNullOrEmpty(attribute.Value))
            {
                return false;
            }

            return true;
        }

        private static void CreateUserTable(CloudTableClient cloudTableClient)
        {
            cloudTableClient.CreateTableIfNotExist(UserTablesServiceContext.UserTableName);

            // Execute conditionally for development storage only.
            if (cloudTableClient.BaseUri.IsLoopback)
            {
                var context = cloudTableClient.GetDataServiceContext();
                var entity = new User { Name = "UserName", Email = "user@email.com" };

                context.AddObject(UserTablesServiceContext.UserTableName, entity);
                context.SaveChangesWithRetries();
                context.DeleteObject(entity);
                context.SaveChangesWithRetries();
            }
        }

        private static void CreateUserPrivilegeTable(CloudTableClient cloudTableClient)
        {
            cloudTableClient.CreateTableIfNotExist(UserTablesServiceContext.UserPrivilegeTableName);

            // Execute conditionally for development storage only.
            if (cloudTableClient.BaseUri.IsLoopback)
            {
                var context = cloudTableClient.GetDataServiceContext();
                var entity = new UserPrivilege { UserId = "UserId", Privilege = "Privilege" };

                context.AddObject(UserTablesServiceContext.UserPrivilegeTableName, entity);
                context.SaveChangesWithRetries();
                context.DeleteObject(entity);
                context.SaveChangesWithRetries();
            }
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}
