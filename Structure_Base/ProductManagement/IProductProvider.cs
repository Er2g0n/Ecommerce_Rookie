using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base.ProductManagement;
public interface IProductProvider
{
    Task<ResultService<Product>> SaveByDapper(Product entity);
    Task<ResultService<Product>> GetByCode(string proCode);
    Task<ResultService<string>> DeleteByDapper(string proCode);
    Task<ResultService<Product_ProductImage_Dto>> SaveProductAndImage(Product_ProductImage_Dto entity);
    Task<ResultService<IEnumerable<ProductImage>>> GetImagesByProductCode(string proCode); // Cập nhật ở đây
    Task<ResultService<string>> DeleteProductAndImageByProductCode(string proCode);// Updated signature}
}