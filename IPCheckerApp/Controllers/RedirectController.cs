using Microsoft.AspNetCore.Mvc;

namespace IPCheckerApp.Controllers
{
    public class RedirectController : Controller
    {
        public IActionResult ToLogin()
        {
            return Redirect("/Identity/Account/Login");
        }
    }
}
