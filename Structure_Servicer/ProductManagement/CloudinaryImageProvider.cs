using Microsoft.AspNetCore.Http;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using Structure_Base.ProductManagement;
namespace Structure_Servicer.ProductManagement;

public class CloudinaryImageProvider : IImageProvider

{
    private readonly Cloudinary _cloudinary;

    public IList<string> AllowImageContentTypes { get; } = new List<string>
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp"
    };

    public CloudinaryImageProvider(IConfiguration configuration)
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

        fileName ??= Guid.NewGuid().ToString();

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
            throw new ArgumentException("URL must be provided.");

        try
        {
            var uri = new Uri(url);
            var pathSegments = uri.AbsolutePath.Split('/');
            int uploadIndex = Array.IndexOf(pathSegments, "upload");
            if (uploadIndex == -1 || uploadIndex + 2 >= pathSegments.Length)
                throw new ArgumentException("Invalid Cloudinary URL format.");

            var publicId = string.Join("/", pathSegments.Skip(uploadIndex + 2));
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting image: {ex.Message}", ex);
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