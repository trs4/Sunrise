using Sunrise.Model;

namespace Sunrise.ViewModels.Interfaces;

public interface ITracksView
{
    void ScrollIntoView(Track track);

    void ScrollIntoView(string trackSource);
}
