using Microsoft.AspNetCore.Identity;
using Nash_AuthAPI.Data;
using Nash_AuthAPI.Models;
using Nash_AuthAPI.Models.Dto;
using Nash_AuthAPI.Service.IService;

namespace Nash_AuthAPI.Service;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    //private readonly RoleManager<IdentityRole> _roleManager;
    //private readonly IJwtTokenGenerator _jwtTokenGenerator;
    public AuthService(AppDbContext db,
    UserManager<ApplicationUser> userManager)
    {
        _db = db;

        _userManager = userManager;
        //_roleManager = roleManager;
    }

    public Task<bool> AssignRole(string email, string roleName)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
    {
        throw new NotImplementedException();
    }

    public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
    {
        ApplicationUser user = new()
        {
            UserName = registrationRequestDto.Email,
            Email = registrationRequestDto.Email,
            NormalizedEmail = registrationRequestDto.Email.ToUpper(),
            Name = registrationRequestDto.Name,
            PhoneNumber = registrationRequestDto.PhoneNumber
        };

        try
        {
            var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
            if (result.Succeeded)
            {
                var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);

                UserDto userDto = new()
                {
                    Email = userToReturn.Email,
                    ID = userToReturn.Id,
                    Name = userToReturn.Name,
                    PhoneNumber = userToReturn.PhoneNumber
                };

                return "";

            }
            else
            {
                return result.Errors.FirstOrDefault().Description;
            }

        }
        catch (Exception ex)
        {

        }
        return "Error Encountered";
    }
}

