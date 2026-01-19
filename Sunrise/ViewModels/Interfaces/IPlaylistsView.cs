using Sunrise.Model;

namespace Sunrise.ViewModels.Interfaces;

public interface IPlaylistsView
{
    void ScrollIntoView(Track? track);

    void ScrollIntoView(Playlist? playlist);
}
