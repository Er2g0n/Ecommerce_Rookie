using Microsoft.AspNetCore.Mvc;
using Nash_WebMVC.Service.IService;
using Newtonsoft.Json;
using Structure_Core.BaseClass;
using Structure_Core.ProductClassification;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Controllers;
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductCategoryService _productCategoryService;
    private readonly IBrandService _brandService;


    public ProductController(IProductService productService, IProductCategoryService productCategoryService, IBrandService brandService)
    {
        _productService = productService;
        _productCategoryService = productCategoryService;
        _brandService = brandService;
    }



    public async Task<IActionResult> AllProducts(string categoryCode = null, string brandCode = null)
    {
        // Fetch categories
        var categoriesResponse = await _productCategoryService.GetAllProductCategoriesAsync();
        var categories = new List<ProductCategory>();
        if (categoriesResponse != null && categoriesResponse.Code == "0" && categoriesResponse.Data != null)
        {
            categories = categoriesResponse.Data;
        }
        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = categoryCode;

        // Fetch brands
        var brandsResponse = await _brandService.GetAllBrandsAsync();
        var brands = new List<Brand>();
        if (brandsResponse != null && brandsResponse.Code == "0" && brandsResponse.Data != null)
        {
            brands = brandsResponse.Data;
        }
        ViewBag.Brands = brands;
        ViewBag.SelectedBrand = brandCode;

        // Fetch products (filtered if categoryCode or brandCode is provided)
        ResultService<List<ProductsWithFirstImageDto>> response;
        if (!string.IsNullOrEmpty(categoryCode) || !string.IsNullOrEmpty(brandCode))
        {
            response = await _productService.GetProductsWithFirstImageByBrandOrCategoryCodeAsync(brandCode, categoryCode);
        }
        else
        {
            response = await _productService.GetAllProductsWithFirstImageAsync();
        }

        var products = response != null && response.Code == "0" ? response.Data : new List<ProductsWithFirstImageDto>();
        return View(products);
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