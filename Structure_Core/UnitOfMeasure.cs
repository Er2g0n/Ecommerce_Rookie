using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Core;
public class UnitOfMeasure : BaseClass.BaseClass
{
    public string UoMCode { get; set; }       // Mã đơn vị đo lường
    public string UoMName { get; set; }     //Tên đơn vị đo lường (kg, cm, pcs, ...)
    public string? UoMDescription { get; set; }                  // Mô tả (nullable)

}
