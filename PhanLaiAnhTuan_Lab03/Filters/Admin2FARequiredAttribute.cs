using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PhanLaiAnhTuan_Lab03.Models;

namespace PhanLaiAnhTuan_Lab03.Filters
{
    public class Admin2FARequiredAttribute : TypeFilterAttribute
    {
        public Admin2FARequiredAttribute() : base(typeof(Admin2FARequiredFilter)) { }

        private class Admin2FARequiredFilter : IAsyncAuthorizationFilter
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public Admin2FARequiredFilter(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
            {
                _userManager = userManager;
                _httpContextAccessor = httpContextAccessor;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

                if (user == null || !_httpContextAccessor.HttpContext.User.IsInRole("Admin"))
                    return; // Cho qua nếu không phải Admin

                if (!user.Is2FAEnabled)
                {
                    context.Result = new RedirectToPageResult("/Account/VerifyAdminOtp", new { area = "Identity" });
                }
            }
        }
    }
}
