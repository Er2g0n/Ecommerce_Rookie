using Microsoft.AspNetCore.Mvc;
using Structure_Base.BaseService;
using Structure_Base;
using Structure_Core;

namespace Nash_ApplicationAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UnitOfMeasureController : ControllerBase
{
    private readonly ICRUD_Service<UnitOfMeasure, int> _ICRUD_Service;
    private readonly IUnitOfMeasureProvider _unitOfMeasureProvider;

    public UnitOfMeasureController(ICRUD_Service<UnitOfMeasure, int> iCRUD_Service, IUnitOfMeasureProvider unitOfMeasureProvider)
    {
        _ICRUD_Service = iCRUD_Service;
        _unitOfMeasureProvider = unitOfMeasureProvider;
    }

    [HttpGet]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAll()
    {
        var rs = await _ICRUD_Service.GetAll();
        return rs == null ? BadRequest("No unit of measures found") : Ok(rs);
    }

    [HttpGet("{id}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetById(int id)
    {
        var rs = await _ICRUD_Service.Get(id);
        return rs == null ? NotFound($"Unit of measure with ID {id} not found") : Ok(rs);
    }

    [HttpDelete]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> Delete(int id)
    {
        var rs = await _ICRUD_Service.Delete(id);
        return rs == null ? BadRequest("Failed to delete unit of measure") : Ok(rs);
    }

    [HttpGet("code/{uomCode}")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> GetByCode(string uomCode)
    {
        var rs = await _unitOfMeasureProvider.GetByCode(uomCode);
        return rs.Code == "0" ? Ok(rs) : NotFound($"Unit of measure with Code {uomCode} not found");
    }

    [HttpPost("SaveByDapper")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> SaveByDapper([FromBody] UnitOfMeasure unitOfMeasure)
    {
        var rs = await _unitOfMeasureProvider.SaveByDapper(unitOfMeasure);
        return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
    }

    [HttpDelete("DeleteByDapper")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteByDapper(string uomCode)
    {
        var rs = await _unitOfMeasureProvider.DeleteByDapper(uomCode);
        return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
    }

    
}
