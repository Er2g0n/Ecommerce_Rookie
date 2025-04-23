using Microsoft.AspNetCore.Http;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Structure_Base.ImageService;
using CloudinaryDotNet;
namespace Structure_Servicer.Image;
public class ImageProvider : IImageProvider

{
    private readonly Cloudinary _cloudinary;

    public IList<string> AllowImageContentTypes { get; } = new List<string>
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    public ImageProvider(IConfiguration configuration)
    {
        var cloudinarySettings = configuration.GetSection("Cloudinary");

        var account = new Account(
            cloudinarySettings["CloudName"],
            cloudinarySettings["ApiKey"],
            cloudinarySettings["ApiSecret"]
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folderName, string fileName = null!)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        if (!AllowImageContentTypes.Contains(file.ContentType))
            throw new ArgumentException($"File type {file.ContentType} is not supported.");

        // If no filename specified, generate one
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = Guid.NewGuid().ToString();
        }

        // Prepare upload parameters
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, file.OpenReadStream()),
            Folder = folderName,
            PublicId = fileName,
            Overwrite = true
        };

        try
        {
            // Upload to Cloudinary
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Failed to upload image to Cloudinary: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred when uploading the image: {ex.Message}", ex);
        }
    }

    public async Task<bool> RemoveImageAsync(string folderName, string fileName)
    {
        if (string.IsNullOrEmpty(folderName) || string.IsNullOrEmpty(fileName))
            throw new ArgumentException("Folder name and file name must be provided.");

        // Format public ID with folder
        string publicId = $"{folderName}/{fileName}";

        return await DeleteImageByPublicIdAsync(publicId);
    }

    public async Task<bool> RemoveImageAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL must be provided.", nameof(url));

        // Extract public ID from Cloudinary URL
        // Example URL: https://res.cloudinary.com/{cloud_name}/image/upload/v{version}/{public_id}
        try
        {
            var uri = new Uri(url);
            var pathSegments = uri.AbsolutePath.Split('/');

            // Skip the first segments that include /image/upload/v{version}/ to get to public ID
            // Find the index of the upload part
            int uploadIndex = Array.IndexOf(pathSegments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= pathSegments.Length)
            {
                throw new ArgumentException("URL does not match Cloudinary format.");
            }

            // Reconstruct the public ID by joining all segments after the version segment
            var publicIdParts = pathSegments.Skip(uploadIndex + 2);
            string publicId = string.Join("/", publicIdParts);

            return await DeleteImageByPublicIdAsync(publicId);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new Exception($"Failed to parse Cloudinary URL: {ex.Message}", ex);
        }
    }

    private async Task<bool> DeleteImageByPublicIdAsync(string publicId)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred when deleting the image: {ex.Message}", ex);
        }
    }
}