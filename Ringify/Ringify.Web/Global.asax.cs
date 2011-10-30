namespace Ringify.Web
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Ringify.Web.Controllers;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Services;

    public class MvcApplication : System.Web.HttpApplication
    {
        private const int DefaultHttpsPort = 443;
        private const int DefaultHttpPort = 10080;
        private const string PortErrorMessage = @"The Web role was started in a wrong port.
                                            For this sample application to work correctly, please make sure that it is running in port {0}. 
                                            Please review the Troubleshooting section of the sample documentation for instructions on how to do this.";

        private static bool securityInitialized = false;

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = (string)null },
                new { controller = new ListConstraint(ListConstraintType.Exclude, "RegistrationService", "SharedAccessSignatureService", "PushNotificationService") });
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                // Provide the configSetter with the initial value.
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });

            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            RouteTable.Routes.AddWcfServiceRoute<RegistrationService>("RegistrationService");
            RouteTable.Routes.AddWcfServiceRoute<SharedAccessSignatureService>("SharedAccessSignatureService");
            RouteTable.Routes.AddWcfServiceRoute<SamplePushUserRegistrationService>("PushNotificationService");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (!securityInitialized)
            {
                InitializeSecurity();
                securityInitialized = true;
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (this.ShouldRedirectToHttps())
            {
                this.RedirectScheme(this.Context.Request.Url, "https");
            }
            else if (this.ShouldRedirectToHttp())
            {
                this.RedirectScheme(this.Context.Request.Url, "http");
            }

            if (!this.IsPortNumberOK() && !IsAllowedContent(this.Context.Request.Path))
            {
                this.CreateWrongPortException();
            }
        }

        private static void InitializeSecurity()
        {
            var adminUser = Membership.FindUsersByName("admin").Cast<MembershipUser>().FirstOrDefault();
            if (adminUser == null)
            {
                adminUser = Membership.CreateUser("admin", "Passw0rd!", "admin@wp7cloudapp.com");
            }

            var adminUserId = adminUser.ProviderUserKey.ToString();
            IUserPrivilegesRepository userPrivilegesRepository = new UserTablesServiceContext();
            userPrivilegesRepository.AddPrivilegeToUser(adminUserId, PrivilegeConstants.AdminPrivilege);
        }

        private static bool IsAllowedContent(string path)
        {
            return path.EndsWith("/Error", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Content", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Scripts", StringComparison.OrdinalIgnoreCase);
        }

        private void RedirectScheme(Uri originalUri, string intendedScheme)
        {
            int portNumber = 0;
            if (intendedScheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                portNumber = DefaultHttpsPort;
            }
            else if (intendedScheme.Equals("http", StringComparison.OrdinalIgnoreCase))
            {
                portNumber = DefaultHttpPort;
            }

            var redirectUrl = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://{1}:{2}{3}",
                    intendedScheme,
                    originalUri.Host,
                    portNumber,
                    originalUri.PathAndQuery);

            this.Response.Redirect(redirectUrl, true);
        }

        private bool ShouldRedirectToHttp()
        {
            return this.Request.IsSecureConnection && this.Context.Request.Url.ToString().EndsWith(".cer", StringComparison.OrdinalIgnoreCase);
        }

        private bool ShouldRedirectToHttps()
        {
            return !this.Request.IsSecureConnection && !this.Context.Request.Url.ToString().EndsWith(".cer", StringComparison.OrdinalIgnoreCase);
        }

        private void CreateWrongPortException()
        {
            var exception = new RoleInWrongPortException(string.Format(CultureInfo.InvariantCulture, PortErrorMessage, DefaultHttpsPort));
            var routeData = new RouteData();
            routeData.Values.Add("Controller", "Error");
            routeData.Values.Add("Action", "Index");
            routeData.Values.Add("Error", exception);

            using (var errorController = new ErrorController())
            {
                ((IController)errorController).Execute(new RequestContext(new HttpContextWrapper(this.Context), routeData));
            }

            this.Context.Response.End();
        }

        private bool IsPortNumberOK()
        {
            var scheme = this.Context.Request.Url.Scheme;
            var portNumber = 0;

            if (scheme.Equals("https"))
            {
                portNumber = DefaultHttpsPort;
            }
            else if (scheme.Equals("http"))
            {
                portNumber = DefaultHttpPort;
            }

            var hostAddress = this.Context.Request.Headers["Host"] ?? string.Empty;
            var portPosition = hostAddress.IndexOf(":", StringComparison.OrdinalIgnoreCase);

            if (portPosition > 0)
            {
                int.TryParse(hostAddress.Substring(portPosition + 1), out portNumber);
            }

            return (portNumber == DefaultHttpsPort) || ((portNumber == DefaultHttpPort) && Context.Request.Url.ToString().EndsWith(".cer", StringComparison.OrdinalIgnoreCase));
        }
    }
}