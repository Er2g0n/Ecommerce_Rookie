using Microsoft.AspNetCore.Mvc;
using Nash_WebMVC.Service.IService;
using Newtonsoft.Json;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }


    public async Task<IActionResult> Motorcycle()
    {
        return View();

    }
    public IActionResult Component()
    {
        return View();
    }
    public async Task<IActionResult> AllProducts()
    {
        var response = await _productService.GetAllProductsWithFirstImageAsync();
        if (response != null && response.Code == "0")
        {
           
                return View(response.Data);
        }
        // Nếu không có dữ liệu, trả về danh sách rỗng hoặc view lỗi
        return View(new List<ProductsWithFirstImageDto>());
    }
    public  IActionResult Detail()
    {
        return View();
    }

}