namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Microsoft.IdentityModel.Claims;

    public class UserPrivilegesAuthorizationManager : ClaimsAuthorizationManager
    {
        private readonly IEnumerable<IStorageRequestValidator> storageRequestValidators;

        public UserPrivilegesAuthorizationManager()
            : this(new IStorageRequestValidator[] { new QueueRequestValidator(), new TableRequestValidator(), new BlobRequestValidator() })
        {
        }

        public UserPrivilegesAuthorizationManager(IEnumerable<IStorageRequestValidator> storageRequestValidators)
        {
            this.storageRequestValidators = storageRequestValidators;
        }

        public override bool CheckAccess(AuthorizationContext context)
        {
            var identity = context.Principal.Identity as IClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return false;
            }

            var nameIdentifier = identity.Claims.FirstOrDefault(c => c.ClaimType == ClaimTypes.NameIdentifier);
            if (nameIdentifier == null)
            {
                return false;
            }

            var resourceUri = new Uri(context.Resource.First().Value, UriKind.RelativeOrAbsolute);
            foreach (var validator in this.storageRequestValidators)
            {
                if (validator.DoesRequestApply(resourceUri))
                {
                    return validator.ValidateRequest(nameIdentifier.Value, HttpContext.Current.Request);
                }
            }

            return base.CheckAccess(context);
        }
    }
}