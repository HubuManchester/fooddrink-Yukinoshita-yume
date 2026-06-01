using System.Net.Http.Json;
using System.Text.Json;
using foodApp.Models;

namespace foodApp.Services;

public static class FoodService
{
    private const string TestDataResource = "test_food_data.json";
    private static readonly string SavedFilePath =
        Path.Combine(FileSystem.AppDataDirectory, "food_data.json");

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // All seed data lives in Resources/Raw/test_food_data.json.
    // At runtime it is merged with user-added items and persisted
    // to food_data.json.  If neither source is available, the
    // collection starts empty.
    private static List<FoodItem> cachedItems = [];
    private static bool isLoaded;

    public static bool LastLoadUsedMockApi { get; private set; }

    // ----------------------------------------------------------------
    //  Load / Save
    // ----------------------------------------------------------------

    public static async Task LoadAsync()
    {
        if (isLoaded) return;

        // 1. Try loading seed data from the embedded JSON resource.
        var seedFromResource = await TryLoadFromResourceAsync();

        // 2. Try loading persisted data from the local JSON file.
        var persisted = await TryLoadFromFileAsync();

        if (persisted is { Count: > 0 } && seedFromResource is { Count: > 0 })
        {
            // Merge: keep all user-added items + seed items from the resource.
            var merged = new List<FoodItem>(seedFromResource);
            var existingIds = new HashSet<string>(merged.Select(i => i.Id));
            foreach (var item in persisted)
            {
                if (!existingIds.Contains(item.Id))
                {
                    merged.Add(item);
                    existingIds.Add(item.Id);
                }
            }

            if (persisted.Count < merged.Count)
                await SaveInternalAsync(merged);

            cachedItems = merged;
        }
        else if (persisted is { Count: > 0 })
        {
            cachedItems = persisted;
        }
        else if (seedFromResource is { Count: > 0 })
        {
            cachedItems = seedFromResource;
            await SaveInternalAsync(cachedItems);
        }
        else
        {
            cachedItems = [];
        }

        isLoaded = true;
    }

    public static async Task SaveAsync()
    {
        await SaveInternalAsync(cachedItems);
    }

    private static async Task SaveInternalAsync(List<FoodItem> items)
    {
        try
        {
            var json = JsonSerializer.Serialize(items, JsonOptions);
            await File.WriteAllTextAsync(SavedFilePath, json);
        }
        catch
        {
        }
    }

    private static async Task<List<FoodItem>?> TryLoadFromResourceAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(TestDataResource);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<List<FoodItem>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<List<FoodItem>?> TryLoadFromFileAsync()
    {
        try
        {
            if (File.Exists(SavedFilePath))
            {
                var json = await File.ReadAllTextAsync(SavedFilePath);
                return JsonSerializer.Deserialize<List<FoodItem>>(json, JsonOptions);
            }
        }
        catch
        {
        }

        return null;
    }

    // ----------------------------------------------------------------
    //  Query
    // ----------------------------------------------------------------

    public static async Task<IReadOnlyList<FoodItem>> SearchAsync(string? query)
    {
        var items = await GetAllAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return items.OrderBy(item => item.Name).ToList();
        }

        var normalised = query.Trim();
        return items
            .Where(item =>
                item.Name.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Region.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                item.Tags.Contains(normalised, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Name)
            .ToList();
    }

    public static async Task<FoodItem?> GetByIdAsync(string id)
    {
        await EnsureLoadedAsync();

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var item = await HttpClient.GetFromJsonAsync<FoodItem>(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(id)}",
                    JsonOptions);

                if (item is not null) return item;
            }
            catch
            {
            }
        }

        return cachedItems.FirstOrDefault(item => item.Id == id);
    }

    public static async Task<FoodItem> AddAsync(FoodItem item)
    {
        await EnsureLoadedAsync();

        if (MockApiConfig.IsConfigured)
        {
            var response = await HttpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item, JsonOptions);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<FoodItem>(JsonOptions);
            if (created is not null)
            {
                cachedItems.Add(created);
                await SaveAsync();
                return created;
            }
        }

        cachedItems.Add(item);
        await SaveAsync();
        return item;
    }

    public static FoodItem? GetRandom()
    {
        if (cachedItems.Count == 0) return null;
        return cachedItems[Random.Shared.Next(cachedItems.Count)];
    }

    public static async Task<(IReadOnlyList<FoodItem> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? query = null, bool randomOrder = false)
    {
        var all = await SearchAsync(query);

        if (randomOrder)
        {
            all = all.OrderBy(_ => Random.Shared.Next()).ToList();
        }
        else
        {
            all = all.OrderBy(item => item.Name).ToList();
        }

        var totalCount = all.Count;
        var items = all.Skip(page * pageSize).Take(pageSize).ToList();
        return (items, totalCount);
    }

    // ----------------------------------------------------------------
    //  Internal helpers
    // ----------------------------------------------------------------

    private static async Task EnsureLoadedAsync()
    {
        if (!isLoaded) await LoadAsync();
    }

    private static async Task<IReadOnlyList<FoodItem>> GetAllAsync()
    {
        await EnsureLoadedAsync();

        if (!MockApiConfig.IsConfigured)
        {
            LastLoadUsedMockApi = false;
            return cachedItems;
        }

        try
        {
            var items = await HttpClient.GetFromJsonAsync<List<FoodItem>>(MockApiConfig.EndpointUrl, JsonOptions);
            if (items is { Count: > 0 })
            {
                cachedItems = items;
                await SaveAsync();
                LastLoadUsedMockApi = true;
                return cachedItems;
            }
        }
        catch
        {
        }

        LastLoadUsedMockApi = false;
        return cachedItems;
    }
}
