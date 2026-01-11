using System;
using Android.Bluetooth;
using Android.Content;
using Avalonia.Android;
using Sunrise.Services;

namespace Sunrise.Android.Services;

internal sealed class AppEnvironment : IAppEnvironment
{
    private readonly AvaloniaMainActivity _activity;
    private string? _machineName;

    public AppEnvironment(AvaloniaMainActivity activity)
        => _activity = activity ?? throw new ArgumentNullException(nameof(activity));

    public string MachineName => _machineName ??= GetMachineName();

    private string GetMachineName() => ((BluetoothManager)_activity.GetSystemService(Context.BluetoothService))?.Adapter?.Name ?? "Android";
}
