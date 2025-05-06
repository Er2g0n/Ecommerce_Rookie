using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Core.ProductManagement;
public class Product : BaseClass.BaseClass
{
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? CategoryCode { get; set; }
    public string? BrandCode { get; set; }
    public string? UoMCode { get; set; }
    public string? Description { get; set; }
}

public class Product_ProductImage_Dto
{
    public string? CreatedBy { get; set; }
    public List<Product>? Products { get; set; }
    public List<ProductImage>? ProductImages{ get; set; }
}

public class ProductForClient
{
    
}

public class ProductsWithFirstImageDto
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string? CategoryCode { get; set; }
    public string? BrandCode { get; set; }
    public string? Description { get; set; }
    public string FirstImagePath { get; set; }
    public int SalePrice { get; set; }  

}

