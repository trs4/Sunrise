using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Sunrise.Controls;
using Sunrise.Converters;

namespace Sunrise.ViewModels.Columns;

public class IsPlayingColumnViewModel : ColumnViewModel
{
    private readonly IDataTemplate _iconDataTemplate = new FuncDataTemplate<object>((_, _) => new Image
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        OpacityMask = Brushes.Gray,
        Opacity = 0.25,
        Width = 12,
        Height = 12,
        [!Image.SourceProperty] = new Binding(nameof(ColumnWrapperViewModel.Value)) { Converter = IsPlayingToIconConverter.Instance },
    });

    public IsPlayingColumnViewModel()
        : base(nameof(TrackViewModel.IsPlaying))
    {
        Width = 20;
        CanUserResize = CanUserSort = CanUserReorder = false;
        CellTemplate = _iconDataTemplate;
    }

    public override Type PropertyType => typeof(bool?);

    public override object? GetValue(object? component)
        => component is TrackViewModel trackViewModel ? trackViewModel.IsPlaying : null;
}
