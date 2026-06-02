using foodApp.Models;
using foodApp.Services;

namespace foodApp.Pages;

public partial class AddFoodPage : ContentPage
{
    private string? capturedImagePath;

    public AddFoodPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);

        if (MapPage.SelectedRegion is not null)
        {
            RegionEntry.Text = MapPage.SelectedRegion;
            MapPage.SelectedRegion = null;
        }
    }

    private async void OnSelectFromMapClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapPage());
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Camera unavailable",
                    "This device does not support taking photos.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null) return;

            capturedImagePath = await SavePhotoAsync(photo);
            PreviewImage.Source = ImageSource.FromFile(capturedImagePath);
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission denied",
                "Camera permission is required. Please enable it in device settings.", "OK");
        }
        catch
        {
            await DisplayAlert("Error",
                "Could not take a photo. Please try again.", "OK");
        }
    }

    private async void OnPickFromGalleryClicked(object? sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is null) return;

            capturedImagePath = await SavePhotoAsync(photo);
            PreviewImage.Source = ImageSource.FromFile(capturedImagePath);
        }
        catch (PermissionException)
        {
            await DisplayAlert("Permission denied",
                "Gallery access is required. Please enable it in device settings.", "OK");
        }
        catch
        {
            await DisplayAlert("Error",
                "Could not load the photo from your gallery. Please try again.", "OK");
        }
    }

    private static async Task<string> SavePhotoAsync(FileResult photo)
    {
        var imagesDir = Path.Combine(FileSystem.AppDataDirectory, "food_images");
        Directory.CreateDirectory(imagesDir);

        var fileName = $"{Guid.NewGuid():N}.jpg";
        var filePath = Path.Combine(imagesDir, fileName);

        await using var sourceStream = await photo.OpenReadAsync();
        await using var destStream = File.OpenWrite(filePath);
        await sourceStream.CopyToAsync(destStream);

        return filePath;
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
                ImageUrl = capturedImagePath ?? "",
                UploadedBy = UserService.CurrentUser?.Username ?? "anonymous",
                Tags = $"{NameEntry.Text} {RegionEntry.Text} {DescriptionEditor.Text}"
            };

            await FoodService.AddAsync(item);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Food entry saved.");

            await DisplayAlert("Saved", "Food entry added successfully.", "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch
        {
            ShowValidation("Something went wrong while saving. Please try again.");
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
