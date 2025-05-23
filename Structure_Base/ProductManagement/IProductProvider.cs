﻿using Structure_Core.BaseClass;
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
    Task<ResultService<IEnumerable<ProductImage>>> GetImagesByProductCode(string proCode); 
    Task<ResultService<string>> DeleteProductAndImageByProductCode(string proCode);



    //For CLient
    Task<ResultService<List<ProductsWithFirstImageDto>>> GetAllProductsWithFirstImage();
    Task<ResultService<ProductWithAllImagesDto>> GetProductWithAllImagesByCode(string productCode);


    Task<ResultService<List<ProductsWithFirstImageDto>>> GetProductsWithFirstImageByBrandOrCategoryCode(string brandCode, string categoryCode);
}