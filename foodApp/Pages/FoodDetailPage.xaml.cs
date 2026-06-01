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

    private bool isSpeaking;

    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = "Record not found";
            DescriptionLabel.Text = "This food entry could not be loaded.";
            PlayDescriptionButton.IsEnabled = false;
            return;
        }

        NameLabel.Text = currentItem.Name;
        RegionLabel.Text = $"Region: {currentItem.RegionLabel}";
        UploadedByLabel.Text = $"Uploaded by: {currentItem.UploadedBy}";
        DescriptionLabel.Text = currentItem.Description;

        var hasDescription = !string.IsNullOrWhiteSpace(currentItem.Description);
        PlayDescriptionButton.IsEnabled = hasDescription;
        PlayDescriptionButton.Text = "\U0001F50A  Play";

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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (isSpeaking)
        {
            SpeechService.Stop();
            isSpeaking = false;
            PlayDescriptionButton.Text = "\U0001F50A  Play";
        }
    }

    private async void OnPlayDescriptionClicked(object? sender, EventArgs e)
    {
        if (currentItem is null || string.IsNullOrWhiteSpace(currentItem.Description))
        {
            await DisplayAlert("No Description",
                "This food entry does not have a description to play.", "OK");
            return;
        }

        if (isSpeaking)
        {
            SpeechService.Stop();
            isSpeaking = false;
            PlayDescriptionButton.Text = "\U0001F50A  Play";
            return;
        }

        try
        {
            isSpeaking = true;
            PlayDescriptionButton.Text = "⏹  Stop";
            await SpeechService.SpeakAsync(currentItem.Description);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Playback Error",
                $"Could not play description: {ex.Message}", "OK");
        }
        finally
        {
            isSpeaking = false;
            PlayDescriptionButton.Text = "\U0001F50A  Play";
        }
    }
}
