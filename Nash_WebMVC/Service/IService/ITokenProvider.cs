namespace Nash_WebMVC.Service.IService;

public interface ITokenProvider
{

    void SetToken(string token);
    string? GetToken();
    void ClearToken();
}
