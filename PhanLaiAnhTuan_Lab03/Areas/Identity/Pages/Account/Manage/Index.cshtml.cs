using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhanLaiAnhTuan_Lab03.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Tuổi")]
        public string? Age { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        Input = new InputModel
        {
            FullName = user.FullName,
            Address = user.Address,
            Age = user.Age
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = Input.FullName;
        user.Address = Input.Address;
        user.Age = Input.Age;

        await _userManager.UpdateAsync(user);
        TempData["Success"] = "Cập nhật thông tin thành công!";
        return RedirectToPage();
    }
}
