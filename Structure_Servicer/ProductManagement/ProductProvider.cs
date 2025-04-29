using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Structure_Base.BaseService;
using Structure_Base.ProductManagement;
using Structure_Context.ProductManagement;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;
using Structure_Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static System.Net.Mime.MediaTypeNames;

namespace Structure_Servicer.ProductManagement
{
    public class ProductProvider : ICRUD_Service<Product, int>, IProductProvider
    {
        private readonly DB_ProductManagement_Context _db;
        private readonly IImageProvider _imageProvider;
        private readonly IConfiguration _configuration;
        private readonly string _dapperConnectionString;
        private const int TimeoutInSeconds = 240;

        public ProductProvider(DB_ProductManagement_Context db, IConfiguration configuration, IImageProvider image)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dapperConnectionString = _configuration.GetConnectionString("DB_Ecommerce_Rookie_Dapper")!;
            _imageProvider = image;

            // Kiểm tra chuỗi kết nối
            if (string.IsNullOrEmpty(_dapperConnectionString))
            {
                throw new InvalidOperationException("Connection string 'DB_Ecommerce_Rookie_Dapper' not found in appsettings.json.");
            }
        }
        public async Task<ResultService<string>> Delete(int id)
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<string>();
            try
            {
                await connection.OpenAsync();
                var product = await connection.QuerySingleOrDefaultAsync<Product>(
                    "Product_GetByID", new { ID = id }, commandType: CommandType.StoredProcedure, commandTimeout: TimeoutInSeconds);

                if (product == null)
                {
                    result.Code = "1";
                    result.Message = "Product not found";
                    return result;
                }

                var param = new DynamicParameters();
                param.Add("@ProductCode", product.ProductCode);
                param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                await connection.ExecuteAsync(
                    "ProductAndImage_Delete",
                    param,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                var message = param.Get<string>("@Message");
                result.Code = message == "OK(SQL)" ? "0" : "-999";
                result.Message = message == "OK(SQL)" ? "Deleted successfully" : message;
                result.Data = "true";
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<ResultService<Product>> Get(int id)
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<Product>();
            try
            {
                await connection.OpenAsync();
                var product = await connection.QuerySingleOrDefaultAsync<Product>(
                    "Product_GetByID", new { ID = id }, commandType: CommandType.StoredProcedure, commandTimeout: TimeoutInSeconds);

                result.Code = product == null ? "1" : "0";
                result.Message = product == null ? "Product not found" : "Success";
                result.Data = product;
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<ResultService<IEnumerable<Product>>> GetAll()
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<IEnumerable<Product>>();
            try
            {
                await connection.OpenAsync();
                var products = await connection.QueryAsync<Product>(
                    "Product_GetAll", commandType: CommandType.StoredProcedure, commandTimeout: TimeoutInSeconds);

                result.Data = products ?? Enumerable.Empty<Product>();
                result.Code = products.Any() ? "0" : "1";
                result.Message = products.Any() ? "Success" : "No products found";
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<ResultService<Product>> SaveByDapper(Product entity)
        {
            var result = new ResultService<Product>();
            if (entity == null)
            {
                result.Code = "1";
                result.Message = "Invalid product data";
                return result;
            }

            try
            {
                entity.ProductCode = string.IsNullOrEmpty(entity.ProductCode) || !entity.ProductCode.StartsWith("PROD") ? "" : entity.ProductCode;
                var products = new List<Product> { entity };
                var dtHeader = General.ConvertToDataTable(products);

                using var connection = new SqlConnection(_dapperConnectionString);
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@CreatedBy", entity.CreatedBy ?? "system");
                param.Add("@udtt_Product", dtHeader.AsTableValuedParameter("UDTT_Product"));
                param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var savedProduct = await connection.QuerySingleOrDefaultAsync<Product>(
                    "Product_Save",
                    param,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                var message = param.Get<string>("@Message");
                result.Code = message.Contains("successfully") ? "0" : "-999";
                result.Message = message.Contains("successfully") ? "Saved successfully" : message;
                result.Data = savedProduct;
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<ResultService<Product>> GetByCode(string code)
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<Product>();
            try
            {
                await connection.OpenAsync();
                var product = await connection.QuerySingleOrDefaultAsync<Product>(
                    "Product_GetByCode",
                    new { ProductCode = code },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                result.Code = product == null ? "1" : "0";
                result.Message = product == null ? "Product not found" : "Success";
                result.Data = product;
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<ResultService<string>> DeleteByDapper(string code)
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<string>();
            try
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@ProductCode", code);
                param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                await connection.ExecuteAsync(
                    "ProductAndImage_Delete",
                    param,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                var message = param.Get<string>("@Message");
                result.Code = message == "OK(SQL)" ? "0" : "-999";
                result.Message = message == "OK(SQL)" ? "Deleted successfully" : message;
                result.Data = "true";
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<ResultService<Product_ProductImage_Dto>> SaveProductAndImage(Product_ProductImage_Dto entity)
        {
            var result = new ResultService<Product_ProductImage_Dto>();
            if (entity == null || entity.Products == null || !entity.Products.Any() || entity.ProductImages == null)
            {
                result.Code = "1";
                result.Message = "Invalid product or image data";
                return result;
            }

            if (!entity.ProductImages.Any())
            {
                result.Code = "1";
                result.Message = "At least one image is required";
                return result;
            }

            if (entity.ProductImages.Count(pi => (bool)pi.IsPrimary) != 1)
            {
                result.Code = "1";
                result.Message = "Exactly one image must be marked as primary";
                return result;
            }

            try
            {
                var product = entity.Products[0];
                product.ProductCode = string.IsNullOrEmpty(product.ProductCode) || !product.ProductCode.StartsWith("PROD") ? "" : product.ProductCode;
                var dtHeader = General.ConvertToDataTable(entity.Products);
                var dtDetail = General.ConvertToDataTable(entity.ProductImages);

                using var connection = new SqlConnection(_dapperConnectionString);
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add ("@CreatedBy", entity.CreatedBy ?? "system");
                param.Add("@udtt_Product", dtHeader.AsTableValuedParameter("UDTT_Product"));
                param.Add("@udtt_ProductImage", dtDetail.AsTableValuedParameter("UDTT_ProductImage"));
                param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                using var multi = await connection.QueryMultipleAsync(
                    "ProductAndImage_Save",
                    param,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                var savedProduct = await multi.ReadSingleOrDefaultAsync<Product>();
                var images = await multi.ReadAsync<ProductImage>();

                var message = param.Get<string>("@Message");
                result.Code = message.Contains("successfully") ? "0" : "-999";
                result.Message = message.Contains("successfully") ? "Saved successfully" : message;
                result.Data = new Product_ProductImage_Dto
                {
                    CreatedBy = entity.CreatedBy ?? "system",
                    Products = savedProduct != null ? new List<Product> { savedProduct } : new List<Product>(),
                    ProductImages = images?.ToList() ?? new List<ProductImage>()
                };
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        //public async Task<ResultService<Product_ProductImage_Dto>> GetProductAndImageByCode(string proCode)
        //{
        //    using var connection = new SqlConnection(_dapperConnectionString);
        //    var result = new ResultService<Product_ProductImage_Dto>();
        //    try
        //    {
        //        await connection.OpenAsync();
        //        using var multi = await connection.QueryMultipleAsync(
        //            "ProductAndImages_GetByCode",
        //            new { ProductCode = proCode },
        //            commandType: CommandType.StoredProcedure,
        //            commandTimeout: TimeoutInSeconds);

        //        var product = await multi.ReadSingleOrDefaultAsync<Product>();
        //        var images = await multi.ReadAsync<ProductImage>();

        //        if (product == null)
        //        {
        //            result.Code = "1";
        //            result.Message = "Product not found";
        //            return result;
        //        }

        //        result.Code = "0";
        //        result.Message = "Retrieved successfully";
        //        result.Data = new Product_ProductImage_Dto
        //        {
        //            Products = new List<Product> { product },
        //            ProductImages = images?.ToList() ?? new List<ProductImage>(),
        //            CreatedBy = product.CreatedBy
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Code = "999";
        //        result.Message = $"Error: {ex.Message}";
        //    }
        //    return result;
        //}

        public async Task<ResultService<string>> Delete_ProductAndImage(List<ProductImage> productImages)
        {
            var result = new ResultService<string>();
            if (productImages == null || !productImages.Any())
            {
                result.Code = "1";
                result.Message = "Invalid image data";
                return result;
            }

            try
            {
                var dtImages = General.ConvertToDataTable(productImages);
                using var connection = new SqlConnection(_dapperConnectionString);
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@udtt_ProductImage", dtImages.AsTableValuedParameter("UDTT_ProductImage"));
                param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                await connection.ExecuteAsync(
                    "ProductAndImage_Delete",
                    param,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                var message = param.Get<string>("@Message");
                result.Code = message == "OK(SQL)" ? "0" : "-999";
                result.Message = message == "OK(SQL)" ? "Deleted successfully" : message;
                result.Data = "true";

                // Delete images from Cloudinary
                foreach (var image in productImages)
                {
                    if (!string.IsNullOrEmpty(image.ImagePath))
                    {
                        await _imageProvider.RemoveImageAsync(image.ImagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

        public async Task<ResultService<IEnumerable<ProductImage>>> GetImagesByProductCode(string code)
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<IEnumerable<ProductImage>>();
            try
            {
                await connection.OpenAsync();
                var productImages = await connection.QueryAsync<ProductImage>(
                    "ProductImages_GetByCode",
                    new { RefProductCode = code },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                result.Data = productImages ?? Enumerable.Empty<ProductImage>();
                result.Code = productImages.Any() ? "0" : "1";
                result.Message = productImages.Any() ? "Success" : "Product Images not found";
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }

    }
}