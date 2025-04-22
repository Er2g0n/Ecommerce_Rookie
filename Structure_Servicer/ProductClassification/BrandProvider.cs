
using Castle.Components.DictionaryAdapter.Xml;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Structure_Base.BaseService;
using Structure_Base.ProductClassification;
using Structure_Context;
using Structure_Core.BaseClass;
using Structure_Core.ProductClassification;
using Structure_Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace Structure_Servicer.ProductClassification;
public class BrandProvider : ICRUD_Service<Brand, int>, IBrandProvider
{
    private readonly DB_Ecommerce_Rookie_Context _dB;
    private readonly IConfiguration _configuration;
    private readonly string _dapperConnectionString;
    private const int TimeoutInSeconds = 240;

    public BrandProvider(DB_Ecommerce_Rookie_Context dB, IConfiguration configuration)
    {
        _dB = dB;
        _configuration = configuration;
        _dapperConnectionString = _configuration.GetConnectionString("DB_Ecommerce_Rookie_Dapper")!;
    }
    public async Task<ResultService<string>> Delete(int id)
    {
        ResultService<string> result = new();
        using (var transaction = _dB.Database.BeginTransaction())
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
                _dB.Brands.Remove(obj.Data);
                if (_dB.SaveChanges() <= 0)
                {
                    result.Message = "Failed to delete data";
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
                result.Message = $"{sqlEx.GetType()} - {sqlEx.Message}";
                return result;
            }
            catch (ArgumentException ex)
            {
                await transaction.RollbackAsync();
                result.Code = "2";
                result.Message = $"An error occurred while trying to connect to your database Server, pls check your Configuration .Details: {ex.GetType()} - {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Message = ex.Message;
                result.Code = "999";
                return result;
            }

        }
    }

    public async Task<ResultService<Brand>> Get(int id)
    {
        ResultService<Brand> result = new();
        using (var sqlconnect = new SqlConnection(_dapperConnectionString))
        {
            try
            {
                await sqlconnect.OpenAsync();
                var rs = await sqlconnect.QuerySingleOrDefaultAsync<Brand>("Brand_GetByID",
                    new
                    {
                        ID = id
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

    public async Task<ResultService<IEnumerable<Brand>>> GetAll()
    {
        ResultService<IEnumerable<Brand>> result = new();

        try
        {
            using (var sqlconnect = new SqlConnection(_dapperConnectionString))
            {
                await sqlconnect.OpenAsync();

                var data = await sqlconnect.QueryAsync<Brand>(
                    "Brand_GetAll",
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: TimeoutInSeconds
                );

                if (data == null || !data.Any())
                {
                    result.Data = Enumerable.Empty<Brand>();
                    result.Message = "No data found";
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

    public async Task<ResultService<Brand>> SaveByDapper(Brand entity)
    {
        var response = new ResultService<Brand>();

        if (entity == null)
        {
            return new ResultService<Brand>()
            {
                Code = "1",
                Message = "Entity is not valid(BE)"
            };
        }
        try
        {
            string Message = string.Empty;
            entity.BrandCode = !entity.BrandCode.Contains("BR") ? string.Empty : entity.BrandCode;
            List<Brand> lst = new List<Brand>();
            lst.Add(entity);

            DataTable dtHeader = General.ConvertToDataTable(lst);
            using (var connection = new SqlConnection(_dapperConnectionString))
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();

                param.Add("@CreatedBy", entity.CreatedBy);
                param.Add("@udtt_Brand", dtHeader.AsTableValuedParameter("UDTT_Brand"));
                param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                var result = await connection.QueryAsync<Brand>(
                    "Brand_Save",
                   param,
                   commandType: CommandType.StoredProcedure,
                   commandTimeout: TimeoutInSeconds
                   );
                var resultMessage = param.Get<string>("@Message");

                if (resultMessage.Contains("successfully"))
                {
                    response.Code = "0"; // Success
                    response.Message = $"Save Successfully(BE) - {resultMessage}"; //Use the sql message
                    // Lấy lại Brand từ database để trả về
                    //Backend cần trả về dữ liệu Brand hoàn chỉnh trong ResultService.Data sau khi lưu.
                    //Frontend sẽ lấy response.data thay vì phải tự tạo đối tượng từ data gửi lên.
                    response.Data = result.FirstOrDefault(); // Lấy bản ghi đầu tiên (nếu có)
                    if (response.Data == null)
                    {
                        response.Message += " (Warning: Could not retrieve saved data(BE))";
                    }
                }
                else
                {
                    response.Code = "-999"; // Fail
                    response.Message = "Failed((BE))";
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

    public async Task<ResultService<Brand>> GetByCode(string code)
    {
        ResultService<Brand> result = new();
        using (var sqlconnect = new SqlConnection(_dapperConnectionString))
        {
            try
            {
                await sqlconnect.OpenAsync();
                var rs = await sqlconnect.QuerySingleOrDefaultAsync<Brand>("Brand_GetByCode",
                    new
                    {
                        BrandCode = code
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
                param.Add("@BrandCode", code);
                param.Add("@Message", Message, dbType: DbType.String, direction: ParameterDirection.Output, size: 500);
                await connection.QueryAsync<Brand>("Brand_Delete",
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
