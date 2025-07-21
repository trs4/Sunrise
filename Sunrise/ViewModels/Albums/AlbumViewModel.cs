﻿using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public sealed class AlbumViewModel : TrackSourceViewModel
{
    public AlbumViewModel(AlbumsRubricViewModel rubric, string name, string artist, List<Track> tracks)
        : base(rubric, name, artist)
    {
        Artist = artist;
        Tracks = tracks;
    }

    public string Artist { get; }

    public List<Track> Tracks { get; }

    protected override Track? GetTrackWithPicture() => Tracks.FirstOrDefault(t => t.HasPicture);

    public override string ToString() => $"{Artist} - {Name}";
}
