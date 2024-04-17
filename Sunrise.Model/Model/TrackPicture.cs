namespace Sunrise.Model;

/// <summary>Рисунок трека</summary>
public class TrackPicture
{
    /// <summary>Идентификатор</summary>
    public int Id { get; set; }

    /// <summary>Тип</summary>
    public string MimeType { get; set; }

    /// <summary>Данные</summary>
    public byte[] Data { get; set; }

    public override string ToString() => $"{Id} {MimeType}";
}
