using Microsoft.AspNetCore.Mvc;
using PhanLaiAnhTuan_Lab03.Models;

namespace PhanLaiAnhTuan_Lab03.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult UpdateStock(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStock(int id, int quantityToAdd)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            product.Quantity += quantityToAdd;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
