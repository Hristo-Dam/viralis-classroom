using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viralis.Common.ViewModels.Classroom;
using Viralis.Services.Implementations;
using Viralis.Services.Interfaces;

namespace Viralis.Web.Controllers
{
    public class ClassroomController : BaseController
    {
        private readonly IClassroomService _classroomService;

        public ClassroomController(IClassroomService classroomService)
        {
            this._classroomService = classroomService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            IEnumerable<ClassroomListViewModel> classrooms = await _classroomService.GetUserClassroomsAsync(CurrentUserId);

            return View(classrooms);
        }


        [HttpGet]
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

            var result = new CreateClassroomViewModel
            {
                Name = model.Name,
                Subject = model.Subject
            };

            try
            {
                await _classroomService.CreateAsync(result, CurrentUserId);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Join(JoinClassroomViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _classroomService.JoinByCodeAsync(model.JoinCode, CurrentUserId);
            }
            catch
            {
                ModelState.AddModelError("", "Invalid or already used join code");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
