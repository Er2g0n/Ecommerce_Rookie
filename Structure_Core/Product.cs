using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Core;
public class Product : BaseClass.BaseClass
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string CategoryCode { get; set; }
    public string BrandCode { get; set; }
    public string UoMCode { get; set; }
    public string Description { get; set; }
}
public class ProductDto : BaseClass.BaseClass
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }

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

    public string Description { get; set; }
}