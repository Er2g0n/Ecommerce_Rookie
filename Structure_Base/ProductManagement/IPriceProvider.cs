using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base.ProductManagement;
public interface IPriceProvider
{
    //Task<ResultService<Price>> Save(Price price);
    Task<ResultService<Price>> CreatePrice(Price price);

    Task<ResultService<Price>> GetByProductCode(string productCode);
    Task<ResultService<Price>> GetLatestPriceByProductCode(string productCode);

    Task<ResultService<string>> Delete(string priceCode);

} 