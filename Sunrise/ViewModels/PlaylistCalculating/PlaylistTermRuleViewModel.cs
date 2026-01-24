using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public sealed class PlaylistTermRuleViewModel : ObservableObject
{
    public static readonly PlaylistParameter[] Parameters = Enum.GetValues<PlaylistParameter>();

    public static readonly PlaylistParameterOperator[] Operators = Enum.GetValues<PlaylistParameterOperator>();

    private PlaylistParameter _parameter;
    private PlaylistParameterOperator _operator;
    private string _value;

    public PlaylistParameter Parameter
    {
        get => _parameter;
        set => SetProperty(ref _parameter, value);
    }

    public PlaylistParameterOperator Operator
    {
        get => _operator;
        set => SetProperty(ref _operator, value);
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

}
