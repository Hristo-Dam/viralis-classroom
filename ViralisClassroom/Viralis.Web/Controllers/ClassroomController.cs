using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Viralis.Data.Models;
using Viralis.Services.Interfaces;
using Viralis.Common.DTOs.Classroom;

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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(userManager.GetUserId(User)!);

            IEnumerable<ClassroomListViewModel> classrooms = await classroomService.GetUserClassroomsAsync(userId);

            return View(classrooms);
        }


        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(CreateClassroomViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Guid userId = Guid.Parse(userManager.GetUserId(User)!);

            var dto = new CreateClassroomViewModel
            {
                Name = model.Name,
                Subject = model.Subject
            };

            await classroomService.CreateAsync(dto, userId);

            return RedirectToAction(nameof(Index));
        }

    }
}
