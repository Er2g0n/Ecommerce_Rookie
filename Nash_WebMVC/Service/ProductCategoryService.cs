using Nash_WebMVC.Models;
using Nash_WebMVC.Service.IService;
using Nash_WebMVC.Utility;
using Structure_Core.BaseClass;
using Structure_Core.ProductClassification;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Service;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IBaseService _baseService;
    public ProductCategoryService(IBaseService baseService)
    {
        _baseService = baseService;
    }
    public async Task<ResultService<List<ProductCategory>>> GetAllProductCategoriesAsync()
    {
        return await _baseService.SendAsync<List<ProductCategory>>(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductCategoryAPIBase + "/api/productCategory"
        });
    }
}
