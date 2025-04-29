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

        //[HttpPost("SaveProductAndImage")]
        //[Consumes("multipart/form-data")]
        //[Produces("application/json")]
        //public async Task<IActionResult> SaveProductAndImage([FromForm] ProductDto productDto)
        //{
        //    if (productDto == null || string.IsNullOrWhiteSpace(productDto.ProductName))
        //    {
        //        return BadRequest("Invalid product data or ProductName is required");
        //    }

        //    if (productDto.Images == null || !productDto.Images.Any())
        //    {
        //        return BadRequest("At least one image is required");
        //    }

        //    if (productDto.IsPrimary.HasValue &&
        //        (productDto.IsPrimary < 0 || productDto.IsPrimary >= productDto.Images.Count))
        //    {
        //        return BadRequest("Invalid PrimaryImageIndex(BE)");
        //    }

        //    var entity = new Product_ProductImage_Dto
        //    {
        //        CreatedBy = "system", // Sau này thay bằng user thực tế
        //        Products = new List<Product>
        //{
        //    new Product
        //    {
        //        ProductCode = productDto.ProductCode ?? Guid.NewGuid().ToString("N"),
        //        ProductName = productDto.ProductName,
        //        Description = productDto.Description,
        //        CategoryCode = productDto.CategoryCode,
        //        BrandCode = productDto.BrandCode,
        //        UoMCode = productDto.UoMCode
        //    }
        //},
        //        ProductImages = new List<ProductImage>()
        //    };

        //    var uploadedImagePaths = new List<string>();

        //    try
        //    {
        //        for (int i = 0; i < productDto.Images.Count; i++)
        //        {
        //            var image = productDto.Images[i];
        //            var isPrimary = i == (productDto.IsPrimary ?? 0);

        //            var path = await _imageProvider.UploadImageAsync(
        //                image,
        //                folderName: "products",
        //                fileName: $"{entity.Products[0].ProductCode}_{Guid.NewGuid()}"
        //            );

        //            uploadedImagePaths.Add(path);

        //            entity.ProductImages.Add(new ProductImage
        //            {
        //                RefProductCode = entity.Products[0].ProductCode,
        //                Position = i,
        //                ImagePath = path,
        //                IsPrimary = isPrimary
        //            });
        //        }

        //        // Upload xong hết mới Save database
        //        var rs = await _productProvider.SaveProductAndImage(entity);

        //        if (rs.Code != "0")
        //        {
        //            // Lưu thất bại → rollback hình ảnh
        //            foreach (var path in uploadedImagePaths)
        //            {
        //                await _imageProvider.RemoveImageAsync(path);
        //            }
        //            return BadRequest(rs.Message);
        //        }

        //        return Ok(rs);
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            if (entity.Products?.Any() == true)
        //            {
        //                await _productProvider.DeleteByDapper(entity.Products[0].ProductCode);
        //            }

        //            foreach (var path in uploadedImagePaths)
        //            {
        //                await _imageProvider.RemoveImageAsync(path);
        //            }
        //        }
        //        catch
        //        {
        //            // Không throw exception phụ khi rollback
        //        }

        //        return BadRequest($"Failed to upload images(BE): {ex.Message}");
        //    }
        //}


        [HttpDelete("Delete_ProductAndImage")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> Delete_ProductAndImage([FromBody] List<ProductImage> productImages)
        {
            if (productImages == null || !productImages.Any())
            {
                return BadRequest("Invalid product images data");
            }
            var rs = await _productProvider.Delete_ProductAndImage(productImages);
            return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
        }
    }
}