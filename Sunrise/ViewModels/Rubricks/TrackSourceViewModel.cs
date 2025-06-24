using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public abstract class TrackSourceViewModel : ObservableObject
{
    private object? _icon;
    private bool _iconLoaded;

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
    public object? Icon
    {
        get
        {
            if (!_iconLoaded)
            {
                var track = GetTrackWithPicture();

                if (track is not null)
                    TrackIconHelper.SetPicture(Rubric.Player, track, icon => Icon = icon);

                _iconLoaded = true;
            }

            return _icon;
        }
        set => SetProperty(ref _icon, value);
    }

    protected abstract Track? GetTrackWithPicture();
}
