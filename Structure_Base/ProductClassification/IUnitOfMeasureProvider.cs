using Structure_Core.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structure_Core.ProductClassification;

namespace Structure_Base.ProductClassification;
public interface IUnitOfMeasureProvider
{
    Task<ResultService<UnitOfMeasure>> SaveByDapper(UnitOfMeasure entity);
    Task<ResultService<UnitOfMeasure>> GetByCode(string code);
    Task<ResultService<string>> DeleteByDapper(string code);
}
