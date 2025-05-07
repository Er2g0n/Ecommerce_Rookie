using Nash_AuthAPI.Models.Dto;

namespace Nash_AuthAPI.Service.IService;

public interface IAuthService
{
    Task<string> Register(RegistrationRequestDto registrationRequestDto);
    Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
    Task<bool> AssignRole(string email, string roleName);
    Task<LoginResponseDto> LoginAdmin(LoginRequestDto loginRequestDto);

    Task<List<UserDto>> GetAllUsers();
}
