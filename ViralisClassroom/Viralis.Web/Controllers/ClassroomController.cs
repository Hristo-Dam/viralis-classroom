using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viralis.Common.ViewModels.Classroom;
using Viralis.Services.Interfaces;

namespace Viralis.Web.Controllers
{
    public class ClassroomController : BaseController
    {
        private readonly IClassroomService classroomService;

        public ClassroomController(IClassroomService classroomService)
        {
            this.classroomService = classroomService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            IEnumerable<ClassroomListViewModel> classrooms = await classroomService.GetUserClassroomsAsync(CurrentUserId);

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

            var result = new CreateClassroomViewModel
            {
                Name = model.Name,
                Subject = model.Subject
            };

            try
            {
                await classroomService.CreateAsync(result, CurrentUserId);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
