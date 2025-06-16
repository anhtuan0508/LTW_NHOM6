using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity.UI.Services;
using PhanLaiAnhTuan_Lab03.Models;

namespace PhanLaiAnhTuan_Lab03.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu hiện tại")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} phải từ {2} đến {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu mới")]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // Kiểm tra mật khẩu hiện tại
            if (!await _userManager.CheckPasswordAsync(user, Input.OldPassword))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                return Page();
            }

            // Gửi OTP thay vì đổi mật khẩu trực tiếp
            var otp = new Random().Next(100000, 999999).ToString();
            TempData["Otp"] = otp;
            TempData["NewPassword"] = Input.NewPassword;

            await _emailSender.SendEmailAsync(
                user.Email,
                "Xác thực OTP đổi mật khẩu",
                $"Mã xác thực OTP của bạn là: <strong>{otp}</strong>");

            return RedirectToPage("VerifyPasswordOtp");
        }
    }
}
