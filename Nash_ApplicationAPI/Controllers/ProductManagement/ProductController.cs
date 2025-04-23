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

        [HttpPost("SaveByDapper")]
        //[Consumes("application/json")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> SaveByDapper([FromBody] ProductDto productDto) //productDto đã có một list bên product
        {
            Product_ProductImage_Dto entity = new();
            foreach (var item in productDto.Images)
            {
                var path = await _imageProvider.UploadImageAsync(item, item.FileName);
                if (path != null)
                {
                    entity.ProductImages.Add(new ProductImage
                    {
                        ImagePath = path,
                        CreatedBy = productDto.CreatedBy,
                        CreatedDate = DateTime.Now
                    });
                }
                else
                {
                    //Nếu ánh vữa upload failed -> Xóa tất cả ảnh vừa upload
                    foreach (var image in entity.ProductImages)
                    {
                        await _imageProvider.RemoveImageAsync(image.ImagePath);
                    }
                    return BadRequest("Upload image failed");
                }
            }
            var rs = await _productProvider.SaveByDapper(entity);
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
    }
}