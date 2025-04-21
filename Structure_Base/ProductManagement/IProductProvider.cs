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
    Task<ResultService<Product>> GetByCode(string code);
    Task<ResultService<string>> DeleteByDapper(string code);
}
