using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PhanLaiAnhTuan_Lab03.Models;
using PhanLaiAnhTuan_Lab03.Repositories;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace PhanLaiAnhTuan_Lab03.Controllers
{
    [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        private readonly IConfiguration _configuration;

        public HomeController(IProductRepository productRepo, ICategoryRepository categoryRepo, IConfiguration configuration)
        {
            _productRepository = productRepo;
            _categoryRepository = categoryRepo;
            _configuration = configuration;
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
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetSuggestions(string term)
        {
            var suggestions = await _productRepository.GetAllAsync();
            var filteredSuggestions = suggestions
                .Where(p => !string.IsNullOrEmpty(p.Name) && (string.IsNullOrEmpty(term) || p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Select(p => new { label = p.Name, value = p.Name })
                .Take(10) // Giới hạn 10 gợi ý
                .ToList();

            return Json(filteredSuggestions);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Chat()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Chat(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                ViewBag.AIResponse = "Vui lòng nhập câu hỏi.";
                return View();
            }

            var invalidKeywords = new[] { "chính trị", "tin tức", "lập trình", "game", "bóng đá", "ca sĩ", "chứng khoán", "bitcoin" };
            if (invalidKeywords.Any(k => userMessage.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                ViewBag.AIResponse = "❌ Xin lỗi, tôi chỉ hỗ trợ các câu hỏi liên quan đến thú cưng và cửa hàng thú cưng.";
                return View();
            }

            // ✅ Lấy dữ liệu từ database
            var allProducts = await _productRepository.GetAllAsync();
            var productDescriptions = allProducts
                .Select(p => $"- {p.Name}, giá: {p.Price:N0} VND")
                .ToList();

            var productContext = string.Join("\n", productDescriptions);
            var aiPrompt = $"""
    Bạn là trợ lý bán hàng cho website thú cưng. Dưới đây là danh sách sản phẩm hiện có:

    {productContext}

    Chỉ trả lời các câu hỏi liên quan đến thú cưng và sản phẩm bên dưới. Nếu không có sản phẩm khách hỏi, hãy trả lời lịch sự là cửa hàng không có.
    """;

            ViewBag.ProductInfo = "🛒 Một số sản phẩm hiện có:\n" + productContext;

            using var httpClient = new HttpClient();
            var apiKey = _configuration["OpenRouter:ApiKey"];
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://localhost:5001");

            var requestBody = new
            {
                model = "openai/gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = aiPrompt },
            new { role = "user", content = userMessage }
        }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            string reply = "Phản hồi không xác định từ AI.";

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            if (root.TryGetProperty("choices", out var choices))
            {
                reply = choices[0].GetProperty("message").GetProperty("content").GetString();
            }
            else if (root.TryGetProperty("error", out var error))
            {
                reply = $"Lỗi từ OpenRouter: {error.GetProperty("message").GetString()}";
            }

            ViewBag.AIResponse = reply;
            ViewBag.UserMessage = userMessage;

            // ✅ Thêm lịch sử trò chuyện vào Session
            var history = HttpContext.Session.GetString("ChatHistory");
            var messages = string.IsNullOrEmpty(history)
                ? new List<(string User, string AI)>()
                : JsonSerializer.Deserialize<List<(string User, string AI)>>(history);

            messages.Add((userMessage, reply));
            HttpContext.Session.SetString("ChatHistory", JsonSerializer.Serialize(messages));

            ViewBag.History = messages;

            return View();
        }
    }}