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
    //public async Task<ResponseDto?> GetAllProductsAsync()
    //{
    //    return await _baseService.SendAsync(new RequestDto()
    //    {
    //        ApiType = SD.ApiType.GET,
    //        Url = SD.ProductAPIBase + "/api/product"
    //    });
    //}

    //public async Task<ResponseDto?> GetProductByCodeAsync(string proCode)
    //{
    //    return await _baseService.SendAsync(new RequestDto()
    //    {
    //        ApiType = SD.ApiType.GET,
    //        Url = SD.ProductAPIBase + "/api/product/code/" + proCode
    //    });
    //}

    //public async Task<ResponseDto?> GetImagesByProductCodeAsync(string productCode)
    //{
    //    return await _baseService.SendAsync(new RequestDto()
    //    {
    //        ApiType = SD.ApiType.GET,
    //        Url = SD.ProductAPIBase + "/api/product/code/" + productCode + "/images"
    //    });
    //}
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
}
