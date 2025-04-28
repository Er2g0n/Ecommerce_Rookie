using Nash_WebMVC.Models;

namespace Nash_WebMVC.Service.IService;

public interface IBaseService
{
    Task<ResponseDto?> SendAsync(RequestDto requestDto);
}
