using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public abstract class RubricViewModel : ObservableObject
{
    protected RubricViewModel(Player player, object? icon, string name)
    {
        Player = player;
        Icon = icon;
        Name = name;
    }

    public Player Player { get; }

    public virtual object? Icon { get; set; }

    public string Name { get; }

    public virtual bool IsDependent => false;

    public abstract IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot);

    public abstract IReadOnlyList<Track> GetTracks(TracksScreenshot screenshot, TrackSourceViewModel? trackSource = null);

    public abstract IReadOnlyList<Track>? GetCurrentTracks();

    public override string ToString() => Name;
}
