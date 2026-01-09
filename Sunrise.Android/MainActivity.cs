using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using Sunrise.Android.Model;

namespace Sunrise.Android;

[Activity(
    Label = "Музыка",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private MediaManager? _manager;

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
        .WithInterFont();

#pragma warning disable CA2000 // Dispose objects before losing scope
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        _manager = new MediaManager(this);
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent? e)
    {
        if (e?.Action == KeyEventActions.Down)
            _manager?.Execute(keyCode);

        return base.OnKeyDown(keyCode, e);
    }

    protected override void OnDestroy()
    {
        _manager?.Release();
        _manager = null;

        base.OnDestroy();
    }

}
