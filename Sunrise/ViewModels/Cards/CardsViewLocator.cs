using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Sunrise.ViewModels.Cards;

public class CardsViewLocator : IDataTemplate
{
    public static CardsViewLocator Instance { get; } = new();

    public Control? Build(object? data)
    {
        if (data is InPlaylistDeviceCardViewModel)
            return new InPlaylistDeviceCardView();
        else if (data is InformationDeviceCardViewModel)
            return new InformationDeviceCardView();

        return null;
    }

    public bool Match(object? data) => data is DeviceCardViewModel;
}
