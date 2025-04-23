using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Base.ProductManagement;
public interface IImageProvider
{

    Task<string> UploadImageAsync(IFormFile file, string folderName, string fileName = null!);
    IList<string> AllowImageContentTypes { get; }
    Task<bool> RemoveImageAsync(string folderName, string fileName = null!);
    Task<bool> RemoveImageAsync(string url);
}
