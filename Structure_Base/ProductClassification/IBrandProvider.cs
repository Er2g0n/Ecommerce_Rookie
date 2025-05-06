using Structure_Core.BaseClass;
using Structure_Core.ProductClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base.ProductClassification;
public interface IBrandProvider
{
    Task<ResultService<Brand>> SaveByDapper(Brand entity);
    Task<ResultService<Brand>> GetByCode(string code);
    Task<ResultService<string>> DeleteByDapper(string code);

}
