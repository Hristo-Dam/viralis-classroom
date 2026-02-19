using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viralis.Common.ViewModels.Assignment;
using Viralis.Services.Interfaces.Assignments;

namespace Viralis.Web.Controllers
{
    public class AssignmentController : BaseController
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public IActionResult Create(Guid classroomId)
        {
            return View(new CreateAssignmentViewModel { ClassroomId = classroomId });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(CreateAssignmentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _assignmentService.CreateAsync(model, CurrentUserId);

            return RedirectToAction("Details", "Classroom",
                new { id = model.ClassroomId, tab = "assignments" });
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, Guid classroomId)
        {
            bool isTeacher = User.IsInRole("Teacher");
            var model = await _assignmentService.GetDetailAsync(id, CurrentUserId, isTeacher);

            if (model == null) return Forbid();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(SubmitAssignmentViewModel model)
        {
            try
            {
                await _assignmentService.SubmitAsync(model, CurrentUserId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details),
                new { id = model.AssignmentId, classroomId = model.ClassroomId });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Grade(GradeSubmissionViewModel model)
        {
            await _assignmentService.GradeAsync(model, CurrentUserId);

            return RedirectToAction(nameof(Details),
                new { id = model.SubmissionId, classroomId = model.ClassroomId });
        }
    }
}
