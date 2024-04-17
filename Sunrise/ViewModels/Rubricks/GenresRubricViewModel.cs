using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public class GenresRubricViewModel : RubricViewModel
{
    public GenresRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Genre)), Texts.Genres) { }

    public override Task<List<Track>> GetTracks(CancellationToken token = default) => Task.FromResult(new List<Track>());
}
