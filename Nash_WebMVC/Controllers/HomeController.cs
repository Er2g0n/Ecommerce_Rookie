using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nash_WebMVC.Models;
using Nash_WebMVC.Service;
using Nash_WebMVC.Service.IService;
using Nash_WebMVC.Utility;
using Structure_Core.ProductClassification;
using Structure_Core.ProductManagement;
using System.Diagnostics;

namespace Nash_WebMVC.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductService _productService;
    public HomeController(ILogger<HomeController> logger,IProductCategoryService productCategoryService, IProductService productService)
    {
        _logger = logger;
        _productCategoryService = productCategoryService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var categoriesResponse = await _productCategoryService.GetAllProductCategoriesAsync();
        if (categoriesResponse != null && categoriesResponse.Code == "0" && categoriesResponse.Data != null)
        {
            ViewBag.Categories = categoriesResponse.Data;
            ViewBag.ProductsByCategory = new Dictionary<string, List<ProductsWithFirstImageDto>>();
            foreach (var category in ViewBag.Categories)
            {
                var productsResponse = await _productService.GetProductsWithFirstImageByBrandOrCategoryCodeAsync(null, category.CategoryCode);
                ViewBag.ProductsByCategory[category.CategoryCode] = productsResponse != null && productsResponse.Code == "0"
                    ? productsResponse.Data
                    : new List<ProductsWithFirstImageDto>();
            }
        }
        else
        {
            ViewBag.Categories = new List<ProductCategory>();
            ViewBag.ProductsByCategory = new Dictionary<string, List<ProductsWithFirstImageDto>>();
        }
        return View();
    }
    public IActionResult Nav()
    {
        return View();
    }
    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult News()
    {
        return View();
    }
    public IActionResult Contact()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
