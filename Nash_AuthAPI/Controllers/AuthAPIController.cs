using Microsoft.AspNetCore.Mvc;

namespace Nash_AuthAPI.Controllers;
public class AuthAPIController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
