using Microsoft.AspNetCore.Mvc;

namespace Nash_WebMVC.Controllers;
public class ProductController : Controller
{
    public IActionResult Motorcycle()
    {
        return View();
    }
    public IActionResult Component()
    {
        return View();
    }

}
