using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PhanLaiAnhTuan_Lab03.Models;
using PhanLaiAnhTuan_Lab03.Repositories;
using System.Text;
using System.Threading.Tasks;

namespace PhanLaiAnhTuan_Lab03.Controllers
{
    [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public HomeController(IProductRepository productRepo, ICategoryRepository categoryRepo)
        {
            _productRepository = productRepo;
            _categoryRepository = categoryRepo;
        }

        public async Task<IActionResult> Index(int? categoryId, string sortOrder, string searchString)
        {
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
                products = products.Where(p => p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();

            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId).ToList();

            products = sortOrder switch
            {
                "asc" => products.OrderBy(p => p.Price).ToList(),
                "desc" => products.OrderByDescending(p => p.Price).ToList(),
                _ => products
            };

            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.SortList = new SelectList(new List<SelectListItem>
    {
        new SelectListItem { Text = "Giá tăng", Value = "asc" },
        new SelectListItem { Text = "Giá giảm", Value = "desc" }
    }, "Value", "Text", sortOrder);

            ViewBag.CurrentSearch = searchString;

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product); // sẽ dùng Views/Home/Details.cshtml
        }
    }

}
