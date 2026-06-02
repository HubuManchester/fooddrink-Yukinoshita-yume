using System.Text.Json;
using foodApp.Models;

namespace foodApp.Services;

public static class UserService
{
    private const string TestDataResource = "test_users.json";
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "users.json");

    private static List<User> users = [];
    private static User? currentUser;

    public static User? CurrentUser => currentUser;
    public static bool IsLoggedIn => currentUser is not null;

    public static async Task LoadAsync()
    {
        // 1. Try loading from the saved file.
        var fromFile = await TryLoadFromFileAsync();

        // 2. Try loading seed users from the embedded resource.
        var fromResource = await TryLoadFromResourceAsync();

        // 3. Merge: persisted users take priority; resource seeds fill gaps.
        var merged = new List<User>();
        var seenUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (fromFile is { Count: > 0 })
        {
            foreach (var u in fromFile)
            {
                if (seenUsernames.Add(u.Username))
                    merged.Add(u);
            }
        }

        if (fromResource is { Count: > 0 })
        {
            foreach (var u in fromResource)
            {
                if (seenUsernames.Add(u.Username))
                    merged.Add(u);
            }
        }

        // If the file didn't have any users, save the merged set so test users
        // are persisted.
        if ((fromFile is null || fromFile.Count == 0) && merged.Count > 0)
        {
            await SaveAsync(merged);
        }

        users = merged;
    }

    public static async Task<string?> RegisterAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "Username cannot be empty.";

        if (string.IsNullOrWhiteSpace(password))
            return "Password cannot be empty.";

        if (password.Length < 3)
            return "Password must be at least 3 characters.";

        if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            return "This username is already taken. Please choose another one.";

        var user = new User
        {
            Username = username.Trim(),
            Password = password
        };

        users.Add(user);
        await SaveAsync(users);
        currentUser = user;
        return null;
    }

    public static string? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return "Please enter both username and password.";

        var user = users.FirstOrDefault(u =>
            u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user is null)
            return "Incorrect username or password.";

        currentUser = user;
        return null;
    }

    public static void Logout()
    {
        currentUser = null;
    }

    // ----------------------------------------------------------------
    //  Cache management
    // ----------------------------------------------------------------

    public static async Task ClearCacheAsync()
    {
        Logout();

        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch
        {
        }

        users = [];
        await LoadAsync();
    }

    // ----------------------------------------------------------------
    //  Persistence helpers
    // ----------------------------------------------------------------

    private static async Task<List<User>?> TryLoadFromFileAsync()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = await File.ReadAllTextAsync(FilePath);
                return JsonSerializer.Deserialize<List<User>>(json);
            }
        }
        catch
        {
        }

        return null;
    }

    private static async Task<List<User>?> TryLoadFromResourceAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(TestDataResource);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<List<User>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static async Task SaveAsync(List<User> list)
    {
        try
        {
            var json = JsonSerializer.Serialize(list);
            await File.WriteAllTextAsync(FilePath, json);
        }
        catch
        {
        }
    }
}
