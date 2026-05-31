using System.Text.Json.Serialization;

namespace foodApp.Models;

public sealed class FoodItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("uploadedBy")]
    public string UploadedBy { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    [JsonIgnore]
    public string RegionLabel => string.IsNullOrWhiteSpace(Region) ? "Unknown region" : Region;
}
