using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public abstract class RubricViewModel
{
    protected RubricViewModel(Player player, object icon, string name)
    {
        Player = player;
        Icon = icon;
        Name = name;
    }

    public Player Player { get; }

    public object Icon { get; }

    public string Name { get; }

    public abstract Task<List<Track>> GetTracks(CancellationToken token = default);

    public override string ToString() => Name;
}
