using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public abstract class TrackSourceViewModel : ObservableObject
{
    private object? _trackIcon;
    private bool _trackLoaded;

    protected TrackSourceViewModel(RubricViewModel rubric, string name, string description)
    {
        Rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));
        Name = name;
        Description = description;
    }

    public RubricViewModel Rubric { get; }

    public string Name { get; }

    public string Description { get; }

    /// <summary>Иконка</summary>
    public object? TrackIcon
    {
        get
        {
            if (!_trackLoaded)
            {
                var track = GetTrackWithPicture();

                if (track is not null)
                    TrackIconHelper.SetPicture(Rubric.Player, track, icon => TrackIcon = icon);

                _trackLoaded = true;
            }

            return _trackIcon;
        }
        set => SetProperty(ref _trackIcon, value);
    }

    protected abstract Track? GetTrackWithPicture();
}
