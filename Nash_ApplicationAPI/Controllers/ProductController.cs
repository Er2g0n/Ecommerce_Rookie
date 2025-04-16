using Microsoft.AspNetCore.Mvc;
using Structure_Base;
using Structure_Base.BaseService;
using Structure_Core;

namespace Nash_ApplicationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ICRUD_Service<Product, int> _ICRUD_Service;
        private readonly IProductProvider _productProvider;

        public ProductController(ICRUD_Service<Product, int> iCRUD_Service, IProductProvider productProvider)
        {
            _ICRUD_Service = iCRUD_Service;
            _productProvider = productProvider;
        }

        [HttpGet]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll()
        {
            var rs = await _ICRUD_Service.GetAll();
            return rs == null ? BadRequest("No products found") : Ok(rs);
        }

        [HttpGet("{id}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetById(int id)
        {
            var rs = await _ICRUD_Service.Get(id);
            return rs == null ? NotFound($"Product with ID {id} not found") : Ok(rs);
        }

        [HttpDelete]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(int id)
        {
            var rs = await _ICRUD_Service.Delete(id);
            return rs == null ? BadRequest("Failed to delete product") : Ok(rs);
        }

        [HttpGet("code/{productCode}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetByCode(string productCode)
        {
            var rs = await _productProvider.GetByCode(productCode);
            return rs.Code == "0" ? Ok(rs) : NotFound($"Product with Code {productCode} not found");
        }

        [HttpPost("SaveByDapper")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> SaveByDapper([FromBody] Product product)
        {
            var rs = await _productProvider.SaveByDapper(product);
            return rs.Code == "0" ? Ok(rs.Message) : BadRequest(rs.Message);
        }

        [HttpDelete("DeleteByDapper")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteByDapper(string productCode)
        {
            var rs = await _productProvider.DeleteByDapper(productCode);
            return rs.Code == "0" ? Ok(rs.Message) : BadRequest(rs.Message);
        }
    }
}