namespace Sunrise.Model;

/// <summary>Воспроизведение трека в приложении</summary>
public sealed class TrackReproduced
{
    public TrackReproduced() { }

    public TrackReproduced(int appId, int trackId, int reproduced)
    {
        AppId = appId;
        TrackId = trackId;
        Reproduced = reproduced;
    }

    /// <summary>Идентификатор приложения</summary>
    public int AppId { get; set; }

    /// <summary>Идентификатор трека</summary>
    public int TrackId { get; set; }

    /// <summary>Воспроизведено</summary>
    public int Reproduced { get; set; }

    public override string ToString() => $"{AppId}-{TrackId}: {Reproduced}";
}
