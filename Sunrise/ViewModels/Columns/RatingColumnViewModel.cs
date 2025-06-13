using System;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Sunrise.Controls;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels.Columns;

public class RatingColumnViewModel : ColumnViewModel
{
    private readonly IDataTemplate _dataTemplate = new FuncDataTemplate<object>((_, _) => new RatingControl
    {
        NumberOfStars = 5,
        [!RatingControl.ValueProperty] = new Binding(nameof(ColumnWrapperViewModel.Value)),
    });

    public RatingColumnViewModel()
        : base(nameof(TrackViewModel.Rating), false)
    {
        Caption = Texts.Rating;
        Width = 80;
        CellTemplate = _dataTemplate;
        EditTemplate = _dataTemplate;
    }

    public override Type PropertyType => typeof(byte);

    public override object? GetValue(object? component)
        => component is TrackViewModel trackViewModel ? trackViewModel.Rating : (byte)0;

    public override void SetValue(object? component, object? value)
    {
        //if (_setter is not null && component is TViewModel source && value is bool boolValue)
        //    _setter(source, boolValue);
    }

}
