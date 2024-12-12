using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JWT.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RecuperarInformacaoController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
		try
		{
			return Ok("Informação Recuperada");

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
