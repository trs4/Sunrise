using System;
using Sunrise.Model;

namespace Sunrise.ViewModels;

public abstract class TrackSourceViewModel : RubricViewModel
{
    private object? _icon;
    private bool _iconLoaded;

    protected TrackSourceViewModel(RubricViewModel rubric, string name, string description)
        : base(rubric.Player, null, name)
    {
        Rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));
        Description = description;
    }

    public RubricViewModel Rubric { get; }

    public string Description { get; }

    /// <summary>Иконка</summary>
    public sealed override object? Icon
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

    public override bool IsDependent => true;

    protected abstract Track? GetTrackWithPicture();
}
