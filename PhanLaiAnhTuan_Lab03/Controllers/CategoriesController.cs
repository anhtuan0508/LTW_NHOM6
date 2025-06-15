using Microsoft.AspNetCore.Mvc;
using PhanLaiAnhTuan_Lab03.Models;
using PhanLaiAnhTuan_Lab03.Repositories;
using System.Threading.Tasks;


namespace PhanLaiAnhTuan_Lab03.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoriesController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        // GET: /Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();

            // Sắp xếp "Chưa có danh mục" xuống cuối bằng cách dùng OrderBy với điều kiện
            var sortedCategories = categories
                .OrderBy(c => c.Name == "Chưa có loài" ? 1 : 0)  // Đẩy "Chưa có danh mục" về cuối
                .ThenBy(c => c.Name)  // Các danh mục khác sắp xếp theo tên
                .ToList();

            return View(sortedCategories);
        }

        // GET: /Categories/Add
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
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


        // GET: /Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra xem danh mục mặc định "Chưa có danh mục" đã tồn tại chưa
            var defaultCategory = await _categoryRepository.GetCategoryByNameAsync("Chưa có loài");
            if (defaultCategory == null)
            {
                // Nếu chưa có thì tạo mới
                var newCategory = new Category
                {
                    Name = "Chưa có loài"
                };
                await _categoryRepository.AddCategoryAsync(newCategory);
                defaultCategory = newCategory;
            }
            var defaultCategoryId = defaultCategory.Id;

            // Không cho xóa danh mục mặc định
            if (id == defaultCategoryId)
            {
                ModelState.AddModelError("", "Không thể xóa loài mặc định.");
                var category = await _categoryRepository.GetCategoryById(id);
                return View("Delete", category);
            }

            // Lấy danh sách sản phẩm thuộc danh mục cần xóa
            var products = await _productRepository.GetProductsByCategoryIdAsync(id);

            // Cập nhật sản phẩm sang danh mục mặc định
            foreach (var product in products)
            {
                product.CategoryId = defaultCategoryId;
                await _productRepository.UpdateAsync(product);
            }

            // Xóa danh mục
            await _categoryRepository.DeleteCategoryAsync(id);

            return RedirectToAction(nameof(Index));
        }








    }
}
