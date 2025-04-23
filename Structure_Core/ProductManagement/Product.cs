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
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string ProductImageUrl { get; set; }
    public string CategoryCode { get; set; }
    public string BrandCode { get; set; }
    public string UoMCode { get; set; }
    public string Description { get; set; }
}
public class ProductDto : BaseClass.BaseClass // Để sử dụng cho việc truyền dữ liệu từ client đến server
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string Description { get; set; }


    //ProductCategory
    public string CategoryCode { get; set; }
    public string CategoryName { get; set; }
    //Brand
    public string BrandCode { get; set; }
    public string BrandName { get; set; }
    //UnitOfMeasure
    public string UoMCode { get; set; }
    public string UoMName { get; set; }
    public string UoMDescription { get; set; }

    public string CreatedBy { get; set; }
    public List<IFormFile> Images { get; set; } // Danh sách hình ảnh
    public int PrimaryImageIndex { get; set; } = 0; // Chỉ số của hình ảnh chính trong danh sách Images
}
public class Product_ProductImage_Dto
{
    public string CreatedBy { get; set; }
    public List<Product> Products { get; set; }
    public List<ProductImage> ProductImages{ get; set; }
}
