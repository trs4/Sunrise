using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.ViewModels;

public class SongsRubricViewModel : RubricViewModel
{
    public SongsRubricViewModel(Player player) : base(player, IconSource.From(nameof(Icons.Song)), Texts.Songs) { }

    public override async Task<List<Track>> GetTracks(CancellationToken token = default)
    {
        var screenshot = await Player.GetAllTracks(token);
        return screenshot.AllTracks;
    }

}
