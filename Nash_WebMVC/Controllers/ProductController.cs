using Microsoft.AspNetCore.Mvc;
using Nash_WebMVC.Service.IService;
using Newtonsoft.Json;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Controllers;
//[Route("/Product")]
public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    //[HttpGet("Motorcyle")]
    public IActionResult Motorcycle()
    {
        return View();

    }
    //[HttpGet("Component")]
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

    public async Task<IActionResult> Detail(string code) //Bên Program đã đổi từ id thành code
    {
        Console.WriteLine("ProductCode received: " + code);

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("ProductCode is required");
        }

        var response = await _productService.GetProductWithAllImagesByCodeAsync(code);

        // Nếu tìm thấy thì trả về view
        if (response != null && response.Code == "0")
        {
            return View(response.Data); // response.Data có thể là Product_ProductImage_Dto
        }

        return NotFound("Product not found");
    }


}