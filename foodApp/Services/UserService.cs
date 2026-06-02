using System.Net.Http.Json;
using System.Text.Json;
using foodApp.Models;

namespace foodApp.Services;

public static class UserService
{
    private const string TestDataResource = "test_users.json";
    private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "users.json");

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static List<User> users = [];
    private static User? currentUser;
    private static bool isLoaded;

    public static User? CurrentUser => currentUser;
    public static bool IsLoggedIn => currentUser is not null;

    // ----------------------------------------------------------------
    //  Load / Save
    // ----------------------------------------------------------------

    public static async Task LoadAsync()
    {
        if (isLoaded) return;

        // 1. Try loading from MockAPI (if configured).
        List<User>? fromApi = null;
        if (MockApiConfig.IsUsersConfigured)
        {
            fromApi = await TryLoadFromApiAsync();
        }

        // 2. Try loading from the saved file.
        var fromFile = await TryLoadFromFileAsync();

        // 3. Try loading seed users from the embedded resource.
        var fromResource = await TryLoadFromResourceAsync();

        // 4. Merge.  Priority:
        //    API > resource > file  (resource beats file so test users
        //    always have the correct default password).
        var merged = new List<User>();
        var seenUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // API users first (strongest).
        if (fromApi is { Count: > 0 })
        {
            foreach (var u in fromApi)
            {
                if (seenUsernames.Add(u.Username))
                    merged.Add(u);
            }
        }

        // Resource users next (ensure test users have correct defaults).
        if (fromResource is { Count: > 0 })
        {
            foreach (var u in fromResource)
            {
                if (seenUsernames.Add(u.Username))
                    merged.Add(u);
            }
        }

        // File users last (only custom-registered ones not in resource/api).
        if (fromFile is { Count: > 0 })
        {
            foreach (var u in fromFile)
            {
                if (seenUsernames.Add(u.Username))
                    merged.Add(u);
            }
        }

        // Persist to file so next launch sees the merged set directly.
        var fileCount = fromFile?.Count ?? 0;
        if (fromApi is { Count: > 0 } && fileCount < merged.Count)
        {
            await SaveAsync(merged);
        }
        else if (fileCount == 0 && merged.Count > 0)
        {
            await SaveAsync(merged);
        }

        users = merged;
        isLoaded = true;
    }

    // ----------------------------------------------------------------
    //  Auth operations
    // ----------------------------------------------------------------

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

        // Try MockAPI first.
        if (MockApiConfig.IsUsersConfigured)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(
                    MockApiConfig.UsersEndpointUrl, user, JsonOptions);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<User>(JsonOptions);
                if (created is not null)
                {
                    users.Add(created);
                    await SaveAsync(users);
                    currentUser = created;
                    return null;
                }
            }
            catch
            {
            }
        }

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
        isLoaded = false;
        await LoadAsync();
    }

    // ----------------------------------------------------------------
    //  Persistence helpers
    // ----------------------------------------------------------------

    private static async Task<List<User>?> TryLoadFromApiAsync()
    {
        try
        {
            var items = await HttpClient.GetFromJsonAsync<List<User>>(
                MockApiConfig.UsersEndpointUrl, JsonOptions);
            return items is { Count: > 0 } ? items : null;
        }
        catch
        {
            return null;
        }
    }

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
