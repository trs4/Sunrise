﻿using System.Collections.Generic;
using System.Linq;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;
using Sunrise.ViewModels.Albums;

namespace Sunrise.ViewModels;

public sealed class AlbumsRubricViewModel : RubricViewModel
{
    private TracksScreenshot? _screenshot;
    private List<AlbumViewModel>? _trackSources;
    private IReadOnlyList<Track>? _currentTracks;

    public AlbumsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Album)), Texts.Albums) { }

    public override IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot)
    {
        if (_trackSources is not null && ReferenceEquals(screenshot, _screenshot))
            return _trackSources;

        var trackSources = new List<AlbumViewModel>(screenshot.AllTracksByArtist.Sum(p => p.Value.Count));

        foreach (var pairByArtist in screenshot.AllTracksByArtist)
        {
            foreach (var pairByAlbum in pairByArtist.Value)
            {
                if (pairByAlbum.Key.Length == 0)
                    continue;

                var tracks = pairByAlbum.Value.OrderBy(t => t.Title).ToList();
                trackSources.Add(new AlbumViewModel(this, pairByAlbum.Key, pairByArtist.Key, tracks));
            }
        }

        trackSources.Sort((a, b) => string.Compare(a.Name, b.Name, true));
        _trackSources = trackSources;
        _screenshot = screenshot;
        return trackSources;
    }

    public override IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null)
        => _currentTracks = (trackSource as AlbumViewModel)?.Tracks ?? [];

    public override IReadOnlyList<Track>? GetCurrentTracks() => _currentTracks;
}
