using Nash_WebMVC.Models;
using Nash_WebMVC.Service.IService;
using Newtonsoft.Json;
using Structure_Core.BaseClass;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static Nash_WebMVC.Utility.SD;

namespace Nash_WebMVC.Service;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenProvider _tokenProvider;
    //public BaseService(IHttpClientFactory httpClientFactory)
    //{
    //    _httpClientFactory = httpClientFactory;
    //}
    public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenProvider = tokenProvider;
    }
    public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
    {
        HttpClient client = _httpClientFactory.CreateClient("NashAPI");
        HttpRequestMessage message = new();
        message.Headers.Add("Accept", "application/json");


        //token
        if(withBearer)
        {
            var token = _tokenProvider.GetToken();
            message.Headers.Add("Authorization", $"Bearer {token}");
        }    


        message.RequestUri = new Uri(requestDto.Url);
        if (requestDto.Data != null)
        {
            message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
        }
            HttpResponseMessage? apiResponse = null;
        switch (requestDto.ApiType)
        {
            case ApiType.POST:
                message.Method = HttpMethod.Post;
                break;
            case ApiType.DELETE:
                message.Method = HttpMethod.Delete;
                break;
            case ApiType.PUT:
                message.Method = HttpMethod.Put;
                break;
            default:
                message.Method = HttpMethod.Get;
                break;
        }

        apiResponse = await client.SendAsync(message);

        switch (apiResponse.StatusCode)
        {
            case HttpStatusCode.NotFound:
                return new() { IsSuccess = false, Message = "Not Found" };
            case HttpStatusCode.Forbidden:
                return new() { IsSuccess = false, Message = "Access Denied" };
            case HttpStatusCode.Unauthorized:
                return new() { IsSuccess = false, Message = "Unauthorized" };
            case HttpStatusCode.InternalServerError:
                return new() { IsSuccess = false, Message = "Internal Server Error" };
            default:
                var apiContent = await apiResponse.Content.ReadAsStringAsync();
                var apiResponseDto = System.Text.Json.JsonSerializer.Deserialize<ResponseDto>(apiContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return apiResponseDto;
        }
    }
    //Lay ben duoi Infrastructure
    public async Task<ResultService<T>> SendAsync<T>(RequestDto requestDto, bool withBearer = true)
    {
        HttpClient client = _httpClientFactory.CreateClient("NashAPI");
        HttpRequestMessage message = new();
        message.Headers.Add("Accept", "application/json");


        //token
        if (withBearer)
        {
            var token = _tokenProvider.GetToken();
            message.Headers.Add("Authorization", $"Bearer {token}");
        }


        message.RequestUri = new Uri(requestDto.Url);
        if (requestDto.Data != null)
        {
            message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
        }
        HttpResponseMessage? apiResult = null;
        switch (requestDto.ApiType)
        {
            case ApiType.POST:
                message.Method = HttpMethod.Post;
                break;
            case ApiType.DELETE:
                message.Method = HttpMethod.Delete;
                break;
            case ApiType.PUT:
                message.Method = HttpMethod.Put;
                break;
            default:
                message.Method = HttpMethod.Get;
                break;
        }

        apiResult = await client.SendAsync(message);

        switch (apiResult.StatusCode)
        {
            case HttpStatusCode.NotFound:
                return new() { Code = "3", Message = "Not Found" };
            case HttpStatusCode.Forbidden:
                return new() { Code = "4", Message = "Access Denied" };
            case HttpStatusCode.Unauthorized:
                return new() { Code = "5", Message = "Unauthorized" };
            case HttpStatusCode.InternalServerError:
                return new() { Code = "6", Message = "Internal Server Error" };
            default:
                var apiContent = await apiResult.Content.ReadAsStringAsync();
                var apiResultDto = System.Text.Json.JsonSerializer.Deserialize<ResultService<T>>(apiContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (apiResultDto == null)
                {
                    return new ResultService<T>
                    {
                        Code = "1",
                        Message = "No data found",
                        Data = default!
                    };
                }
                else
                {
                    return apiResultDto;
                }
        }
    }
}
