using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viralis.Common.Constants;

namespace Viralis.Web.Controllers
{
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(RoleConstants.ADMIN))
                    return RedirectToAction("Users", "Admin");

                if (User.IsInRole(RoleConstants.SCHOOL_ADMINISTRATOR))
                    return RedirectToAction("Index", "SchoolAdmin");

                return RedirectToAction("Index", "Classroom");
            }

            return View();
        }
    }
}
