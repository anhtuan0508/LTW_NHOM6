using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanLaiAnhTuan_Lab03.Data;
using PhanLaiAnhTuan_Lab03.Models;
using System.Linq;
using System.Threading.Tasks;
using PhanLaiAnhTuan_Lab03.Models.ViewModels;


namespace PhanLaiAnhTuan_Lab03.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Statistics()
        {
            var stats = await _context.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .Where(od => od.Order.Status == "Đã xác nhận")
                .GroupBy(od => od.Product.Name)
                .Select(g => new ProductSalesViewModel
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    OrderDate = g.Max(x => x.Order.OrderDate) // ngày bán gần nhất
                })
                .ToListAsync();

            return View(stats);
        }


        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            if (order.Status == "Đã xác nhận")
            {
                TempData["Message"] = "Đơn hàng đã được xác nhận trước đó.";
                return RedirectToAction("Index");
            }

            // ✅ Kiểm tra tồn kho trước
            foreach (var item in order.OrderDetails)
            {
                if (item.Product.Quantity < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm '{item.Product.Name}' không đủ hàng (tồn kho: {item.Product.Quantity}).";
                    return RedirectToAction("Index");
                }
            }

            // ✅ Trừ số lượng
            foreach (var item in order.OrderDetails)
            {
                item.Product.Quantity -= item.Quantity;
            }

            order.Status = "Đã xác nhận";
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đơn hàng đã được xác nhận và kho đã được cập nhật.";
            return RedirectToAction("Index");
        }
    }
}
