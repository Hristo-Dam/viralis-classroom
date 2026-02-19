using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Viralis.Common.Constants;
using Viralis.Common.ViewModels.SchoolAdmin;
using Viralis.Data.Models;

namespace Viralis.Web.Controllers
{
    [Authorize(Roles = "School Administrator")]
    public class SchoolAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SchoolAdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get all teachers
            var teachers = await _userManager.GetUsersInRoleAsync(RoleConstants.TEACHER);
            return View(teachers);
        }

        [HttpGet]
        public IActionResult CreateTeacher() => View();

        [HttpPost]
        public async Task<IActionResult> CreateTeacher(CreateTeacherViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Check if email already taken
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("", "A user with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser();
            await _userManager.SetUserNameAsync(user, model.Email);
            await _userManager.SetEmailAsync(user, model.Email);

            var result = await _userManager.CreateAsync(user, model.TemporaryPassword);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleConstants.TEACHER);
                TempData["Success"] = $"Teacher account created for {model.Email}";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
