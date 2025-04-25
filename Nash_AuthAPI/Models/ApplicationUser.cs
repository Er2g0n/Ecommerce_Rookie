using Microsoft.AspNetCore.Identity;

namespace Nash_AuthAPI.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }

}
