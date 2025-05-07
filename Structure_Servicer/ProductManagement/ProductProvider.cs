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
        public async Task<ResultService<IEnumerable<ProductImage>>> GetImagesByProductCode(string code)
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<IEnumerable<ProductImage>>();
            try
            {
                await connection.OpenAsync();
                var productImages = await connection.QueryAsync<ProductImage>(
                    "ProductImages_GetByProductCode",
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
        public async Task<ResultService<string>> DeleteProductAndImageByProductCode(string productCode)
        {
            var result = new ResultService<string>();
            if (string.IsNullOrEmpty(productCode))
            {
                result.Code = "1";
                result.Message = "ProductCode is required";
                return result;
            }

            try
            {
                // Step 1: Fetch the images associated with the product
                var imageResult = await GetImagesByProductCode(productCode);
                if (imageResult.Code != "0")
                {
                    result.Code = "1";
                    result.Message = "Failed to fetch product images: " + imageResult.Message;
                    return result;
                }

                var productImages = imageResult.Data?.ToList() ?? new List<ProductImage>();

                // Step 2: Delete images from Cloudinary
                foreach (var image in productImages)
                {
                    if (!string.IsNullOrEmpty(image.ImagePath))
                    {
                        await _imageProvider.RemoveImageAsync(image.ImagePath);
                    }
                }

                // Step 3: Delete product and images from the database
                using var connection = new SqlConnection(_dapperConnectionString);
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@ProductCode", productCode);
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



        //Get All Products With First Image
        public async Task<ResultService<List<ProductsWithFirstImageDto>>> GetAllProductsWithFirstImage()
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<List<ProductsWithFirstImageDto>>();

            try
            {
                await connection.OpenAsync();
                var productsWithImages = await connection.QueryAsync<dynamic>(
                    "ProductsWithFirstImage_GetAll",
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds
                );

                var resultList = new List<ProductsWithFirstImageDto>();

                foreach (var item in productsWithImages)
                {
                    var product = new ProductsWithFirstImageDto
                    {
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        CategoryCode = item.CategoryCode,
                        BrandCode = item.BrandCode,
                        Description = item.Description,
                        FirstImagePath = item.FirstImagePath,
                        SalePrice = item.SalePrice

                    };
                    string firstImagePath = item.FirstImagePath ?? "~/images/default-motorcycle.jpg";
                    resultList.Add(product);
                }

                result.Code = resultList.Any() ? "0" : "1";
                result.Message = resultList.Any() ? "Success" : "No products found";
                result.Data = resultList;
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error retrieving products: {ex.Message}";
                result.Data = null;
            }

            return result;
        }
        //Get Product With All Images
        public async Task<ResultService<ProductWithAllImagesDto>> GetProductWithAllImagesByCode(string productCode)
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<ProductWithAllImagesDto>();
            try
            {
                await connection.OpenAsync();
                using var multi = await connection.QueryMultipleAsync(
                    "ProductWithAllImages_GetByCode",
                    new { ProductCode = productCode },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                var productInfo = await multi.ReadSingleOrDefaultAsync<dynamic>();
                var images = await multi.ReadAsync<ProductImageDto>();

                if (productInfo == null)
                {
                    result.Code = "1";
                    result.Message = "Product not found";
                    return result;
                }

                var product = new ProductWithAllImagesDto
                {
                    ProductCode = productInfo.ProductCode,
                    ProductName = productInfo.ProductName,
                    BrandCode = productInfo.BrandCode,
                    BrandName = productInfo.BrandName,
                    Description = productInfo.Description,
                    LatestPrice = productInfo.LatestPrice,
                    FirstImagePath = productInfo.FirstImagePath ?? "~/images/default-motorcycle.jpg",
                    Images = images?.ToList() ?? new List<ProductImageDto>()
                };


                result.Code = "0";
                result.Message = "Success";
                result.Data = product;
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error: {ex.Message}";
            }
            return result;
        }



        public async Task<ResultService<List<Product>>> GetAllProductsByCategoryCode(string categoryCode)
        {
            if (string.IsNullOrEmpty(categoryCode))
            {
                return new ResultService<List<Product>>
                {
                    Code = "1",
                    Message = "CategoryCode is required",
                    Data = null
                };
            }

            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<List<Product>>();

            try
            {
                await connection.OpenAsync();
                var products = await connection.QueryAsync<Product>(
                    "Product_GetByCategoryCode",
                    new { CategoryCode = categoryCode },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds
                );

                var productList = products?.ToList() ?? new List<Product>();
                result.Code = productList.Any() ? "0" : "1";
                result.Message = productList.Any() ? "Success" : "No products found for this category";
                result.Data = productList;
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error retrieving products: {ex.Message}";
                result.Data = null;
            }

            return result;
        }
        public async Task<ResultService<List<(Product Product, string FirstImagePath)>>> GetProductsWithFirstImageByCategoryCode(string categoryCode)
        {
            if (string.IsNullOrEmpty(categoryCode))
            {
                return new ResultService<List<(Product Product, string FirstImagePath)>>
                {
                    Code = "1",
                    Message = "CategoryCode is required",
                    Data = null
                };
            }

            using var connection = new SqlConnection(_dapperConnectionString);
            var result = new ResultService<List<(Product Product, string FirstImagePath)>>();

            try
            {
                await connection.OpenAsync();
                var productsWithImages = await connection.QueryAsync<dynamic>(
                    "ProductImages_GetByCategoryCode",
                    new { CategoryCode = categoryCode },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds
                );

                var resultList = new List<(Product Product, string FirstImagePath)>();
                foreach (var item in productsWithImages)
                {
                    var product = new Product
                    {
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        CategoryCode = item.CategoryCode,
                        BrandCode = item.BrandCode,
                        UoMCode = item.UoMCode,
                        Description = item.Description,
                        ID = item.ProductID,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = item.CreatedDate,
                        UpdatedBy = item.UpdatedBy,
                        UpdatedDate = item.UpdatedDate
                    };
                    string firstImagePath = item.FirstImagePath ?? "~/images/default-motorcycle.jpg";
                    resultList.Add((product, firstImagePath));
                }

                result.Code = resultList.Any() ? "0" : "1";
                result.Message = resultList.Any() ? "Success" : "No products found for this category";
                result.Data = resultList;
            }
            catch (Exception ex)
            {
                result.Code = "999";
                result.Message = $"Error retrieving products: {ex.Message}";
                result.Data = null;
            }

            return result;
        }


    
    }
}