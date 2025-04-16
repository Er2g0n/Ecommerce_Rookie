using Structure_Core.BaseClass;
using Structure_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base;
public interface IProductCategoryProvider
{
    Task<ResultService<ProductCategory>> SaveByDapper(ProductCategory entity);
    Task<ResultService<ProductCategory>> GetByCode(string code);
    Task<ResultService<string>> DeleteByDapper(string code);
}
