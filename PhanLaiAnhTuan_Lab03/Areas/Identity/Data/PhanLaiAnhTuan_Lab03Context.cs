using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhanLaiAnhTuan_Lab03.Models; // Thêm namespace của ApplicationUser

namespace PhanLaiAnhTuan_Lab03.Data;

public class PhanLaiAnhTuan_Lab03Context : IdentityDbContext<ApplicationUser> // Sửa IdentityUser thành ApplicationUser
{
    public PhanLaiAnhTuan_Lab03Context(DbContextOptions<PhanLaiAnhTuan_Lab03Context> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
    }
}