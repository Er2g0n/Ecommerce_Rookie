using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Core.ProductManagement;
public class Price : BaseClass.BaseClass
{
    public string PriceCode { get; set; }
    public string ProductCode { get; set; }
    public decimal? PriceCost { get; set; }
    public decimal? PriceSale { get; set; }
    public decimal? PriceVAT { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ApplyDate { get; set; }
    public int PriceStatus { get; set; }
}
