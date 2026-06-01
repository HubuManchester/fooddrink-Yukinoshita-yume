namespace foodApp.Services;

public static class SpeechService
{
    private static CancellationTokenSource? currentSpeech;

    public static async Task SpeakAsync(string text)
    {
        Stop();

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.05f
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public static void Stop()
    {
        if (currentSpeech is null) return;

        currentSpeech.Cancel();
        currentSpeech.Dispose();
        currentSpeech = null;
    }
}
