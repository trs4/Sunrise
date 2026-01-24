namespace Sunrise.Model;

public class PlaylistCalculatedData
{
    public List<PlaylistTermRule>? TermRules { get; set; }

    public List<PlaylistSortingRule>? SortingRules { get; set; }

    public int MaxTracks { get; set; }
}
