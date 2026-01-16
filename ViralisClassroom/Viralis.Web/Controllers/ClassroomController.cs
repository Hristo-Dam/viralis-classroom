using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Viralis.Data.Models;
using Viralis.Common.DTOs;
using Viralis.Services.Interfaces;

namespace Viralis.Web.Controllers
{
    public class ClassroomController : BaseController
    {
        private readonly IClassroomService classroomService;
        private readonly UserManager<ApplicationUser> userManager;

        public ClassroomController(
            IClassroomService classroomService,
            UserManager<ApplicationUser> userManager)
        {
            this.classroomService = classroomService;
            this.userManager = userManager;
        }

        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(CreateClassroomViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = Guid.Parse(userManager.GetUserId(User)!);

            await classroomService.CreateAsync(model, userId);

            return RedirectToAction(nameof(Index));
        }
    }
}
