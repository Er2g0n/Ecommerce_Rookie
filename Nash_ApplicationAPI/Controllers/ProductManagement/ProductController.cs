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
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetProductAndImagesByCode(string productCode)
        {
            var rs = await _productProvider.GetProductAndImageByCode(productCode);
            return rs.Code == "0" ? Ok(rs) : NotFound($"Product with Code {productCode} not found");
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

        [HttpPost("SaveProductAndImage")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<IActionResult> SaveProductAndImage([FromForm] ProductDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest("Invalid product data(BE)");
            }
            // Kiểm tra các trường bắt buộc tối thiểu
            if (string.IsNullOrEmpty(productDto.ProductName))
            {
                return BadRequest("ProductName is required(BE)");
            }
            var entity = new Product_ProductImage_Dto
            {
                Products = new List<Product>
                {
                    new Product
                    {
                        ProductCode = productDto.ProductCode ?? string.Empty,
                        ProductName = productDto.ProductName,
                        Description = productDto.Description,
                        CategoryCode = productDto.CategoryCode,
                        BrandCode = productDto.BrandCode,
                        UoMCode = productDto.UoMCode,
                        CreatedDate = null, // Sẽ được SQL xử lý
                        UpdatedDate = null  // Sẽ được SQL xử lý
                    }
                },
                ProductImages = new List<ProductImage>()
            };
            // Upload hình ảnh lên Cloudinary nếu có
            if (productDto.Images != null && productDto.Images.Any())
            {
                for (int i = 0; i < productDto.Images.Count; i++)
                {
                    var image = productDto.Images[i];
                    var isPrimary = i == (productDto.PrimaryImageIndex ?? 0);

                    try
                    {
                        var path = await _imageProvider.UploadImageAsync(
                            image,
                            folderName: "products",
                            fileName: $"{entity.Products[0].ProductCode ?? "TEMP"}_{Guid.NewGuid()}"
                        );

                        entity.ProductImages.Add(new ProductImage
                        {
                            RefProductCode = entity.Products[0].ProductCode ?? string.Empty,
                            ImagePath = path,
                            IsPrimary = isPrimary,
                            CreatedDate = null, // Sẽ được SQL xử lý
                            UpdatedDate = null  // Sẽ được SQL xử lý
                        });

                        if (isPrimary)
                        {
                            entity.Products[0].ProductImageUrl = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        foreach (var uploadedImage in entity.ProductImages)
                        {
                            await _imageProvider.RemoveImageAsync(uploadedImage.ImagePath);
                        }
                        return BadRequest($"Failed to upload image(BE): {ex.Message}");
                    }
                }
            }

            var rs = await _productProvider.SaveProductAndImage(entity);
            return rs.Code == "0" ? Ok(rs) : BadRequest(rs);
        }
    
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