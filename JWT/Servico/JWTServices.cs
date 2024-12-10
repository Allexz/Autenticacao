using JWT.Models;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private string BuildToken(LoginRequest login)
    {
        Claim[] claims = new[]
        {
            //Representa o SUBJECT do token, geralmente adota-se o nome do usuario.
            new Claim(JwtRegisteredClaimNames.Sub, login.Username),
            //Identificador unico para o TOKEN gerado
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        //Chave para assinar o TOKEN, e recuperada da configuracao e convertida em bytes para uso na criptografia simetrica
        SymmetricSecurityKey? key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
        //Define as credenciais da assinatura, neste caso o algoritmo HMAC-SHA256 para proteger o TOKEN
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            //Emissor
            issuer: _configuration["Jwt:Issuer"],
            //Quem pode utilizar o TOKEN
            audience: _configuration["Jwt:Audience"],
            //Dados que estao sendo incluidos no TOKEN
            claims: claims,
            //Data de expiracao a partir da criacao do TOKEN
            expires: DateTime.UtcNow.AddMinutes(30),
            //Credenciais utilizadas para assinar o TOKEN
            signingCredentials: creds);
        //TOKEN e serializado para retornar em forma de STRING para o cliente
        string result = new JwtSecurityTokenHandler().WriteToken(token);
        return result;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public (string, string) GetToken(LoginRequest login)
    {
        (string,string) result = (BuildToken(login), GenerateRefreshToken());
        SaveRefreshToken(login, result.Item2);
        return result;
    }

    private void SaveRefreshToken(LoginRequest login, string token)
    {
        _redis.StringAppend($"Token:{login.Username}", token);
    }

    private void GetRefreshToken(LoginRequest login, string token)
    {
        string? refreshToken = _redis.StringGet($"Token:{login.Username}");
    }


}
