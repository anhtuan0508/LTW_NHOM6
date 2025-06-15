using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhanLaiAnhTuan_Lab03.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        public string UserId { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = "";
        public string Notes { get; set; } = "";
        public string Status { get; set; } = "Chờ Xác nhận";

        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}
