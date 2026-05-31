using foodApp.Models;
using foodApp.Services;

namespace foodApp.Pages;

[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : ContentPage
{
    private FoodItem? currentItem;

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodService.GetByIdAsync(id);
        RenderItem();
    }

    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = "Record not found";
            DescriptionLabel.Text = "This food entry could not be loaded.";
            return;
        }

        NameLabel.Text = currentItem.Name;
        RegionLabel.Text = $"Region: {currentItem.RegionLabel}";
        UploadedByLabel.Text = $"Uploaded by: {currentItem.UploadedBy}";
        DescriptionLabel.Text = currentItem.Description;

        if (!string.IsNullOrWhiteSpace(currentItem.ImageUrl))
        {
            if (currentItem.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                FoodImage.Source = ImageSource.FromUri(new Uri(currentItem.ImageUrl));
            }
            else
            {
                FoodImage.Source = ImageSource.FromFile(currentItem.ImageUrl);
            }
        }
        else
        {
            FoodImage.HeightRequest = 0;
        }
    }
}
