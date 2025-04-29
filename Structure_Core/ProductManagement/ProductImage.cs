using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Core.ProductManagement;
public class ProductImage : BaseClass.BaseClass
{
    public string? RefProductCode { get; set; }
    public int? Position { get; set; } 
    public string? ImagePath { get; set; } 
    public bool? IsPrimary { get; set; }
}
public class ImageDto
{
    public ProductImage Properties { get; set; }
    public IFormFile? File { get; set; }
}