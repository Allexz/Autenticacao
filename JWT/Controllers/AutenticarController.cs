using JWT.Models;
using JWT.Servico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;

namespace JWT.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AutenticarController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IJWTServices _jWTServices;
    private static readonly ConcurrentDictionary<string, RefreshTokenEntry> _refreshTokens = new();
    private readonly HttpClient _httpClient;
    
    public AutenticarController(IConfiguration configuration,
                                IJWTServices jWTServices,
                                HttpClient httpClient)
    {
        _configuration = configuration;
        _jWTServices = jWTServices;
        _httpClient = httpClient;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Models.LoginRequest login)
    {
        // Validação estática para simplificação (substitua por autenticação real)
        try
        {
            if (login.Username != "string" || login.Password != "string")
            {
                return Unauthorized("Usuário ou senha inválidos");
            }

            TokenResponse response = await _jWTServices.GetToken(login);

            return Ok(response);
        }
        catch (SecurityTokenExpiredException)
        {
            return Unauthorized("Token expirado");
        }
        catch (SecurityTokenException)
        {
            return Unauthorized("Token inválido");
        }
    }

    [HttpPost("refreshtoken")]
    public async Task<IActionResult> GetRefreshToken([FromBody] Models.RefreshTokenRequest refreshTokenRequest)
    {
        try
        {
            string? response = await _jWTServices.GetRefreshToken(refreshTokenRequest);

            return Ok(response);
        }
        catch (SecurityTokenExpiredException)
        {
            return Unauthorized("Token expirado");
        }
        catch (SecurityTokenException)
        {
            return Unauthorized("Token inválido");
        }
    }
}

