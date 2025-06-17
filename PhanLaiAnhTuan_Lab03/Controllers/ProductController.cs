using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhanLaiAnhTuan_Lab03.Filters;
using PhanLaiAnhTuan_Lab03.Models;
using PhanLaiAnhTuan_Lab03.Repositories;
using System.Threading.Tasks;

namespace PhanLaiAnhTuan_Lab03.Controllers
{
    [Authorize(Roles = "Admin")]
    [Admin2FARequired]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;

        public ProductController(IProductRepository productRepository,
                                ICategoryRepository categoryRepository,
                                ApplicationDbContext context,
                                Cloudinary cloudinary)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _cloudinary = cloudinary;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        private string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            input = input.Normalize(System.Text.NormalizationForm.FormD);
            var chars = input.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(System.Text.NormalizationForm.FormC).ToLower();
        }

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

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesToViewBag();
            return View();
        }
        // chỗ này sửa này 
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
                var uploadResult = await UploadImageToCloudinary(imageFile);
                if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    product.ImageUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    ModelState.AddModelError("", "Tải ảnh lên Cloudinary thất bại.");
                    await LoadCategoriesToViewBag();
                    return View(product);
                }
            }

            await _productRepository.AddAsync(product);
            return RedirectToAction(nameof(Index));
        }

        private async Task<ImageUploadResult> UploadImageToCloudinary(IFormFile imageFile)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(imageFile.FileName, imageFile.OpenReadStream()),
                PublicId = $"{DateTime.Now.Ticks}_{Path.GetFileNameWithoutExtension(imageFile.FileName)}",
                Overwrite = false
            };

            return await _cloudinary.UploadAsync(uploadParams);
        }

        private async Task LoadCategoriesToViewBag()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
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

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadResult = await UploadImageToCloudinary(imageFile);
                if (uploadResult != null && uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var publicId = Path.GetFileNameWithoutExtension(existingProduct.ImageUrl.Split('/').Last());
                        var deletionParams = new DeletionParams(publicId)
                        {
                            ResourceType = ResourceType.Image
                        };
                        var deletionResult = await _cloudinary.DestroyAsync(deletionParams); // Sử dụng DestroyAsync
                        if (deletionResult.Error != null)
                        {
                            Console.WriteLine($"Error deleting old image from Cloudinary: {deletionResult.Error.Message}");
                        }
                    }
                    existingProduct.ImageUrl = uploadResult.SecureUrl.ToString();
                }
            }

            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var publicId = Path.GetFileNameWithoutExtension(product.ImageUrl.Split('/').Last());
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams); // Sử dụng DestroyAsync
                if (deletionResult.Error != null)
                {
                    Console.WriteLine($"Error deleting image from Cloudinary: {deletionResult.Error.Message}");
                }
            }

            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}