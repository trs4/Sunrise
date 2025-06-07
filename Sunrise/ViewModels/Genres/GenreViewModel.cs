using System.Collections.Generic;
using Sunrise.Model;

namespace Sunrise.ViewModels.Genres;

public sealed class GenreViewModel : TrackSourceViewModel
{
    public GenreViewModel(string name, List<Track> tracks, GenresRubricViewModel rubric)
        : base(rubric)
    {
        Name = name;
        Tracks = tracks;
    }

    public string Name { get; }

    public List<Track> Tracks { get; }

    public override string ToString() => Name;
}
