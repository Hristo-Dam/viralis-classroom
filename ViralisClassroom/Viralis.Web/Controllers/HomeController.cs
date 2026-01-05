using Microsoft.AspNetCore.Mvc;

namespace Viralis.Web.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
