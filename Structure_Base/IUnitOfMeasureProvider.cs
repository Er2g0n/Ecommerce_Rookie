using Structure_Core.BaseClass;
using Structure_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base;
public interface IUnitOfMeasureProvider
{
    Task<ResultService<UnitOfMeasure>> SaveByDapper(UnitOfMeasure entity);
    Task<ResultService<UnitOfMeasure>> GetByCode(string code);
    Task<ResultService<string>> DeleteByDapper(string code);
}
