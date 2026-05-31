using foodApp.Models;
using foodApp.Services;

namespace foodApp.Pages;

public partial class AddFoodPage : ContentPage
{
    public AddFoodPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var validationMessage = ValidateForm();
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }

            var item = new FoodItem
            {
                Name = NameEntry.Text!.Trim(),
                Region = RegionEntry.Text?.Trim() ?? "",
                Description = DescriptionEditor.Text!.Trim(),
                ImageUrl = ImageUrlEntry.Text?.Trim() ?? "",
                UploadedBy = UserService.CurrentUser?.Username ?? "anonymous",
                Tags = $"{NameEntry.Text} {RegionEntry.Text} {DescriptionEditor.Text}"
            };

            await FoodService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Food entry saved.");

            await DisplayAlert(
                "Saved",
                MockApiConfig.IsConfigured
                    ? "The food entry has been saved to mockapi.io."
                    : "The food entry has been saved to local data.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation($"Could not save the entry: {ex.Message}");
        }
    }

    private string? ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            return "Please enter a food name.";
        }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            return "Please add a description for the food.";
        }

        return null;
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
