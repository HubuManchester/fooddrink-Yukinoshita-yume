using System.Text.Json;
using foodApp.Models;

namespace foodApp.Services;

public static class UserService
{
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "users.json");

    private static List<User> users = new();

    private static User? currentUser;

    public static User? CurrentUser => currentUser;

    public static bool IsLoggedIn => currentUser is not null;

    public static async Task LoadAsync()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
        }
        catch
        {
            users = new List<User>();
        }
    }

    public static async Task<string?> RegisterAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "Username cannot be empty.";
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return "Password cannot be empty.";
        }

        if (password.Length < 3)
        {
            return "Password must be at least 3 characters.";
        }

        if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            return "This username is already taken. Please choose another one.";
        }

        var user = new User
        {
            Username = username.Trim(),
            Password = password
        };

        users.Add(user);
        await SaveAsync();
        currentUser = user;
        return null;
    }

    public static string? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return "Please enter both username and password.";
        }

        var user = users.FirstOrDefault(u =>
            u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user is null)
        {
            return "Incorrect username or password.";
        }

        currentUser = user;
        return null;
    }

    public static void Logout()
    {
        currentUser = null;
    }

    private static async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(FilePath, json);
    }
}
