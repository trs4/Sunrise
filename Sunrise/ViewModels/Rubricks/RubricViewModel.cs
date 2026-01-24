using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;
using Sunrise.ViewModels.Rubricks;

namespace Sunrise.ViewModels;

public abstract class RubricViewModel : ObservableObject
{
    private string _name;

    protected RubricViewModel(Player player, object? icon, string name)
    {
        Player = player;
        Icon = icon;
        Name = name;
    }

    public Player Player { get; }

    public virtual object? Icon { get; set; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public abstract RubricTypes Type { get; }

    public virtual bool IsDependent => false;

    public abstract IReadOnlyList<TrackSourceViewModel>? GetTrackSources(TracksScreenshot screenshot);

    public abstract ValueTask<IReadOnlyList<Track>> GetTracksAsync(TrackSourceViewModel? trackSource = null, CancellationToken token = default);

    public abstract IReadOnlyList<Track>? GetCurrentTracks();

    public override string ToString() => Name;
}
