namespace Ringify.Web.Services
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Web;
    using Microsoft.IdentityModel.Claims;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Models;

    [ServiceBehavior(IncludeExceptionDetailInFaults = false)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class RegistrationService : IRegistrationService
    {
        private readonly IUserRepository userRepository;
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public RegistrationService()
            : this(new UserTablesServiceContext(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public RegistrationService(IUserRepository userRepository, IUserPrivilegesRepository userPrivilegesRepository)
        {
            this.userRepository = userRepository;
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        public string CreateUser(RegistrationUser user)
        {
            if ((user == null) || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.EMail))
            {
                throw new WebFaultException<string>("Invalid user information.", HttpStatusCode.BadRequest);
            }

            var identity = HttpContext.Current.User.Identity as IClaimsIdentity;
            var nameIdentifier = identity.Claims.Single(c => c.ClaimType == ClaimTypes.NameIdentifier).Value;
            this.userRepository.CreateUser(nameIdentifier, user.Name, user.EMail);
            this.SetUserDefaultUserPrivileges(nameIdentifier);

            return "Success";
        }

        public string CheckUserRegistration()
        {
            var identity = HttpContext.Current.User.Identity as IClaimsIdentity;

            var nameIdentifier = identity.Claims.Single(c => c.ClaimType == ClaimTypes.NameIdentifier).Value;

            return (this.userRepository.GetUser(nameIdentifier) != null).ToString();
        }

        private void SetUserDefaultUserPrivileges(string userId)
        {
            this.userPrivilegesRepository.AddPrivilegeToUser(userId, PrivilegeConstants.TablesUsagePrivilege);
            this.userPrivilegesRepository.AddPrivilegeToUser(userId, PrivilegeConstants.BlobsUsagePrivilege);
            this.userPrivilegesRepository.AddPrivilegeToUser(userId, PrivilegeConstants.QueuesUsagePrivilege);
            this.userPrivilegesRepository.AddPrivilegeToUser(userId, string.Format(CultureInfo.InvariantCulture, "{0}{1}", "SampleData", PrivilegeConstants.TablePrivilegeSuffix));
        }
    }
}