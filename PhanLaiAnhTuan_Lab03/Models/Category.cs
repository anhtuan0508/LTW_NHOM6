using System.ComponentModel.DataAnnotations;

namespace PhanLaiAnhTuan_Lab03.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        public List<Product>? Products { get; set; }

        // thêm  ơ  dây ne 
        public int? ParentCategoryId { get; set; } // Foreign key cho danh mục cha
        public List<Category>? SubCategories { get; set; } = new List<Category>(); // Danh sách danh mục con
        public Category? ParentCategory { get; set; } // Thêm dòng này để tham chiếu đến danh mục cha
    }

}
