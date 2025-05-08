using Nash_WebMVC.Models;
using Nash_WebMVC.Service.IService;
using Nash_WebMVC.Utility;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Service;

public class ProductService : IProductService
{
    private readonly IBaseService _baseService;

    public ProductService(IBaseService baseService)
    {
        _baseService = baseService;
    }
   
    public async Task<ResultService<List<ProductsWithFirstImageDto>>> GetAllProductsWithFirstImageAsync()
    {
        return await _baseService.SendAsync<List<ProductsWithFirstImageDto>> (new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product/allWithFirstImage"
        });
    }
    public async Task<ResultService<ProductWithAllImagesDto>> GetProductWithAllImagesByCodeAsync(string productCode)
    {
        return await _baseService.SendAsync<ProductWithAllImagesDto>(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product/code/" + productCode + "/details"
        });
    }
    public async Task<ResultService<List<ProductsWithFirstImageDto>>> GetProductsWithFirstImageByBrandOrCategoryCodeAsync(string brandCode, string categoryCode)
    {
        var query = string.Empty;
        if (!string.IsNullOrEmpty(brandCode) || !string.IsNullOrEmpty(categoryCode))
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(brandCode)) queryParams.Add($"brandCode={Uri.EscapeDataString(brandCode)}");
            if (!string.IsNullOrEmpty(categoryCode)) queryParams.Add($"categoryCode={Uri.EscapeDataString(categoryCode)}");
            query = "?" + string.Join("&", queryParams);
        }

        return await _baseService.SendAsync<List<ProductsWithFirstImageDto>>(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product/filterByBrandOrCategory" + query
        });
    }
}
