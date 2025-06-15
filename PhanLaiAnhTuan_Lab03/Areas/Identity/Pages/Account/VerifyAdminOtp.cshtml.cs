using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using PhanLaiAnhTuan_Lab03.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;

namespace PhanLaiAnhTuan_Lab03.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class VerifyAdminOtpModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<VerifyAdminOtpModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyAdminOtpModel(SignInManager<ApplicationUser> signInManager,
                                   UserManager<ApplicationUser> userManager,
                                   ILogger<VerifyAdminOtpModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public string OtpCode { get; set; }

        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            var expectedOtp = HttpContext.Session.GetString("AdminOTP");
            var userEmail = HttpContext.Session.GetString("AdminEmail");

            if (string.IsNullOrEmpty(expectedOtp) || string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError(string.Empty, "Xác minh thất bại. Vui lòng đăng nhập lại.");
                return RedirectToPage("./Login");
            }

            if (OtpCode == expectedOtp)
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user != null)
                {
                    // ✅ Cập nhật cờ xác thực
                    user.Is2FAEnabled = true;
                    await _userManager.UpdateAsync(user);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("Admin đã xác thực OTP và đăng nhập thành công.");

                    HttpContext.Session.Remove("AdminOTP");
                    HttpContext.Session.Remove("AdminEmail");

                    return RedirectToAction("Index", "Product", new { area = "" });

                }
            }

            ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ.");
            return Page();
        }

    }
}
