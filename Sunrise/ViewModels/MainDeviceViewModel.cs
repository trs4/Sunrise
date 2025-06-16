using Sunrise.Model;

namespace Sunrise.ViewModels;

public sealed class MainDeviceViewModel : MainViewModel
{
    private bool _isShortTrackVisible;
    private bool _isTrackVisible;

    public MainDeviceViewModel() { } // For designer

    public MainDeviceViewModel(Player player) : base(player) { }

    public bool IsShortTrackVisible
    {
        get => _isShortTrackVisible;
        set => SetProperty(ref _isShortTrackVisible, value);
    }

    public bool IsTrackVisible
    {
        get => _isTrackVisible;
        set => SetProperty(ref _isTrackVisible, value);
    }

}
