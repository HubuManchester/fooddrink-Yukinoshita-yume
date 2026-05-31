using System.Net.Http.Json;
using System.Text.Json;
using foodApp.Models;

namespace foodApp.Services;

public static class FoodService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly List<FoodItem> LocalFallbackItems =
    [
        new()
        {
            Id = "local1",
            Name = "Peking Duck",
            ImageUrl = "",
            Region = "Beijing, China",
            UploadedBy = "system",
            Description = "Crispy roast duck served with thin pancakes, spring onions, and sweet bean sauce.",
            Tags = "beijing duck roast chinese"
        },
        new()
        {
            Id = "local2",
            Name = "Sushi Platter",
            ImageUrl = "",
            Region = "Tokyo, Japan",
            UploadedBy = "system",
            Description = "Assorted nigiri and maki rolls with fresh salmon, tuna, and shrimp.",
            Tags = "sushi japanese seafood"
        },
        new()
        {
            Id = "local3",
            Name = "Margherita Pizza",
            ImageUrl = "",
            Region = "Naples, Italy",
            UploadedBy = "system",
            Description = "Classic pizza with San Marzano tomatoes, fresh mozzarella, and basil.",
            Tags = "pizza italian vegetarian"
        },
        new()
        {
            Id = "local4",
            Name = "Tom Yum Goong",
            ImageUrl = "",
            Region = "Bangkok, Thailand",
            UploadedBy = "system",
            Description = "Spicy and sour shrimp soup with lemongrass, galangal, and lime leaves.",
            Tags = "thai soup spicy shrimp"
        }
    ];

    private static List<FoodItem> cachedItems = new(LocalFallbackItems);

    public static bool LastLoadUsedMockApi { get; private set; }

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
        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var item = await HttpClient.GetFromJsonAsync<FoodItem>(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(id)}",
                    JsonOptions);

                if (item is not null)
                {
                    return item;
                }
            }
            catch
            {
            }
        }

        return cachedItems.FirstOrDefault(item => item.Id == id);
    }

    public static async Task<FoodItem> AddAsync(FoodItem item)
    {
        if (MockApiConfig.IsConfigured)
        {
            var response = await HttpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item, JsonOptions);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<FoodItem>(JsonOptions);
            if (created is not null)
            {
                cachedItems.Add(created);
                return created;
            }
        }

        cachedItems.Add(item);
        return item;
    }

    public static FoodItem GetRandom()
    {
        return cachedItems[Random.Shared.Next(cachedItems.Count)];
    }

    private static async Task<IReadOnlyList<FoodItem>> GetAllAsync()
    {
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
