namespace Ringify.Web.Controllers
{
    using System;
    using System.Globalization;
    using System.Web.Mvc;
    using System.Web.Security;
    using Ringify.Web.Infrastructure;
    using Ringify.Web.Models;
    using Ringify.Web.UserAccountWrappers;

    [HandleError]
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication formsAuth;
        private readonly IMembershipService membershipService;
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public AccountController()
            : this(new FormsAuthenticationService(), new AccountMembershipService(), new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public AccountController(IFormsAuthentication formsAuth, IMembershipService membershipService, IUserPrivilegesRepository userPrivilegesRepository)
        {
            this.formsAuth = formsAuth;
            this.membershipService = membershipService;
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        public ActionResult Unauthorized()
        {
            return this.View();
        }

        public ActionResult LogOn()
        {
            return this.View(new LogOnModel());
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Needs to take same parameter type as Controller.Redirect()")]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (!this.ValidateLogOn(model))
            {
                return this.View(model);
            }

            this.formsAuth.SignIn(model.UserName, model.RememberMe);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        public ActionResult LogOff()
        {
            this.formsAuth.SignOut();

            return this.RedirectToAction("Index", "Home");
        }

        private bool ValidateLogOn(LogOnModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                this.ModelState.AddModelError("username", "You must specify a username.");
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                this.ModelState.AddModelError("password", "You must specify a password.");
            }

            if (!this.membershipService.ValidateUser(model.UserName, model.Password))
            {
                this.ModelState.AddModelError("", "The username or password provided is incorrect.");
            }

            return ModelState.IsValid;
        }
    }
}