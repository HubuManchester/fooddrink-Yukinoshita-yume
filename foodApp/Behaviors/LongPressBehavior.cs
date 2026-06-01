namespace foodApp.Behaviors;

public class LongPressBehavior : Behavior<View>
{
    private bool alreadyTriggered;
    private CancellationTokenSource? pressTimerCts;
    private Point? pressPoint;
    private PointerGestureRecognizer? pointerRecognizer;

    public static Action<string>? GlobalHandler { get; set; }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(
            nameof(CommandParameter),
            typeof(string),
            typeof(LongPressBehavior));

    public string? CommandParameter
    {
        get => (string?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);

        pointerRecognizer = new PointerGestureRecognizer();
        pointerRecognizer.PointerPressed += OnPointerPressed;
        pointerRecognizer.PointerMoved  += OnPointerMoved;
        pointerRecognizer.PointerReleased += OnPointerReleased;
        pointerRecognizer.PointerExited += OnPointerReleased;
        bindable.GestureRecognizers.Add(pointerRecognizer);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        if (pointerRecognizer is not null)
        {
            pointerRecognizer.PointerPressed  -= OnPointerPressed;
            pointerRecognizer.PointerMoved   -= OnPointerMoved;
            pointerRecognizer.PointerReleased -= OnPointerReleased;
            pointerRecognizer.PointerExited  -= OnPointerReleased;
            bindable.GestureRecognizers.Remove(pointerRecognizer);
            pointerRecognizer = null;
        }

        CancelTimer();
        base.OnDetachingFrom(bindable);
    }

    private void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        pressPoint = e.GetPosition(null);
        StartTimer();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (pressPoint is { } start)
        {
            var current = e.GetPosition(null);
            if (current is null) return;
            var dx = Math.Abs(current.Value.X - start.X);
            var dy = Math.Abs(current.Value.Y - start.Y);
            if (dx > 20 || dy > 20)
                CancelTimer();
        }
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        CancelTimer();
    }

    private void StartTimer()
    {
        CancelTimer();
        pressTimerCts = new CancellationTokenSource();
        var token = pressTimerCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(800, token);
                if (!token.IsCancellationRequested)
                    MainThread.BeginInvokeOnMainThread(Fire);
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
        }, token);
    }

    private void CancelTimer()
    {
        if (pressTimerCts is null) return;
        pressTimerCts.Cancel();
        pressTimerCts.Dispose();
        pressTimerCts = null;
        pressPoint = null;
    }

    private void Fire()
    {
        if (alreadyTriggered) return;
        alreadyTriggered = true;
        CancelTimer();

        var handler = GlobalHandler;
        var param = CommandParameter;
        if (handler is not null && param is not null)
            handler(param);

        _ = Task.Run(async () =>
        {
            await Task.Delay(800);
            alreadyTriggered = false;
        });
    }
}
