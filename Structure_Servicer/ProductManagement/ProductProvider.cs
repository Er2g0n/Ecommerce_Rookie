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

        public ProductProvider(DB_ProductManagement_Context db, IConfiguration configuration,IImageProvider image)
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
            ResultService<string> result = new();
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var obj = await Get(id);
                    if (!obj.Code.Equals("0"))
                    {
                        result.Message = obj.Message;
                        result.Code = obj.Code;
                        result.Data = "false";
                        return result;
                    }

                    _db.Products.Remove(obj.Data);
                    if (_db.SaveChanges() <= 0)
                    {
                        result.Message = "Failed to delete product";
                        result.Code = "-1";
                        result.Data = "false";
                        return result;
                    }

                    await transaction.CommitAsync();
                    result.Message = "Success";
                    result.Code = "0";
                    result.Data = "true";
                    return result;
                }
                catch (SqlException sqlEx)
                {
                    await transaction.RollbackAsync();
                    result.Code = "1";
                    result.Message = $"SQL error: {sqlEx.GetType()} - {sqlEx.Message}";
                    return result;
                }
                catch (ArgumentException ex)
                {
                    await transaction.RollbackAsync();
                    result.Code = "2";
                    result.Message = $"Configuration error: {ex.GetType()} - {ex.Message}";
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Message = $"Unexpected error: {ex.Message}";
                    result.Code = "999";
                    return result;
                }
            }
        }

        public async Task<ResultService<Product>> Get(int id)
        {
            ResultService<Product> result = new();
            using (var sqlConnection = new SqlConnection(_dapperConnectionString))
            {
                try
                {
                    await sqlConnection.OpenAsync();
                    var rs = await sqlConnection.QuerySingleOrDefaultAsync<Product>(
                        "Product_GetByID",
                        new { ID = id },
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: TimeoutInSeconds);

                    if (rs == null)
                    {
                        result.Message = "Product not found";
                        result.Code = "1";
                    }
                    else
                    {
                        result.Message = "Success";
                        result.Code = "0";
                        result.Data = rs;
                    }
                }
                catch (Exception ex)
                {
                    result.Message = $"Error: {ex.Message}";
                    result.Code = "999";
                }
                return result;
            }
        }

        public async Task<ResultService<IEnumerable<Product>>> GetAll()
        {
            ResultService<IEnumerable<Product>> result = new();
            try
            {
                using (var sqlConnection = new SqlConnection(_dapperConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    var data = await sqlConnection.QueryAsync<Product>(
                        "Product_GetAll",
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: TimeoutInSeconds);

                    if (data == null || !data.Any())
                    {
                        result.Data = Enumerable.Empty<Product>();
                        result.Message = "No products found";
                        result.Code = "1";
                    }
                    else
                    {
                        result.Data = data;
                        result.Message = "Success";
                        result.Code = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                result.Data = null;
                result.Message = $"Exception: {ex.Message}";
                result.Code = "999";
            }

            return result;
        }

        public async Task<ResultService<Product>> SaveByDapper(Product entity)
        {
            var response = new ResultService<Product>();

            if (entity == null)
            {
                return new ResultService<Product>()
                {
                    Code = "1",
                    Message = "Entity is not valid(BE)"
                };
            }
            try
            { 
                string Message = string.Empty;
                entity.ProductCode = !entity.ProductCode.Contains("PROD") ? string.Empty : entity.ProductCode;

                var lst = new List<Product> { entity }; //viết gọn

                DataTable dtHeader = General.ConvertToDataTable(lst);
                using (var connection = new SqlConnection(_dapperConnectionString))
                {
                    await connection.OpenAsync();
                    var param = new DynamicParameters();

                    param.Add("@CreatedBy", entity.CreatedBy);
                    param.Add("@udtt_Product", dtHeader.AsTableValuedParameter("UDTT_Product"));
         
                    param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    var result = await connection.QueryAsync<Product>(
                        "Product_Save",
                       param,
                       commandType: CommandType.StoredProcedure,
                       commandTimeout: TimeoutInSeconds
                       );
                    var resultMessage = param.Get<string>("@Message");

                    if (resultMessage.Contains("successfully"))
                    {
                        response.Code = "0"; // Success
                        response.Message = $"Save Successfully(BE) - {resultMessage}";
                        response.Data = result.FirstOrDefault();
                        if (response.Data == null)
                        {
                            response.Message += " (Warning: Could not retrieve saved data(BE))";
                        }

                    }
                    else
                    {
                        response.Code = "-999"; // Fail
                        response.Message = "Failed(BE)";
                    }

                    return response;
                }
            
            }
            catch (SqlException sqlex)
            {
                response.Code = "2";
                response.Message = $"Something wrong happened with Database, please Check the configuration: {sqlex.GetType()} - {sqlex.Message}";
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                response.Code = "3";
                response.Message = $"Concurrency error or Conflict happened: {ex.GetType()} - {ex.Message}";
                return response;
            }
            catch (DbUpdateException ex)
            {
                response.Code = "4";
                response.Message = $"Database update error: {ex.GetType()} - {ex.Message}";
                return response;
            }
            catch (OperationCanceledException ex)
            {
                response.Code = "5";
                response.Message = $"Operation canceled: {ex.GetType()} - {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Code = "6";
                response.Message = $"An unexpected error occurred: {ex.GetType()} - {ex.Message}";
                return response;
            }
        }

        public async Task<ResultService<Product>> GetByCode(string code)
        {
            ResultService<Product> result = new();
            using (var sqlconnect = new SqlConnection(_dapperConnectionString))
            {
                try
                {
                    await sqlconnect.OpenAsync();
                    var rs = await sqlconnect.QuerySingleOrDefaultAsync<Product>("Product_GetByCode",
                        new
                        {
                            ProductCode = code
                        },
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: TimeoutInSeconds);

                    if (rs == null)
                    {
                        result.Message = "Failed to get data";
                        result.Code = "1";
                    }
                    else
                    {
                        result.Message = "Success";
                        result.Code = "0";
                        result.Data = rs;
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                    result.Code = "999";
                    return result;
                }
            }
        }

        public async Task<ResultService<string>> DeleteByDapper(string code)
        {
            ResultService<string> resultService = new();
            try
            {
                string Message = string.Empty;
                using (var connection = new SqlConnection(_dapperConnectionString))
                {
                    await connection.OpenAsync();
                    var param = new DynamicParameters();
                    param.Add("@ProductCode", code);
                    param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    await connection.QueryAsync<Product>("Product_Delete",
                       param,
                       commandType: CommandType.StoredProcedure,
                       commandTimeout: TimeoutInSeconds);
                    var resultMessage = param.Get<string>("@Message");

                    if (resultMessage.Contains("OK"))
                    {
                        resultService.Code = "0"; // Success
                        resultService.Message = "Deleted Successfully(BE)";
                    }
                    else
                    {
                        resultService.Code = "-999";
                        resultService.Message = resultMessage ?? "Failed(BE)";
                    }

                    return resultService;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new ResultService<string>()
                {
                    Code = "2",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (DbUpdateException ex)
            {
                return new ResultService<string>()
                {
                    Code = "3",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ResultService<string>()
                {
                    Code = "4",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (SqlException ex)
            {
                return new ResultService<string>()
                {
                    Code = "5",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ResultService<string>()
                {
                    Code = "6",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
        }

        public async Task<ResultService<Product_ProductImage_Dto>> SaveProductAndImage(Product_ProductImage_Dto entity)
        {
            var response = new ResultService<Product_ProductImage_Dto>();
            if (entity == null)
            {
                return new ResultService<Product_ProductImage_Dto>()
                {
                    Code = "1",
                    Message = "Entity is not valid(BE)"
                };
            }
            try
            {
                string Message = string.Empty;
                entity.Products[0].ProductCode = !entity.Products[0].ProductCode.Contains("PROD") ? string.Empty : entity.Products[0].ProductCode;

                // Tạo DataTable cho Products
                DataTable dtHeader = General.ConvertToDataTable(entity.Products);

                // Tạo DataTable cho ProductImages
                DataTable dtDetail = General.ConvertToDataTable(entity.ProductImages ?? new List<ProductImage>());

                using (var connection = new SqlConnection(_dapperConnectionString))
                {
                    await connection.OpenAsync();
                    var param = new DynamicParameters();

                    param.Add("@CreatedBy", entity.CreatedBy ?? "system");
                    param.Add("@udtt_Product", dtHeader.AsTableValuedParameter("UDTT_Product"));
                    param.Add("@udtt_ProductImage", dtDetail.AsTableValuedParameter("UDTT_ProductImage"));
                    param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    using (var multi = await connection.QueryMultipleAsync(
                        "ProductAndImage_Save",
                        param,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: TimeoutInSeconds
                    ))
                    {
                        var savedProduct = await multi.ReadSingleOrDefaultAsync<Product>();
                        var images = await multi.ReadAsync<ProductImage>();

                        response.Code = "0"; // Success
                        response.Message = $"Save Successfully(BE) - {param.Get<string>("@Message")}";
                        response.Data = new Product_ProductImage_Dto
                        {
                            CreatedBy = entity.CreatedBy ?? "system",
                            Products = savedProduct != null ? new List<Product> { savedProduct } : new List<Product>(),
                            ProductImages = images?.ToList() ?? new List<ProductImage>()
                        };
                        if (response.Data.Products.Count == 0)
                        {
                            response.Message += " (Warning: Could not retrieve saved product data(BE))";
                        }
                    }
                }
            }
            catch (SqlException sqlex)
            {
                response.Code = "2";
                response.Message = $"Something wrong happened with Database, please Check the configuration: {sqlex.GetType()} - {sqlex.Message}";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                response.Code = "3";
                response.Message = $"Concurrency error or Conflict happened: {ex.GetType()} - {ex.Message}";
            }
            catch (DbUpdateException ex)
            {
                response.Code = "4";
                response.Message = $"Database update error: {ex.GetType()} - {ex.Message}";
            }
            catch (OperationCanceledException ex)
            {
                response.Code = "5";
                response.Message = $"Operation canceled: {ex.GetType()} - {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Code = "6";
                response.Message = $"An unexpected error occurred: {ex.GetType()} - {ex.Message}";
            }

            return response;
        }

        public async Task<ResultService<Product_ProductImage_Dto>> GetProductAndImageByCode(string proCode)
        {
            var response = new ResultService<Product_ProductImage_Dto>();
            using (var connection = new SqlConnection(_dapperConnectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Gọi Stored Procedure và lấy hai result sets
                    using (var multiResult = await connection.QueryMultipleAsync(
                        "ProductAndImages_GetByCode",
                        new { ProductCode = proCode },
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: TimeoutInSeconds))
                    {
                        // Lấy thông tin sản phẩm
                        var product = await multiResult.ReadSingleOrDefaultAsync<Product>();
                        if (product == null)
                        {
                            response.Code = "1";
                            response.Message = "Product not found(BE)";
                            return response;
                        }

                        // Lấy danh sách hình ảnh
                        var images = await multiResult.ReadAsync<ProductImage>();

                        // Tạo đối tượng trả về
                        response.Data = new Product_ProductImage_Dto
                        {
                            Products = new List<Product> { product },
                            ProductImages = images?.ToList() ?? new List<ProductImage>(),
                            CreatedBy = product.CreatedBy
                        };

                        response.Code = "0";
                        response.Message = "Retrieved Successfully(BE)";
                    }
                }
                catch (SqlException sqlex)
                {
                    response.Code = "2";
                    response.Message = $"Database error: {sqlex.GetType()} - {sqlex.Message}";
                }
                catch (Exception ex)
                {
                    response.Code = "999";
                    response.Message = $"An unexpected error occurred: {ex.GetType()} - {ex.Message}";
                }
            }

            return response;        }

        public async Task<ResultService<string>> Delete_ProductAndImage(List<ProductImage> entity)
        {
            ResultService<string> resultService = new();
            try
            {
                string Message = string.Empty;
                using (var connection = new SqlConnection(_dapperConnectionString))
                {
                    await connection.OpenAsync();
                    var param = new DynamicParameters();
                    param.Add("@udtt_ProductImage", General.ConvertToDataTable(entity).AsTableValuedParameter("UDTT_ProductImage"));
                    param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                    await connection.QueryAsync<Product_ProductImage_Dto>("ProductAndImage_Delete",
                       param,
                       commandType: CommandType.StoredProcedure,
                       commandTimeout: TimeoutInSeconds);
                    var resultMessage = param.Get<string>("@Message");
                    if (resultMessage.Contains("OK"))
                    {
                        resultService.Code = "0"; // Success
                        resultService.Message = "Deleted Successfully(BE)";
                    }
                    else
                    {
                        resultService.Code = "-999";
                        resultService.Message = resultMessage ?? "Failed(BE)";
                    }
                    return resultService;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return new ResultService<string>()
                {
                    Code = "2",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (DbUpdateException ex)
            {
                return new ResultService<string>()
                {
                    Code = "3",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ResultService<string>()
                {
                    Code = "4",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (SqlException ex)
            {
                return new ResultService<string>()
                {
                    Code = "5",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ResultService<string>()
                {
                    Code = "6",
                    Data = null,
                    Message = $"{ex.GetType()}, {ex.Message}"
                };
            }
        }
    }
}