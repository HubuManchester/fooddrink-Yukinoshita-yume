namespace foodApp.Models;

public sealed class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
