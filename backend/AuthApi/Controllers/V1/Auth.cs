using Microsoft.AspNetCore.Mvc;
using AuthApi.DTOs;
namespace AuthApi.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Verifica se a API está saudável.
    /// </summary>
    /// <returns>Status da aplicação</returns>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy" });

    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        //TODO
        var response = new LoginResponse(
            Token: "jwt",
            Role: "User");

        return Ok(response);
    }


}