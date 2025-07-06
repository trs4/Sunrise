using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Resources;

namespace Sunrise.ViewModels.Genres;

public sealed class GenreViewModel : TrackSourceViewModel
{
    public GenreViewModel(GenresRubricViewModel rubric, string name, List<Track> tracks)
        : base(rubric, name, string.Format(Texts.SongsFormat, tracks.Count))
        => Tracks = tracks;

    public List<Track> Tracks { get; }

    protected override Track? GetTrackWithPicture() => Tracks.FirstOrDefault(t => t.HasPicture);

    public override string ToString() => Name;
}
