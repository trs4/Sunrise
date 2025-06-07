using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sunrise.ViewModels.Columns;

public class CheckedColumnViewModel<TViewModel> : ColumnViewModel, ICheckedColumn
    where TViewModel : ObservableObject
{
    private readonly Func<TViewModel, bool> _getter;
    private readonly Action<TViewModel, bool>? _setter;

    public CheckedColumnViewModel(string name, Func<TViewModel, bool> getter, Action<TViewModel, bool>? setter = null)
        : base(name, setter is null)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter;
        Width = 20;
        CanUserResize = CanUserSort = false;
    }

    public override Type PropertyType => typeof(bool);

    public override object? GetValue(object? component)
        => component is TViewModel source && _getter(source);

    public override void SetValue(object? component, object? value)
    {
        if (_setter is not null && component is TViewModel source && value is bool boolValue)
            _setter(source, boolValue);
    }

}
