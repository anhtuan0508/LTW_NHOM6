namespace PhanLaiAnhTuan_Lab03.Models
{
    public class EmailSettings
    {
        public string Email { get; set; } // Tài khoản Gmail (ví dụ: phanlaianhtuan@gmail.com)
        public string AppPassword { get; set; } // App password bạn lấy từ Google
        public string DisplayName { get; set; } // Tên hiển thị
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
    }
}
