using Nash_AuthAPI.Models;

namespace Nash_AuthAPI.Service.IService;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);

}
