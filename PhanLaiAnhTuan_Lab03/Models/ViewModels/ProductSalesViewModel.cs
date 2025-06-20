namespace PhanLaiAnhTuan_Lab03.Models.ViewModels
{
    public class ProductSalesViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public DateTime OrderDate { get; set; } // ngày bán gần nhất
    }
}
