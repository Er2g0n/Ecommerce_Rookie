using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Structure_Base.BaseService;
using Structure_Base.ProductManagement;
using Structure_Core.ProductManagement;
using Structure_Servicer.ProductManagement;

namespace Nash_ApplicationAPI.Controllers.ProductManagement
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]

    public class ProductController : ControllerBase
    {
        private readonly ICRUD_Service<Product, int> _ICRUD_Service;
        private readonly IProductProvider _productProvider;
        private readonly IImageProvider _imageProvider;

        public ProductController(ICRUD_Service<Product, int> iCRUD_Service, IProductProvider productProvider, IImageProvider imageProvider)
        {
            _ICRUD_Service = iCRUD_Service;
            _productProvider = productProvider;
            _imageProvider = imageProvider;
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
        
        
        [HttpGet("code/{productCode}/images")]
        public async Task<IActionResult> GetImagesByProductCode(string productCode)
        {
            var rs = await _productProvider.GetImagesByProductCode(productCode);
            return rs.Code == "0" ? Ok(rs) : NotFound(rs.Message);
        }
        
        [HttpPost("SaveByDapper")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> SaveByDapper([FromBody] Product product)
        {

            var rs = await _productProvider.SaveByDapper(product);
            return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
        }

        [HttpDelete("DeleteByDapper")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteByDapper(string productCode)
        {
            var rs = await _productProvider.DeleteByDapper(productCode);
            return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
        }


        [HttpDelete("DeleteProductAndImage/{productCode}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteProductAndImage(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                return BadRequest("ProductCode is required");
            }
            var rs = await _productProvider.DeleteProductAndImageByProductCode(productCode);
            return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
        }




        //----------------------For Client------------------
  

        [HttpGet("allWithFirstImage")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllProductsWithFirstImage()
        {
            var rs = await _productProvider.GetAllProductsWithFirstImage();
            return rs.Code == "0" ? Ok(rs) : NotFound(rs.Message);
        }

        [HttpGet("code/{productCode}/details")]
        public async Task<IActionResult> GetProductWithAllImagesByCode(string productCode)
        {
            var rs = await _productProvider.GetProductWithAllImagesByCode(productCode);
            return rs.Code == "0" ? Ok(rs) : NotFound(rs.Message);
        }

        [HttpGet("byCategory/{categoryCode}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllProductsByCategoryCode(string categoryCode)
        {
            if (string.IsNullOrEmpty(categoryCode))
            {
                return BadRequest("CategoryCode is required");
            }

            var rs = await _productProvider.GetAllProductsByCategoryCode(categoryCode);
            return rs.Code == "0" ? Ok(rs) : NotFound(rs.Message);
        }

        [HttpGet("byCategoryWithFirstImage/{categoryCode}")]
        public async Task<IActionResult> GetProductsWithFirstImageByCategoryCode(string categoryCode)
        {
            if (string.IsNullOrEmpty(categoryCode))
            {
                return BadRequest("CategoryCode is required");
            }

            var rs = await _productProvider.GetProductsWithFirstImageByCategoryCode(categoryCode);
            return rs.Code == "0" ? Ok(rs) : NotFound(rs.Message);
        }



        //----------------------------------------------------------


    }
}