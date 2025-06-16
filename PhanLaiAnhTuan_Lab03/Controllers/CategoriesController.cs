using Microsoft.AspNetCore.Mvc;
using PhanLaiAnhTuan_Lab03.Models;
using PhanLaiAnhTuan_Lab03.Repositories;
using System.Threading.Tasks;

namespace PhanLaiAnhTuan_Lab03.Controllers
{
    [Route("loai")] // Đường dẫn thân thiện thay vì "categories"
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoriesController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        [Route("")] // GET /loai
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var sortedCategories = categories
                .OrderBy(c => c.Name == "Chưa có loài" ? 1 : 0)
                .ThenBy(c => c.Name)
                .ToList();

            return View(sortedCategories);
        }

        [HttpGet]
        [Route("tao-moi")] // GET /loai/tao-moi
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("tao-moi")] // POST /loai/tao-moi
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpGet]
        [Route("chinh-sua/{id:int}")] // GET /loai/chinh-sua/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [Route("chinh-sua")] // POST /loai/chinh-sua
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateCategoryAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpGet]
        [Route("xoa/{id:int}")] // GET /loai/xoa/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [Route("xoa/{id:int}")] // POST /loai/xoa/5
        [ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var defaultCategory = await _categoryRepository.GetCategoryByNameAsync("Chưa có loài");
            if (defaultCategory == null)
            {
                var newCategory = new Category { Name = "Chưa có loài" };
                await _categoryRepository.AddCategoryAsync(newCategory);
                defaultCategory = newCategory;
            }
            var defaultCategoryId = defaultCategory.Id;

            if (id == defaultCategoryId)
            {
                ModelState.AddModelError("", "Không thể xóa loài mặc định.");
                var category = await _categoryRepository.GetCategoryById(id);
                return View("Delete", category);
            }

            var products = await _productRepository.GetProductsByCategoryIdAsync(id);
            foreach (var product in products)
            {
                product.CategoryId = defaultCategoryId;
                await _productRepository.UpdateAsync(product);
            }

            await _categoryRepository.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
