using Nash_WebMVC.Models;
using Nash_WebMVC.Service.IService;
using Nash_WebMVC.Utility;
using Structure_Core.BaseClass;
using Structure_Core.ProductClassification;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Service;

public class BrandService : IBrandService
{
    private readonly IBaseService _baseService;
    public BrandService(IBaseService baseService)
    {
        _baseService = baseService;
    }
    public async Task<ResultService<List<Brand>>> GetAllBrandsAsync()
    {
        return await _baseService.SendAsync<List<Brand>>(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.BrandAPIBase + "/api/brand"
        });
    }
}
