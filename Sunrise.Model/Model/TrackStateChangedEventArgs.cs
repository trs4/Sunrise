using Sunrise.Model.SoundFlow.Enums;

namespace Sunrise.Model;

public sealed class TrackStateChangedEventArgs : EventArgs
{
    /// <summary>Оповещает об изменении трека</summary>
    /// <param name="track">Трек</param>
    /// <param name="state">Статус</param>
    /// <param name="position">Текущая позиция от 0 до 1</param>
    internal TrackStateChangedEventArgs(Track track, PlaybackState state, double position)
    {
        Track = track;
        State = state;
        Position = position;
    }

    public Track Track { get; }

    public PlaybackState State { get; }

    public double Position { get; }
}

public delegate void TrackStateChangedEventHandler(object? sender, TrackStateChangedEventArgs e);
