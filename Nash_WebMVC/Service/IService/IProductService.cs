using Nash_WebMVC.Models;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;

namespace Nash_WebMVC.Service.IService;

public interface IProductService
{
    Task<ResponseDto?> GetAllProductsAsync();
    Task<ResponseDto?> GetProductByCodeAsync(string proCode);
    Task<ResponseDto?> GetImagesByProductCodeAsync(string productCode);
    Task<ResultService<List<ProductsWithFirstImageDto>>> GetAllProductsWithFirstImageAsync();

}
