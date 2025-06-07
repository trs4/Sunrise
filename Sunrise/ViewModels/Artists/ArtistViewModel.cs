using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;

namespace Sunrise.ViewModels.Artists;

public sealed class ArtistViewModel : TrackSourceViewModel
{
    private readonly Dictionary<string, List<Track>> _tracksByAlbums;

    public ArtistViewModel(string name, Dictionary<string, List<Track>> tracksByAlbums, ArtistsRubricViewModel rubric)
        : base(rubric)
    {
        Name = name;
        _tracksByAlbums = tracksByAlbums;
    }

    public string Name { get; }

    public List<Track> GetTracks()
    {
        var tracks = new List<Track>(_tracksByAlbums.Sum(p => p.Value.Count));

        foreach (var albumTracks in _tracksByAlbums.Values)
            tracks.AddRange(albumTracks);

        tracks.Sort((a, b) => string.Compare(a.Title, b.Title, true));
        return tracks;
    }

    public override string ToString() => Name;
}
