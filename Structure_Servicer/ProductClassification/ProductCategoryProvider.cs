using Dapper;
using Microsoft.Data.SqlClient;
using Structure_Base.BaseService;
using Structure_Context;
using Structure_Core.BaseClass;
using Structure_Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Structure_Core.ProductClassification;
using Structure_Base.ProductClassification;

namespace Structure_Servicer.ProductClassification;
public class ProductCategoryProvider : ICRUD_Service<ProductCategory, int>
    , IProductCategoryProvider
{
    private readonly DB_Ecommerce_Rookie_Context _db;
    private readonly IConfiguration _configuration;
    private readonly string _dapperConnectionString;
    private const int TimeoutInSeconds = 240;

    public ProductCategoryProvider(DB_Ecommerce_Rookie_Context db, IConfiguration configuration)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dapperConnectionString = _configuration.GetConnectionString("DB_Ecommerce_Rookie_Dapper")!;

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

                _db.ProductCategories.Remove(obj.Data);
                if (_db.SaveChanges() <= 0)
                {
                    result.Message = "Failed to delete product category";
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

    public async Task<ResultService<ProductCategory>> Get(int id)
    {
        ResultService<ProductCategory> result = new();
        using (var sqlConnection = new SqlConnection(_dapperConnectionString))
        {
            try
            {
                await sqlConnection.OpenAsync();
                var rs = await sqlConnection.QuerySingleOrDefaultAsync<ProductCategory>(
                    "ProductCategory_GetByID",
                    new { ID = id },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                if (rs == null)
                {
                    result.Message = "Product category not found";
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

    public async Task<ResultService<IEnumerable<ProductCategory>>> GetAll()
    {
        ResultService<IEnumerable<ProductCategory>> result = new();
        try
        {
            using (var sqlConnection = new SqlConnection(_dapperConnectionString))
            {
                await sqlConnection.OpenAsync();
                var data = await sqlConnection.QueryAsync<ProductCategory>(
                    "ProductCategory_GetAll",
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                if (data == null || !data.Any())
                {
                    result.Data = Enumerable.Empty<ProductCategory>();
                    result.Message = "No product categories found";
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
    public async Task<ResultService<ProductCategory>> SaveByDapper(ProductCategory entity)
    {
        var response = new ResultService<ProductCategory>();

        if (entity == null)
        {
            return new ResultService<ProductCategory>()
            {
                Code = "1",
                Message = "Entity is not valid(BE)"
            };
        }
        try
        {
            string Message = string.Empty;
            entity.CategoryCode = !entity.CategoryCode.Contains("CAT") ? string.Empty : entity.CategoryCode;
            List<ProductCategory> lst = new List<ProductCategory>();
            lst.Add(entity);

            DataTable dtHeader = General.ConvertToDataTable(lst);
            using (var connection = new SqlConnection(_dapperConnectionString))
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();

                param.Add("@CreatedBy", entity.CreatedBy);
                param.Add("@udtt_ProductCategory", dtHeader.AsTableValuedParameter("UDTT_ProductCategory"));

                param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var result = await connection.QueryAsync<ProductCategory>("ProductCategory_Save",
                   param,
                   commandType: CommandType.StoredProcedure,
                      commandTimeout: TimeoutInSeconds);
                var resultMessage = param.Get<string>("@Message");

                if (resultMessage.Contains("successfully"))
                {
                    response.Code = "0"; // Success
                    response.Message = $"Save Successfully(BE) - {resultMessage}"; //Use the sql
                    response.Data = result.FirstOrDefault();
                    if (response.Data != null)
                    {
                        response.Data.CategoryCode = resultMessage;
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
            response.Message = $"Concurrency error or Conflict happened : {ex.GetType()} - {ex.Message}";
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

    public async Task<ResultService<ProductCategory>> GetByCode(string code)
    {
        ResultService<ProductCategory> result = new();
        using (var sqlconnect = new SqlConnection(_dapperConnectionString))
        {
            try
            {
                await sqlconnect.OpenAsync();
                var rs = await sqlconnect.QuerySingleOrDefaultAsync<ProductCategory>("ProductCategory_GetByCode",
                    new
                    {
                        CategoryCode = code
                    },
                     commandType: CommandType.StoredProcedure,
                     commandTimeout: 240);
                if (rs == null)
                {
                    result.Message = "Failed to get data";
                    result.Code = "1";
                }
                else
                {
                    result.Message = "Success";
                    result.Code = "0";
                }
                result.Data = rs;
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
                param.Add("@CategoryCode", code);
                param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                await connection.QueryAsync<ProductCategory>("ProductCategory_Delete",
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
                    resultService.Message = "Failed(BE)";
                    resultService.Message = resultMessage ?? "Failed(BE)"; // Sử dụng message từ stored procedure nếu có

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
