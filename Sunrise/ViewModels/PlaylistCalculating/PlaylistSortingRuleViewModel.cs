using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public sealed class PlaylistSortingRuleViewModel : ObservableObject
{
    public static readonly PlaylistParameter[] Parameters = Enum.GetValues<PlaylistParameter>();

    public static readonly PlaylistParameterSorting[] Sortings = Enum.GetValues<PlaylistParameterSorting>();

    private PlaylistParameter _parameter;
    private PlaylistParameterSorting _sorting;

    public PlaylistParameter Parameter
    {
        get => _parameter;
        set => SetProperty(ref _parameter, value);
    }

    public PlaylistParameterSorting Sorting
    {
        get => _sorting;
        set => SetProperty(ref _sorting, value);
    }

}
