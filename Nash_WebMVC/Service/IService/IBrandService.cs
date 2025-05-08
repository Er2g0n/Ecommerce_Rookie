using Structure_Core.BaseClass;
using Structure_Core.ProductClassification;

namespace Nash_WebMVC.Service.IService;

public interface IBrandService
{
    Task<ResultService<List<Brand>>> GetAllBrandsAsync();
}
