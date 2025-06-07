using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Sunrise.ViewModels.Columns;

namespace Sunrise.Controls;

public class ColumnWrapperViewModel : INotifyPropertyChanged
{
    private readonly Delegate? _getter;
    private readonly object? _obj;
    private readonly Delegate? _setter;
    private object? _value;

    public ColumnWrapperViewModel(object? obj,
        Delegate? getter,
        Delegate? setter = null,
        ColumnViewModel? viewModel = null)
    {
        _obj = obj;
        _getter = getter;
        _setter = setter;
        ViewModel = viewModel;

        if (_obj is INotifyPropertyChanged notifyObject)
            notifyObject.PropertyChanged += NotifyObject_PropertyChanged;
    }

    public ColumnViewModel? ViewModel { get; }

    public object? Value
    {
        get
        {
            if (_value is not null)
                return _value;

            if (_getter is not null)
                _value = _getter.DynamicInvoke(_obj);

            return _value;
        }
        set
        {
            if (SetPropertyValue(ref _value, value) && _setter is not null)
            {
                var delegates = _setter.GetInvocationList();
                var parameters = delegates[0].Method.GetParameters();
                var type = parameters[1].ParameterType;

                object? getValue = GetValue(value);
                object? convertValue = Convert(getValue, type);

                _setter.DynamicInvoke(_obj, convertValue);
            }
        }
    }

    private void NotifyObject_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_getter is null)
            return;

        var newValue = _getter.DynamicInvoke(_obj);

        if (_value != newValue)
        {
            _value = newValue;
            OnPropertyChanged(nameof(Value));
        }
    }

    private static object? GetValue(object? sourceValue)
    {
        if (sourceValue is ContentControl contentControl)
            return contentControl.Content;

        return sourceValue;
    }

    private static object? Convert(object? value, Type? targetType)
    {
        if (value is not null && targetType is not null)
        {
            if (value.GetType() == targetType)
                return value;

            if (targetType == typeof(string))
                return value.ToString();

            if (targetType == typeof(object))
                return value;

            return System.Convert.ChangeType(value, ToConvertibleType(targetType));
        }

        return value;
    }

    private static Type ToConvertibleType(Type? type)
    {
        if (type is null)
            return typeof(object);

        if (!typeof(IConvertible).IsAssignableFrom(type))
            return type;

        return type;
    }

    public override string? ToString() => Value?.ToString();

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetPropertyValue<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
