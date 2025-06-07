using System.Collections.Generic;
using Sunrise.Model;

namespace Sunrise.ViewModels.Albums;

public sealed class AlbumViewModel : TrackSourceViewModel
{
    public AlbumViewModel(string name, string artist, List<Track> tracks, AlbumsRubricViewModel rubric)
        : base(rubric)
    {
        Name = name;
        Artist = artist;
        Tracks = tracks;
    }

    public string Name { get; }

    public string Artist { get; }

    public List<Track> Tracks { get; }

    public override string ToString() => $"{Name} - {Artist}";
}
