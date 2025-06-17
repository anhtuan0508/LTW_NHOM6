namespace PhanLaiAnhTuan_Lab03.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }              // ✅ ID người dùng
        public ApplicationUser User { get; set; }       // ✅ Navigation tới người dùng

        public string Comment { get; set; }
        public int Rating { get; set; } // 1 - 5 sao
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
