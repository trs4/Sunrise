using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public class AlbumsRubricViewModel : RubricViewModel
{
    public AlbumsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Album)), Texts.Albums) { }

    public override Task<List<Track>> GetTracks(CancellationToken token = default) => Task.FromResult(new List<Track>());
}
