using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using PhanLaiAnhTuan_Lab03.Models;
using System.ComponentModel.DataAnnotations;


namespace PhanLaiAnhTuan_Lab03.Areas.Identity.Pages.Account.Manage
{
    public class VerifyPasswordOtpModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public VerifyPasswordOtpModel(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string OTP { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var otp = TempData["Otp"]?.ToString();
            var newPassword = TempData["NewPassword"]?.ToString();

            if (Input.OTP != otp)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không chính xác.");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Không tìm thấy người dùng.");

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Không thể xoá mật khẩu cũ.");
                return Page();
            }

            var addResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Không thể thêm mật khẩu mới.");
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Mật khẩu đã được đổi thành công.";
            return RedirectToPage("ChangePassword");
        }
    }

}
