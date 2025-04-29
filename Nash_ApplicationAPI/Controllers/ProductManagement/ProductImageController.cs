using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Structure_Base.BaseService;
using Structure_Base.ProductManagement;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;
using Structure_Servicer.ProductManagement;
using System.Drawing;
using static Dapper.SqlMapper;

namespace Nash_ApplicationAPI.Controllers.ProductManagement;
[Route("api/[controller]")]
[ApiController]
public class ProductImageController : ControllerBase
{

    private readonly IImageProvider _imageProvider;
    private readonly IProductImageProvider _ProductImageProvider;

    public ProductImageController(IImageProvider imageProvider, IProductImageProvider productImageProvider)
    {
        _imageProvider = imageProvider;
        _ProductImageProvider = productImageProvider;
    }

    //[HttpPost("SaveImage")]
    //[Consumes("multipart/form-data")]
    //public async Task<IActionResult> SaveImage([FromForm] ImageDto imageDto)
    //{
    //    if (imageDto == null || imageDto.File == null || imageDto.Properties == null)
    //    {
    //        return BadRequest("Invalid image data or file");
    //    }

    //    try
    //    {
    //        var imagePath = await _imageProvider.UploadImageAsync(
    //            imageDto.File,
    //            folderName: "products",
    //            fileName: $"{imageDto.Properties.RefProductCode ?? "TEMP"}_{Guid.NewGuid()}"
    //        );

    //        imageDto.Properties.ImagePath = imagePath;
    //        var rs = new ResultService<ProductImage>
    //        {
    //            Code = "0",
    //            Message = "Image uploaded successfully",
    //            Data = imageDto.Properties
    //        };
    //        return Ok(rs);
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new ResultService<ProductImage>
    //        {
    //            Code = "999",
    //            Message = $"Failed to upload image: {ex.Message}"
    //        });
    //    }
    //}

    [HttpDelete("DeleteImage")]
    public async Task<IActionResult> DeleteImage([FromBody] ProductImage image)
    {
        if (image == null || string.IsNullOrEmpty(image.ImagePath))
        {
            return BadRequest("Invalid image data");
        }

        try
        {
            var deleted = await _imageProvider.RemoveImageAsync(image.ImagePath);
            var rs = new ResultService<string>
            {
                Code = deleted ? "0" : "1",
                Message = deleted ? "Image deleted successfully" : "Failed to delete image",
                Data = deleted ? "true" : "false"
            };
            return Ok(rs);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResultService<string>
            {
                Code = "999",
                Message = $"Failed to delete image: {ex.Message}"
            });
        }
    }


    [HttpPost("SaveImage")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SaveImage([FromForm] List<IFormFile> formFiles, [FromForm] string text)
    {
        try
        {
            var result = new List<ProductImage>();
            for (int i = 0; i < formFiles.Count; i++)
            {
                var image = formFiles[i];
                var uploadedImagePaths = new List<string>();

                var path = await _imageProvider.UploadImageAsync(
                    image,
                    folderName: "products",
                    fileName: $"{text}_{Guid.NewGuid()}"
                );

                uploadedImagePaths.Add(path);
                result.Add(await _ProductImageProvider.Save(new ProductImage
                {
                    RefProductCode = text,
                    Position = i,
                    ImagePath = path,
                    IsPrimary = false
                }));
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(null);

        }
    } 
}

