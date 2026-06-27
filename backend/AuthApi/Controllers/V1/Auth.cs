using System.IdentityModel.Tokens.Jwt;
using Application.Services;
using AuthApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Verifica se a API está saudável.
    /// </summary>
    /// <returns>Status da aplicação</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "healthy" });

    /// <summary>
    /// Realiza o login do usuário e retorna um token JWT.
    /// </summary>
    /// <param name="request">Credenciais do usuário</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Token JWT e role do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        try
        {
            var token = await _authService.LoginAsync(request.Username, request.Password, ct);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var role = jwt.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "User";

            return Ok(new LoginResponse(Token: token, Role: role));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Credenciais inválidas." });
        }
    }
}