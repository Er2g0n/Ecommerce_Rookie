using Nash_WebMVC.Models;
using Structure_Core.BaseClass;

namespace Nash_WebMVC.Service.IService;

public interface IBaseService
{
    Task<ResponseDto> SendAsync(RequestDto requestDto, bool withBearer = true);
    Task<ResultService<T>> SendAsync<T>(RequestDto requestDto, bool withBearer = true);
}

