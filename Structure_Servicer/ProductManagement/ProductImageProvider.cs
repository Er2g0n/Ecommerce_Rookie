using Structure_Base.ProductManagement;
using Structure_Context.ProductManagement;
using Structure_Core.ProductManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Servicer.ProductManagement;
public class ProductImageProvider : IProductImageProvider
{
    private readonly DB_ProductManagement_Context _db;

    public ProductImageProvider(DB_ProductManagement_Context db)
    {
        _db = db;
    }

    public async Task<ProductImage> Save(ProductImage productImage) 
    {
        try
        {
            _db.ProductImages.Add(productImage);
            await _db.SaveChangesAsync();
        }
        catch(Exception ex)
        {
        }
        return productImage;

    }
}
