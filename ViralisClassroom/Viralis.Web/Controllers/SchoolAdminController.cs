using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Viralis.Common.Constants;
using Viralis.Common.ViewModels.SchoolAdmin;
using Viralis.Data;
using Viralis.Data.Models;

namespace Viralis.Web.Controllers
{
    [Authorize(Roles = "School Administrator")]
    public class SchoolAdminController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public SchoolAdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var school = await _db.SchoolAdministrators
                .Include(s => s.Teachers)
                .FirstOrDefaultAsync(s => s.SchoolAdminId == CurrentUserId);

            if (school == null)
                return View(new List<ApplicationUser>());

            return View(school.Teachers.ToList());
        }

        [HttpGet]
        public IActionResult CreateTeacher() => View();

        [HttpPost]
        public async Task<IActionResult> CreateTeacher(CreateTeacherViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("", "A user with this email already exists.");
                return View(model);
            }

            // Check if email already taken
            var school = await _db.SchoolAdministrators
                            .FirstOrDefaultAsync(s => s.SchoolAdminId == CurrentUserId);

            if (school == null)
            {
                school = new SchoolAdministrator
                {
                    SchoolAdminId = CurrentUserId,
                    Name = User.Identity!.Name!
                };
                _db.SchoolAdministrators.Add(school);
                await _db.SaveChangesAsync();
            }

            var user = new ApplicationUser
            {
                SchoolAdminId = school.Id // link teacher to this admin's school
            };

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
            var school = await _db.SchoolAdministrators
                .FirstOrDefaultAsync(s => s.SchoolAdminId == CurrentUserId);

            if (school == null) return Forbid();

            var user = await _userManager.FindByIdAsync(userId.ToString());

            // Make sure this teacher actually belongs to THIS admin's school
            if (user == null || user.SchoolAdminId != school.Id)
                return Forbid();

            await _userManager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }
    }
}
