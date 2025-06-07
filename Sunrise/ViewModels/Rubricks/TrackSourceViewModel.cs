using System;

namespace Sunrise.ViewModels;

public abstract class TrackSourceViewModel
{
    protected TrackSourceViewModel(RubricViewModel rubric)
        => Rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));

    public RubricViewModel Rubric { get; }
}
