using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using PhanLaiAnhTuan_Lab03.Models;
using PhanLaiAnhTuan_Lab03.Repositories;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PhanLaiAnhTuan_Lab03.Filters;


namespace PhanLaiAnhTuan_Lab03.Controllers
{
    //[Authorize(Roles = "Admin")]
    //[Admin2FARequired]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;   // khai báo DbContext
        private readonly IWebHostEnvironment _environment; // khai báo để lưu file ảnh

        public ProductController(IProductRepository productRepository,
                                 ICategoryRepository categoryRepository,
                                 IWebHostEnvironment webHostEnvironment, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
            _context = context;               // khởi tạo DbContext qua DI
            _environment = environment;       // khởi tạo IWebHostEnvironment qua DI
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product); // Tạo view Details.cshtml trong thư mục Views/Product
        }
        private string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            input = input.Normalize(System.Text.NormalizationForm.FormD);
            var chars = input.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC).ToLower();
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, string sortOrder)
        {
            var products = await _productRepository.GetAllAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                var normalizedSearch = RemoveDiacritics(searchString);

                products = products
                    .Where(p => !string.IsNullOrEmpty(p.Name) && RemoveDiacritics(p.Name).Contains(normalizedSearch))
                    .ToList();
            }

            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;

            products = sortOrder switch
            {
                "name_asc" => products.OrderBy(p => p.Name).ToList(),
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                _ => products.ToList()
            };

            return View(products);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesToViewBag();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesToViewBag();
                return View(product);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = uniqueFileName;  // chỉ lưu tên file thôi
            }

            await _productRepository.AddAsync(product);
            return RedirectToAction(nameof(Index));
        }
        private async Task LoadCategoriesToViewBag()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(product);
        }


        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesToViewBag();
                return View(product);
            }

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null) return NotFound();

            // Cập nhật các trường dữ liệu
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", existingProduct.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                // Lưu ảnh mới
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                existingProduct.ImageUrl = uniqueFileName;
            }

            await _productRepository.UpdateAsync(existingProduct);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            // Xóa ảnh nếu có
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", product.ImageUrl);
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
