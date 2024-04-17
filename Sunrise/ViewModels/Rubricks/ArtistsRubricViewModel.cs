using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public class ArtistsRubricViewModel : RubricViewModel
{
    public ArtistsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Artist)), Texts.Artists) { }

    public override Task<List<Track>> GetTracks(CancellationToken token = default) => Task.FromResult(new List<Track>());
}
