using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Core.ProductManagement;
public class ProductImage : BaseClass.BaseClass
{
    public string RefProductCode { get; set; }
    public string ImagePath { get; set; }
    public bool IsPrimary { get; set; }
}
