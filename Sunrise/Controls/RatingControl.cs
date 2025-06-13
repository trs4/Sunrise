using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;

namespace Sunrise.Controls;

[TemplatePart("PART_StarsPresenter", typeof(ItemsControl))]
public class RatingControl : TemplatedControl
{
    private int _value;
    private IEnumerable<int> _stars = Enumerable.Range(1, 5);

    public RatingControl()
        => UpdateStars();

    public static readonly StyledProperty<int> NumberOfStarsProperty
        = AvaloniaProperty.Register<RatingControl, int>(nameof(NumberOfStars), defaultValue: 5, coerce: CoerceNumberOfStars);

    public int NumberOfStars
    {
        get => GetValue(NumberOfStarsProperty);
        set => SetValue(NumberOfStarsProperty, value);
    }

    private static int CoerceNumberOfStars(AvaloniaObject sender, int value) => value < 1 ? 1 : value;

    public static readonly DirectProperty<RatingControl, int> ValueProperty
        = AvaloniaProperty.RegisterDirect<RatingControl, int>(nameof(Value), o => o.Value, (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public int Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public static readonly DirectProperty<RatingControl, IEnumerable<int>> StarsProperty
        = AvaloniaProperty.RegisterDirect<RatingControl, IEnumerable<int>>(nameof(Stars), o => o.Stars);

    public IEnumerable<int> Stars
    {
        get => _stars;
        private set => SetAndRaise(StarsProperty, ref _stars, value);
    }

    private void UpdateStars()
        => Stars = Enumerable.Range(1, NumberOfStars);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == NumberOfStarsProperty)
            UpdateStars();
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        base.UpdateDataValidation(property, state, error);

        if (property == ValueProperty)
            DataValidationErrors.SetError(this, error);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (e.Source is Path star && star.DataContext is int value)
            Value = value == 1 && e.GetPosition(this).X < 5 ? 0 : value;
    }

}

