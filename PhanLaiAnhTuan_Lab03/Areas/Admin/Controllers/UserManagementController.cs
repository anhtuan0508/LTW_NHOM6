using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanLaiAnhTuan_Lab03.Models;
using Microsoft.Extensions.Localization;

namespace PhanLaiAnhTuan_Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("admin/nguoi-dung")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<UserManagementController> _localizer;

        public UserManagementController(UserManager<ApplicationUser> userManager,
                                        IStringLocalizer<UserManagementController> localizer)
        {
            _userManager = userManager;
            _localizer = localizer;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpGet]
        [Route("tao-nhan-vien")]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        [HttpPost]
        [Route("tao-nhan-vien")]
        public async Task<IActionResult> CreateEmployee(EmployeeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return Unauthorized(_localizer["UnauthorizedCreateEmployee"]);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Employee");
                TempData["Success"] = _localizer["CreateEmployeeSuccess"];
                return RedirectToAction("CreateEmployee");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [Route("vo-hieu-hoa/{id}")]
        public async Task<IActionResult> Disable(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = _localizer["CannotDisableAdmin"];
                return RedirectToAction("Index");
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = _localizer["UserDisabled"];
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("kich-hoat/{id}")]
        public async Task<IActionResult> Enable(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = _localizer["UserEnabled"];
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("xoa/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Route("xoa/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = _localizer["CannotDeleteAdmin"];
                return RedirectToAction("Index");
            }

            var result = await _userManager.DeleteAsync(user);
            TempData["Success"] = result.Succeeded
                ? _localizer["DeleteSuccess"]
                : _localizer["DeleteFail"];

            return RedirectToAction("Index");
        }
    }
}
