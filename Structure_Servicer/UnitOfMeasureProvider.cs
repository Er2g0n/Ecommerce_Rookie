using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Structure_Base;
using Structure_Base.BaseService;
using Structure_Context;
using Structure_Core;
using Structure_Core.BaseClass;
using Structure_Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Servicer;
public class UnitOfMeasureProvider : ICRUD_Service<UnitOfMeasure, int>
    , IUnitOfMeasureProvider
{
    private readonly DB_Ecommerce_Rookie_Context _db;
    private readonly IConfiguration _configuration;
    private readonly string _dapperConnectionString;
    private const int TimeoutInSeconds = 240;
    public UnitOfMeasureProvider(DB_Ecommerce_Rookie_Context db, IConfiguration configuration)
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

                _db.UnitOfMeasures.Remove(obj.Data);
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

    public async Task<ResultService<UnitOfMeasure>> Get(int id)
    {
        ResultService<UnitOfMeasure> result = new();
        using (var sqlConnection = new SqlConnection(_dapperConnectionString))
        {
            try
            {
                await sqlConnection.OpenAsync();
                var rs = await sqlConnection.QuerySingleOrDefaultAsync<UnitOfMeasure>(
                    "UnitOfMeasure_GetByID",
                    new { ID = id },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                if (rs == null)
                {
                    result.Message = "Unit of measure not found";
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


    public async Task<ResultService<IEnumerable<UnitOfMeasure>>> GetAll()
    {
        ResultService<IEnumerable<UnitOfMeasure>> result = new();
        try
        {
            using (var sqlConnection = new SqlConnection(_dapperConnectionString))
            {
                await sqlConnection.OpenAsync();
                var data = await sqlConnection.QueryAsync<UnitOfMeasure>(
                    "UnitOfMeasure_GetAll",
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds);

                result.Data = data ?? Enumerable.Empty<UnitOfMeasure>();
                result.Message = "Success";
                result.Code = "0";
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


    public async Task<ResultService<UnitOfMeasure>> SaveByDapper(UnitOfMeasure entity)
    {
        var response = new ResultService<UnitOfMeasure>();

        if (entity == null)
        {
            return new ResultService<UnitOfMeasure>()
            {
                Code = "1",
                Message = "Entity is not valid(BE)"
            };
        }

        try
        {
            string Message = string.Empty;
            entity.UoMCode = !entity.UoMCode.Contains("UOM") ? string.Empty : entity.UoMCode;
            List<UnitOfMeasure> lst = new() { entity };

            DataTable dtHeader = General.ConvertToDataTable(lst);
            using (var connection = new SqlConnection(_dapperConnectionString))
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();

                param.Add("@CreatedBy", entity.CreatedBy);
                param.Add("@udtt_UnitOfMeasure", dtHeader.AsTableValuedParameter("UDTT_UnitOfMeasure"));
                param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var result = await connection.QueryAsync<UnitOfMeasure>(
                   "UnitOfMeasure_Save",
                   param,
                   commandType: CommandType.StoredProcedure,
                   commandTimeout: TimeoutInSeconds);

                var resultMessage = param.Get<string>("@Message");

                if (resultMessage.Contains("successfully"))
                {
                    response.Code = "0";
                    response.Message = $"Save Successfully(BE) - {resultMessage}";
                    response.Data = result.FirstOrDefault();
                    if (response.Data == null)
                    {
                        response.Message += " (Warning: Could not retrieve saved data(BE))";
                    }
                }
                else
                {
                    response.Code = "-999";
                    response.Message = resultMessage ?? "Failed(BE)";
                }

                return response;
            }
        }
        catch (Exception ex)
        {
            response.Code = "6";
            response.Message = $"Unexpected error: {ex.GetType()} - {ex.Message}";
            return response;
        }
    }


    public async Task<ResultService<UnitOfMeasure>> GetByCode(string code)
    {
        ResultService<UnitOfMeasure> result = new();
        using (var sqlconnect = new SqlConnection(_dapperConnectionString))
        {
            try
            {
                await sqlconnect.OpenAsync();
                var rs = await sqlconnect.QuerySingleOrDefaultAsync<UnitOfMeasure>(
                    "UnitOfMeasure_GetByCode",
                    new { UoMCode = code },
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
                param.Add("@UoMCode", code);
                param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                await connection.QueryAsync<UnitOfMeasure>("UnitOfMeasure_DeleteByCode",
                   param,
                   commandType: CommandType.StoredProcedure,
                   commandTimeout: TimeoutInSeconds);

                var resultMessage = param.Get<string>("@Message");

                if (resultMessage.Contains("OK", StringComparison.OrdinalIgnoreCase))
                {
                    resultService.Code = "0";
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
