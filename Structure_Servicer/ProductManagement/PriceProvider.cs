using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Structure_Base.BaseService;
using Structure_Base.ProductManagement;
using Structure_Core.BaseClass;
using Structure_Core.ProductManagement;
using Structure_Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Servicer.ProductManagement;
public class PriceProvider : ICRUD_Service<Price, int>, IPriceProvider
{
    private readonly IConfiguration _configuration;
    private readonly string _dapperConnectionString;
    private const int TimeoutInSeconds = 240;

    public PriceProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _dapperConnectionString = _configuration.GetConnectionString("DB_Ecommerce_Rookie_Dapper")!;

        if (string.IsNullOrEmpty(_dapperConnectionString))
        {
            throw new InvalidOperationException("Connection string 'DB_Ecommerce_Rookie_Dapper' not found.");
        }
    }
    public Task<ResultService<string>> Delete(int id)
    {
        throw new NotImplementedException();
    }
    public Task<ResultService<Price>> Get(int id)
    {
        throw new NotImplementedException();
    }
    public Task<ResultService<IEnumerable<Price>>> GetAll()
    {
        throw new NotImplementedException();
    }
    public async Task<ResultService<Price>> Save(Price price)
    {
        var result = new ResultService<Price>();
        if (price == null || string.IsNullOrEmpty(price.ProductCode))
        {
            result.Code = "1";
            result.Message = "Invalid price or product code";
            return result;
        }

        try
        {
            Console.WriteLine($"Saving price for ProductCode: {price.ProductCode}");
            Console.WriteLine($"Price data: {Newtonsoft.Json.JsonConvert.SerializeObject(price)}");

            price.PriceCode = string.IsNullOrEmpty(price.PriceCode) || !price.PriceCode.StartsWith("PRICE") ? "" : price.PriceCode;
            var prices = new List<Price> { price };
            var dtPrice = General.ConvertToDataTable(prices);

            using var connection = new SqlConnection(_dapperConnectionString);
            await connection.OpenAsync();
            var param = new DynamicParameters();
            param.Add("@CreatedBy", price.CreatedBy ?? "system");
            param.Add("@udtt_Price", dtPrice.AsTableValuedParameter("UDTT_Price"));
            param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            var savedPrice = await connection.QuerySingleOrDefaultAsync<Price>(
                "Price_Save",
                param,
                commandType: CommandType.StoredProcedure,
                commandTimeout: TimeoutInSeconds);

            var message = param.Get<string>("@Message");
            Console.WriteLine($"Price_Save result: Code={result.Code}, Message={message}");
            result.Code = message.Contains("successfully") ? "0" : "-999";
            result.Message = message.Contains("successfully") ? "Saved successfully" : message;
            result.Data = savedPrice;
        }
        catch (Exception ex)
        {
            result.Code = "999";
            result.Message = $"Error: {ex.Message}";
            Console.WriteLine($"Error saving price: {ex.Message}");
        }
        return result;
    }

    public async Task<ResultService<Price>> GetByProductCode(string productCode)
    {
        var result = new ResultService<Price>();
        if (string.IsNullOrEmpty(productCode))
        {
            result.Code = "1";
            result.Message = "Product code is required";
            return result;
        }

        try
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            await connection.OpenAsync();
            var price = await connection.QuerySingleOrDefaultAsync<Price>(
                "Price_GetByProductCode",
                new { ProductCode = productCode },
                commandType: CommandType.StoredProcedure,
                commandTimeout: TimeoutInSeconds);

            result.Code = price == null ? "1" : "0";
            result.Message = price == null ? "Price not found" : "Success";
            result.Data = price;
        }
        catch (Exception ex)
        {
            result.Code = "999";
            result.Message = $"Error: {ex.Message}";
        }
        return result;
    }

    public async Task<ResultService<string>> Delete(string priceCode)
    {
        var result = new ResultService<string>();
        if (string.IsNullOrEmpty(priceCode))
        {
            result.Code = "1";
            result.Message = "Price code is required";
            return result;
        }

        try
        {
            using var connection = new SqlConnection(_dapperConnectionString);
            await connection.OpenAsync();
            var param = new DynamicParameters();
            param.Add("@PriceCode", priceCode);
            param.Add("@Message", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

            await connection.ExecuteAsync(
                "Price_Delete",
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



 


}