using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sunrise.ViewModels.Cards;

public abstract class DeviceCardViewModel : ObservableObject
{
    protected DeviceCardViewModel(TrackPlayDeviceViewModel owner)
    {
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        CloseCardCommand = new RelayCommand(CloseCard);
    }

    public TrackPlayDeviceViewModel Owner { get; }

    public IRelayCommand CloseCardCommand { get; }

    public void CloseCard()
    {
        Owner.ShowCard = false;
        Owner.CardDialog = null;
    }

}
