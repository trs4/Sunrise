using System.Collections.Generic;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public abstract class RubricViewModel
{
    protected RubricViewModel(Player player, object? icon, string name)
    {
        Player = player;
        Icon = icon;
        Name = name;
    }

    public Player Player { get; }

    public object? Icon { get; }

    public string Name { get; }

    public virtual bool IsDependent => false;

    public abstract IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot);

    public abstract IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null);

    public override string ToString() => Name;
}
