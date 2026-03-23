using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Viralis.Common.Constants;
using Viralis.Common.ViewModels.Admin;
using Viralis.Data.Models;

namespace Viralis.Web.Controllers
{
    [Authorize(Roles = RoleConstants.ADMIN)]
    public class AdminController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Users(string? search, string? role)
        {
            var users = await _userManager.Users.ToListAsync();

            var viewModels = new List<AdminUserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModels.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            if (!string.IsNullOrWhiteSpace(search))
                viewModels = viewModels.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(role) && role != "All")
                viewModels = viewModels.Where(u => u.Role == role).ToList();

            ViewBag.Search = search;
            ViewBag.RoleFilter = role ?? "All";
            ViewBag.TotalUsers = viewModels.Count;

            return View(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(Guid userId, string newRole)
        {
            var validRoles = new[] { RoleConstants.STUDENT, RoleConstants.TEACHER, RoleConstants.SCHOOL_ADMINISTRATOR, RoleConstants.ADMIN };
            if (!validRoles.Contains(newRole))
            {
                TempData["Error"] = "Invalid role.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = $"Role updated to {newRole} for {user.Email}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Success"] = $"{user.Email} has been unlocked.";
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["Success"] = $"{user.Email} has been locked.";
            }

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            if (user.Id == CurrentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"{user.Email} has been deleted.";
            return RedirectToAction(nameof(Users));
        }
    }
}
