using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanLaiAnhTuan_Lab03.Models;

namespace PhanLaiAnhTuan_Lab03.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách đơn hàng
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .AsNoTracking() // ❗ rất quan trọng để lấy dữ liệu mới nhất
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // Xác nhận đơn
        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (order.Status == "Đã xác nhận")
            {
                TempData["Warning"] = "❗ Đơn hàng đã được xác nhận trước đó.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra tồn kho
            foreach (var detail in order.OrderDetails)
            {
                if (detail.Product != null && detail.Product.Quantity < detail.Quantity)
                {
                    TempData["Error"] = $"❌ Không đủ hàng cho: {detail.Product.Name}. Có {detail.Product.Quantity}, cần {detail.Quantity}.";
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
            }

            // Trừ kho
            foreach (var detail in order.OrderDetails)
            {
                if (detail.Product != null)
                {
                    detail.Product.Quantity -= detail.Quantity;
                    _context.Products.Update(detail.Product); // đảm bảo EF theo dõi thay đổi
                }
            }

            order.Status = "Đã xác nhận";
            _context.Orders.Update(order); // đảm bảo EF theo dõi thay đổi

            await _context.SaveChangesAsync(); // lưu trừ kho + trạng thái

            TempData["Success"] = "✅ Đã xác nhận đơn hàng.";
            return RedirectToAction(nameof(Index));
        }

        // Hủy đơn
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Đã hủy";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
