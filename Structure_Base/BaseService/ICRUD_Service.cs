﻿using Structure_Core.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base.BaseService;
public interface ICRUD_Service<T, U>
{
    Task<ResultService<string>> Delete(U id);
    Task<ResultService<T>> Get(U id);
    Task<ResultService<IEnumerable<T>>> GetAll();
}
