using Nash_WebMVC.Models;
using Nash_WebMVC.Service.IService;
using Nash_WebMVC.Utility;

namespace Nash_WebMVC.Service;

public class AuthService : IAuthService
{
    private readonly IBaseService _baseService;

    public AuthService( IBaseService baseService)
    {
        _baseService = baseService;
    }

    public async Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto registrationRequestDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = registrationRequestDto,
            Url = SD.AuthAPIBase + "/api/auth/AssignRole"
        });
    }

    public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = loginRequestDto,
            Url = SD.AuthAPIBase + "/api/auth/login"
        });
    }

    public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Data = registrationRequestDto,
            Url = SD.AuthAPIBase + "/api/auth/register"
        });
    }
}
