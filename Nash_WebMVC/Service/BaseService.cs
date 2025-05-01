using Nash_WebMVC.Models;
using Nash_WebMVC.Service.IService;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Nash_WebMVC.Utility.SD;

namespace Nash_WebMVC.Service;

public class BaseService : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BaseService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
    {
        HttpClient client = _httpClientFactory.CreateClient("NashAPI");
        HttpRequestMessage message = new();
        message.Headers.Add("Accept", "application/json");

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
                var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                return apiResponseDto;
        }
    }
}
