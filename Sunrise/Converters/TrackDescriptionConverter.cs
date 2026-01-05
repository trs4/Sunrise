using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Sunrise.ViewModels;

namespace Sunrise.Converters;

public sealed class TrackDescriptionConverter : IMultiValueConverter
{
    public static readonly TrackDescriptionConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[0] is not MainViewModel mainViewModel || values[1] is not TrackViewModel trackViewModel)
            return null;

        if (mainViewModel.SelectedTrackSource is null)
            return trackViewModel.Artist;
        else if (mainViewModel.SelectedTrackSource is ArtistViewModel)
            return trackViewModel.Album;
        else if (mainViewModel.SelectedTrackSource is AlbumViewModel)
            return null;

        string str1 = trackViewModel.Artist;
        string str2 = trackViewModel.Album;

        if (string.IsNullOrWhiteSpace(str1))
            str1 = null;

        if (string.IsNullOrWhiteSpace(str2))
            str2 = null;

        return str1 is null ? str2 : (str2 is null ? str1 : $"{str1} - {str2}");
    }

}
