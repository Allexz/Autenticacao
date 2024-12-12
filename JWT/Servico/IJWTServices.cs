using JWT.Models;

namespace JWT.Servico;

public interface IJWTServices
{
    Task<TokenResponse> GetToken(LoginRequest login);
    Task<string?> GetRefreshToken(RefreshTokenRequest refreshReq);
}
