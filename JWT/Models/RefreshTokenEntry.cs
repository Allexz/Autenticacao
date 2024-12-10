namespace JWT.Models;

public class RefreshTokenEntry
{
    public string Username { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
}
