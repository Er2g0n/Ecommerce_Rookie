using Microsoft.AspNetCore.Mvc;
using Nash_AuthAPI.Models.Dto;
using Nash_AuthAPI.Service.IService;


namespace Nash_AuthAPI.Controllers;
    [Route("api/auth")]
    [ApiController]
public class AuthAPIController : Controller
{
    private readonly IAuthService _authService;
    //private readonly IMessageBus _messageBus;
    //private readonly IConfiguration _configuration;
    protected ResponseDto _response;
    public AuthAPIController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        //_configuration = configuration;
        //_messageBus = messageBus;
        _response = new();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
    {

        var errorMessage = await _authService.Register(model);
        if (!string.IsNullOrEmpty(errorMessage))
        {
            _response.IsSuccess = false;
            _response.Message = errorMessage;
            return BadRequest(_response);
        }
        //await _messageBus.PublishMessage(model.Email, _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"));
        return Ok(_response);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
    {
        var loginResponse = await _authService.Login(model);
        if (loginResponse.User == null)
        {
            _response.IsSuccess = false;
            _response.Message = "Username or password is incorrect";
            return BadRequest(_response);
        }
        _response.Result = loginResponse;
        return Ok(_response);
    }
}
