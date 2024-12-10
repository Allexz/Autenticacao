using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWT.Models;
using JWT.Servico;
using System.Collections.Concurrent;
using StackExchange.Redis;

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
    public IActionResult Login([FromBody] Models.LoginRequest login)
    {
        // Validação estática para simplificação (substitua por autenticação real)
        if (login.Username != "usuario" || login.Password != "senha")
        {
            return Unauthorized("Usuário ou senha inválidos");
        }

        (string, string) token = _jWTServices.GetToken(login);


        return Ok(token);
    }
}

