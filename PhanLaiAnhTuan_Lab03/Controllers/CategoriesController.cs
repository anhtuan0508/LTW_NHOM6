using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            // Lọc chỉ các danh mục cha (loài chính)
            var parentCategories = _categoryRepository.GetAllCategoriesAsync().Result
                .Where(c => c.ParentCategoryId == null);

            ViewBag.CategoryList = new SelectList(parentCategories, "Id", "Name");

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
            // ✅ THÊM lại ViewBag nếu lỗi validate
            ViewBag.CategoryList = new SelectList(await _categoryRepository.GetAllCategoriesAsync(), "Id", "Name");
            return View(category);
        }

        //  them đoạn xác nhận sửa

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }

            var allCategories = await _categoryRepository.GetAllCategoriesAsync();

            // ✅ Chỉ lấy danh mục cha (ParentId == null), và khác chính nó
            ViewBag.AllCategories = allCategories
                .Where(c => c.Id != category.Id && c.ParentCategoryId == null)
                .ToList();

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

            var allCategories = await _categoryRepository.GetAllCategoriesAsync();

            // ✅ Gửi lại danh mục cha đúng cách nếu ModelState lỗi
            ViewBag.AllCategories = allCategories
                .Where(c => c.Id != category.Id && c.ParentCategoryId == null)
                .ToList();

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

            // Kiểm tra nếu có danh mục con
            var allCategories = await _categoryRepository.GetAllCategoriesAsync();
            var hasChildren = allCategories.Any(c => c.ParentCategoryId == id);
            ViewBag.HasSubCategories = hasChildren;

            return View(category);
        }


        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra danh mục mặc định "Chưa có loài"
            var defaultCategory = await _categoryRepository.GetCategoryByNameAsync("Chưa có loài");
            if (defaultCategory == null)
            {
                var newCategory = new Category { Name = "Chưa có loài" };
                await _categoryRepository.AddCategoryAsync(newCategory);
                defaultCategory = newCategory;
            }

            var defaultCategoryId = defaultCategory.Id;

            // Không cho xóa danh mục mặc định
            if (id == defaultCategoryId)
            {
                ModelState.AddModelError("", "Không thể xóa loài mặc định.");
                return View("Delete", category);
            }

            // ✅ Gỡ liên kết với các danh mục con (nếu có)
            var allCategories = await _categoryRepository.GetAllCategoriesAsync();
            var subCategories = allCategories.Where(c => c.ParentCategoryId == id).ToList();

            foreach (var sub in subCategories)
            {
                sub.ParentCategoryId = null;
                await _categoryRepository.UpdateCategoryAsync(sub);
            }

            // ✅ Cập nhật sản phẩm sang danh mục mặc định
            var products = await _productRepository.GetProductsByCategoryIdAsync(id);
            foreach (var product in products)
            {
                product.CategoryId = defaultCategoryId;
                await _productRepository.UpdateAsync(product);
            }

            // ✅ Xóa danh mục
            await _categoryRepository.DeleteCategoryAsync(id);

            return RedirectToAction(nameof(Index));
        }





    }
}
