using Structure_Core.BaseClass;
using Structure_Core.ProductClassification;

namespace Nash_WebMVC.Service.IService;

public interface IProductCategoryService
{
    Task<ResultService<List<ProductCategory>>> GetAllProductCategoriesAsync();
}
