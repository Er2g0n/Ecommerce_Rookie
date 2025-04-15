using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Core.BaseClass;
public class ResultService<T>
{
    public string Message { get; set; }
    public string Code { get; set; }
    public T Data { get; set; }
}

///// 1, lỗi null
//// 2, lỗi không null
