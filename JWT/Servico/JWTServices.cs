using JWT.Models;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWT.Servico;

public class JWTServices : IJWTServices
{
    private readonly IConfiguration _configuration;
    private readonly IDatabase _redis;
    public JWTServices(IConfiguration configuration, IConnectionMultiplexer redis)
    {
        _configuration = configuration;
        _redis = redis.GetDatabase();
    }

    public async Task<TokenResponse> GetToken(LoginRequest login)
    {
        TokenResponse result = new TokenResponse
        {
            AccessToken = await BuildToken(login, "new"),
            RefreshToken = Guid.NewGuid().ToString() //await BuildToken(login, "refresh")
        };
        await RevokeRefreshTokenAsync(login.Username);
        await SaveRefreshTokenAsync(login, result.RefreshToken);
        return result;
    }
    public async Task<string?> GetRefreshToken(RefreshTokenRequest refreshReq)
    {
        string? tokenRecuperado = await _redis.StringGetAsync($"Token:{refreshReq.UserId }");
        if (string.IsNullOrWhiteSpace(tokenRecuperado))
        {
            throw new SecurityTokenArgumentException();
        }
        return tokenRecuperado;
    }

    private async Task<string> BuildToken(LoginRequest login, string @type)
    {
        JwtSecurityToken token = await GenerateToken(login, @type);
        string result = new JwtSecurityTokenHandler().WriteToken(token);//TOKEN serializado
        return result;
    }
    private async Task<JwtSecurityToken> GenerateToken(LoginRequest login, string @type)
    {
        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, login.Username),//Representa o SUBJECT do token, geralmente adota-se o nome do usuario.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())//Identificador unico para o TOKEN gerado
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));//Chave para assinar o TOKEN
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);//Define as credenciais da assinatura

        DateTime expireTime = @type switch
        {
            "refresh" => DateTime.UtcNow.AddDays(7),
            "new" => DateTime.UtcNow.AddMinutes(5),
            _ => throw new ArgumentException("Invalid token type", nameof(type))
        };

        return new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"],//Emissor
                                        audience: _configuration["Jwt:Audience"],//Quem pode utilizar o TOKEN
                                        claims: @type == "new" 
                                            ? claims
                                            : null ,//Dados que estao sendo incluidos no TOKEN
                                        expires: expireTime,//Data de expiracao a partir da criacao do TOKEN
                                        signingCredentials: creds//Credenciais utilizadas para assinar o TOKEN
                                      );
    }
    private Task SaveRefreshTokenAsync(LoginRequest login, string token)
    {
        _redis.StringSet($"Token:{login.Username}", token);
        return Task.CompletedTask;
    }
    private async Task RevokeRefreshTokenAsync(string username)
    {
        await _redis.KeyDeleteAsync(username);
    }

}
