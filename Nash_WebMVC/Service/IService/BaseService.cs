using Nash_WebMVC.Models;

namespace Nash_WebMVC.Service.IService;

public class BaseService : IBaseService
{
    public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
    {
        throw new NotImplementedException();
    }
}
