using JWT.Models;

namespace JWT.Servico;

public interface IJWTServices
{
    (string,string) GetToken(LoginRequest login);
}
