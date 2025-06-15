using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PhanLaiAnhTuan_Lab03.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public bool Is2FAEnabled { get; set; } = false;
    }
}
