using Sunrise.Model.Resources;

namespace Sunrise.Model;

public enum PlaylistParameterSorting
{
    /// <summary>По возрастанию</summary>
    Ascending,

    /// <summary>По убыванию</summary>
    Descending,
}

public static class PlaylistParameterSortingExtensions
{
    public static string GetName(this PlaylistParameterSorting sorting) => sorting switch
    {
        PlaylistParameterSorting.Ascending => Texts.Ascending,
        PlaylistParameterSorting.Descending => Texts.Descending,
        _ => throw new NotSupportedException(sorting.ToString()),
    };
}