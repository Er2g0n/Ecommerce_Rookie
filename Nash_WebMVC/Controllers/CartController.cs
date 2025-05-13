using Microsoft.AspNetCore.Mvc;
using Nash_WebMVC.Service.IService;
using Nash_WebMVC.Utility;
using Structure_Core.OrderManagement;

namespace Nash_WebMVC.Controllers;

public class CartController : Controller
{
    private readonly IProductService _productService;

    public CartController(IProductService productService)
    {
        _productService = productService;

    }


    public async Task<IActionResult> AddToCart(string productCode, int quantity)
    {
        // Clear TempData to avoid lingering messages
        TempData.Remove("error");
        TempData.Remove("success");

        if (string.IsNullOrEmpty(productCode))
        {
            TempData["error"] = "Product code is required.";
            return RedirectToAction("Detail", "Product", new { code = productCode });
        }

        if (quantity <= 0)
        {
            TempData["error"] = "Quantity must be greater than zero.";
            return RedirectToAction("Detail", "Product", new { code = productCode });
        }

        var response = await _productService.GetProductWithAllImagesByCodeAsync(productCode);
        if (response == null || response.Code != "0" || response.Data == null)
        {
            TempData["error"] = "Product not found.";
            return RedirectToAction("Detail", "Product", new { code = productCode });
        }

        var product = response.Data;

        var cartItem = new OrderDetailDTO
        {
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            BrandCode = product.BrandCode,
            BrandName = product.BrandName,
            ImagePath = product.FirstImagePath,
            Price = product.LatestPrice ?? 0,
            Quantity = quantity
        };

        CartHelper.AddToCart(HttpContext.Session, cartItem);
        TempData["success"] = $"{product.ProductName} added to cart successfully!";

        // Render the Detail view directly
        ViewData["Title"] = product.ProductName;
        return View("~/Views/Product/Detail.cshtml", product);
    }

    public IActionResult Index()
    {
        var cart = CartHelper.GetCart(HttpContext.Session);
        return View(cart);
    }

    public IActionResult Checkout()
    {
        var cart = CartHelper.GetCart(HttpContext.Session);
        if (!cart.Any())
        {
            TempData["error"] = "Your cart is empty.";
            return RedirectToAction("Cart", "Cart"); // Fixed redirect to self
        }

            // TODO: Process the order (save to database, etc.)
            CartHelper.ClearCart(HttpContext.Session);
        TempData["success"] = "Order placed successfully!";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult GetCartPartial()
    {
        var cart = CartHelper.GetCart(HttpContext.Session);
        return PartialView("_CartPartial", cart);
    }
}
