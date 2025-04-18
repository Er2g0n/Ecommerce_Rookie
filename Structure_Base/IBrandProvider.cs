﻿using Structure_Core;
using Structure_Core.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base;
public interface IBrandProvider
{
    Task<ResultService<Brand>> SaveByDapper(Brand entity);
    Task<ResultService<Brand>> GetByCode(string code);
    Task<ResultService<string>> DeleteByDapper(string code);
}
