using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sunrise.ViewModels.Columns;

public class TextColumnViewModel<TViewModel, T> : ColumnViewModel
    where TViewModel : ObservableObject
{
    private readonly Func<TViewModel, T> _getter;
    private readonly Action<TViewModel, T>? _setter;

    public TextColumnViewModel(string name, Func<TViewModel, T> getter, Action<TViewModel, T>? setter = null)
        : base(name, setter is null)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter;
    }

    public override Type PropertyType => typeof(T);

    public override object? GetValue(object? component)
        => component is TViewModel source ? _getter(source) : null;

    public override void SetValue(object? component, object? value)
    {
        if (_setter is not null && component is TViewModel source)
            _setter(source, (T)value);
    }

}
