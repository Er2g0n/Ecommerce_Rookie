using Structure_Core.ProductManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base.ProductManagement;
public interface IProductImageProvider
{
    Task<ProductImage> Save(ProductImage productImage);
}
