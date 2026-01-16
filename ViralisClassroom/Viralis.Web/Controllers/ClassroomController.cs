using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viralis.Common.DTOs;

namespace Viralis.Web.Controllers
{
    public class ClassroomController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public IActionResult Create(CreateClassroomViewModel model)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
