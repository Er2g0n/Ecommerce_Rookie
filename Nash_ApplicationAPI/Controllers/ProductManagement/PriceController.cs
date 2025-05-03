using Microsoft.AspNetCore.Mvc;
using Structure_Base.ProductManagement;
using Structure_Core.ProductManagement;
using System.Threading.Tasks;

namespace Nash_ApplicationAPI.Controllers.ProductManagement
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]

    public class PriceController : ControllerBase
    {
        private readonly IPriceProvider _priceProvider;

        public PriceController(IPriceProvider priceProvider)
        {
            _priceProvider = priceProvider;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SavePrice([FromBody] Price price)
        {
            var result = await _priceProvider.Save(price);
            if (result.Code == "0")
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("GetByProductCode/{productCode}")]
        public async Task<IActionResult> GetPriceByProductCode(string productCode)
        {
            var result = await _priceProvider.GetByProductCode(productCode);
            if (result.Code == "0")
                return Ok(result);
            return NotFound(result);
        }

        [HttpDelete("delete/{priceCode}")]
        public async Task<IActionResult> DeletePrice(string priceCode)
        {
            var result = await _priceProvider.Delete(priceCode);
            if (result.Code == "0")
                return Ok(result);
            return BadRequest(result);
        }
    }
}