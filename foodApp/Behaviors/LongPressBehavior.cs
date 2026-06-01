using System.Windows.Input;

namespace foodApp.Behaviors;

public class LongPressBehavior : Behavior<View>
{
    private bool alreadyTriggered;
    private CancellationTokenSource? longPressCts;
    private bool nativeAttached;

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(LongPressBehavior));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(string), typeof(LongPressBehavior));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public string? CommandParameter
    {
        get => (string?)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.InputTransparent = false;
        bindable.HandlerChanged += OnViewHandlerChanged;
    }

    protected override void OnDetachingFrom(View bindable)
    {
        bindable.HandlerChanged -= OnViewHandlerChanged;
        longPressCts?.Cancel();
        longPressCts?.Dispose();
        longPressCts = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnViewHandlerChanged(object? sender, EventArgs e)
    {
        if (sender is not View view || nativeAttached) return;

#if ANDROID
        if (view.Handler?.PlatformView is Android.Views.View nativeView)
        {
            nativeView.LongClick += (_, _) => FireCommand();
            nativeAttached = true;
        }
#elif IOS || MACCATALYST
        if (view.Handler?.PlatformView is UIKit.UIView iosView)
        {
            var recognizer = new UIKit.UILongPressGestureRecognizer(gr =>
            {
                if (gr.State == UIKit.UIGestureRecognizerState.Began)
                {
                    FireCommand();
                }
            });
            recognizer.MinimumPressDuration = 0.8;
            iosView.AddGestureRecognizer(recognizer);
            nativeAttached = true;
        }
#elif WINDOWS
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement winView)
        {
            winView.Holding += (_, args) =>
            {
                if (args.HoldingState == Microsoft.UI.Input.HoldingState.Started)
                {
                    FireCommand();
                }
            };
            nativeAttached = true;
        }
#endif
    }

    private void FireCommand()
    {
        if (alreadyTriggered) return;
        alreadyTriggered = true;

        if (Command is not null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }

        // Reset after a short delay to allow next long press
        Task.Run(async () =>
        {
            await Task.Delay(500);
            alreadyTriggered = false;
        });
    }
}
