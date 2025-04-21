using Microsoft.AspNetCore.Mvc;
using Structure_Base.BaseService;
using Structure_Base.ProductClassification;
using Structure_Core.ProductClassification;

namespace Nash_ApplicationAPI.Controllers.ProductClassification;

[Route("api/[controller]")]
[ApiController]
public class ProductCategoryController : ControllerBase
{
    private readonly ICRUD_Service<ProductCategory, int> _ICRUD_Service;
    private readonly IProductCategoryProvider _productCategoryProvider;

    public ProductCategoryController(ICRUD_Service<ProductCategory, int> iCRUD_Service, IProductCategoryProvider productCategoryProvider)
    {
        _productCategoryProvider = productCategoryProvider;
        _ICRUD_Service = iCRUD_Service;

    }

    [HttpGet]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAll()
    {
        var rs = await _ICRUD_Service.GetAll();
        return rs == null ? BadRequest("No product categories found") : Ok(rs);
    }

    [HttpGet("{id}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetById(int id)
    {
        var rs = await _ICRUD_Service.Get(id);
        return rs == null ? NotFound($"Product category with ID {id} not found") : Ok(rs);
    }

    [HttpDelete]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> Delete(int id)
    {
        var rs = await _ICRUD_Service.Delete(id);
        return rs == null ? BadRequest("Failed to delete product category") : Ok(rs);
    }



    [HttpGet("categoryCode/{categoryCode}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetByCode(string categoryCode)
    {
        var rs = await _productCategoryProvider.GetByCode(categoryCode);
        return rs.Code == "0" ? Ok(rs) : NotFound($"Category with Code {categoryCode} not found");
    }

    [HttpPost("SaveByDapper")]
    [Consumes("application/json")]
    [Produces("application/json")]

    public async Task<IActionResult> SaveByDapper([FromBody] ProductCategory productCategory)
    {
        var rs = await _productCategoryProvider.SaveByDapper(productCategory);
        return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
    }
    [HttpDelete("DeleteByDapper")]
    [Consumes("application/json")]
    [Produces("application/json")]

    public async Task<IActionResult> DeleteByDapper(string categoryCode)
    {
        var rs = await _productCategoryProvider.DeleteByDapper(categoryCode);
        return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
    }


}
