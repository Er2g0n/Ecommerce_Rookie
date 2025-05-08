using Nash_WebMVC.Models;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Service.IService;

public interface IProductService
{



    Task<ResultService<List<ProductsWithFirstImageDto>>> GetAllProductsWithFirstImageAsync();
    Task<ResultService<ProductWithAllImagesDto>> GetProductWithAllImagesByCodeAsync(string productCode);
    Task<ResultService<List<ProductsWithFirstImageDto>>> GetProductsWithFirstImageByBrandOrCategoryCodeAsync(string brandCode, string categoryCode);
}
