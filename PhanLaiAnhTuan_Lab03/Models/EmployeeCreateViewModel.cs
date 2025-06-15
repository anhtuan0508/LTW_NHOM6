using System;
using System.ComponentModel.DataAnnotations;

namespace PhanLaiAnhTuan_Lab03.Models
{
    public class EmployeeCreateViewModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [Display(Name = "Chức vụ")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Ngày vào làm không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày vào làm")]
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
