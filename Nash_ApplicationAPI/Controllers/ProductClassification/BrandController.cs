using Microsoft.AspNetCore.Mvc;
using Structure_Base.BaseService;
using Structure_Core.ProductClassification;
using Structure_Base.ProductClassification;

namespace Nash_ApplicationAPI.Controllers.ProductClassification;

[Route("api/[controller]")]
[ApiController]
public class BrandController : ControllerBase
{
    private readonly ICRUD_Service<Brand, int> _ICRUD_Service;
    private readonly IBrandProvider _brandProvider;

    public BrandController(ICRUD_Service<Brand, int> iCRUD_Service, IBrandProvider brandProvider)
    {
        _brandProvider = brandProvider;
        _ICRUD_Service = iCRUD_Service;
    }
    #region Normal CRUD

    [HttpGet]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAll()
    {
        var rs = await _ICRUD_Service.GetAll();

        return rs == null ? BadRequest("No brands found") : Ok(rs);
    }

    [HttpGet("{id}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetById(int id)
    {
        var rs = await _ICRUD_Service.Get(id);
        return rs == null ? NotFound($"Brand with ID {id} not found") : Ok(rs);
    }

    [HttpDelete]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> Delete(int id)
    {
        var rs = await _ICRUD_Service.Delete(id);
        return rs == null ? BadRequest("Failed to delete brand") : Ok(rs);
    }
    #endregion

    #region Dapper CRUD

    [HttpGet("brandCode/{brandCode}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetByCode(string brandCode)
    {
        var rs = await _brandProvider.GetByCode(brandCode);
        return rs.Code == "0" ? Ok(rs) : NotFound($"Brand with Code {brandCode} not found");
    }

    [HttpPost("SaveByDapper")]
    [Consumes("application/json")]
    [Produces("application/json")]

    public async Task<IActionResult> SaveByDapper([FromBody] Brand brand)
    {
        var rs = await _brandProvider.SaveByDapper(brand);

        return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
    }
    [HttpDelete("DeleteByDapper")]
    [Consumes("application/json")]
    [Produces("application/json")]

    public async Task<IActionResult> DeleteByDapper(string brandCode)
    {
        var rs = await _brandProvider.DeleteByDapper(brandCode);
        return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
    }


    #endregion
}
