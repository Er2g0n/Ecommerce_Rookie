using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base.BaseService;
public interface ICRUD_Service<T, U>
{
    //Task<ResultService<T>> Create(T entity);
    //Task<ResultService<T>> Update(T entity);  
    Task<ResultService<string>> Delete(U id);
    Task<ResultService<T>> Get(U id);
    Task<ResultService<IEnumerable<T>>> GetAll();
}
