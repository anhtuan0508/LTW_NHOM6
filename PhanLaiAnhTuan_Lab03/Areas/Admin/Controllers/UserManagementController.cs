using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanLaiAnhTuan_Lab03.Models;

namespace PhanLaiAnhTuan_Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Hiển thị danh sách user và vai trò
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

        // GET: Form tạo nhân viên
        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        // POST: Tạo tài khoản nhân viên
        [HttpPost]
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
                return Unauthorized("Bạn không có quyền tạo tài khoản nhân viên.");
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
                TempData["Success"] = "Tạo tài khoản nhân viên thành công!";
                return RedirectToAction("CreateEmployee");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // Vô hiệu hóa user
        public async Task<IActionResult> Disable(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = "Không thể vô hiệu hóa Admin.";
                return RedirectToAction("Index");
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Đã vô hiệu hóa người dùng.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Enable(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Đã kích hoạt lại người dùng.";
            return RedirectToAction("Index");
        }

        // GET: Hiển thị xác nhận xóa (tuỳ chọn)
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user); // Tạo View xác nhận nếu muốn
        }

        [HttpPost, ActionName("Delete")]
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
                TempData["Error"] = "Không thể xóa người dùng có quyền Admin.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.DeleteAsync(user);
            TempData["Success"] = result.Succeeded
                ? "Đã xóa người dùng thành công."
                : "Không thể xóa người dùng.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains("Admin"))
            {
                TempData["Error"] = "Không thể thay đổi quyền Admin.";
                return RedirectToAction("Index");
            }

            // Xoá các vai trò hiện tại (trừ Admin)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = $"Đã cập nhật vai trò thành '{newRole}' cho {user.Email}.";
            return RedirectToAction("Index");
        }


    }
}
