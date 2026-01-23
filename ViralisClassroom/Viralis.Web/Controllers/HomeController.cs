using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Viralis.Web.Controllers
{
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            string test = string.Empty;
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Classroom");
            }

            return View();
        }
    }
}
