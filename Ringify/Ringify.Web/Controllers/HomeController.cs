namespace Ringify.Web.Controllers
{
    using System.Web.Mvc;

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            this.ViewData["Message"] = "Home";

            return this.View();
        }
    }
}
