namespace Sunrise.Model.SoundFlow.Metadata.Models;

public sealed class AlbumArt
{
    internal AlbumArt(byte[] data) : this("image/jpeg", data) { }

    internal AlbumArt(string mimeType, byte[] data)
    {
        MimeType = mimeType;
        Data = data;
    }

    public string MimeType { get; }

    public byte[] Data { get; }

    public override string ToString() => MimeType;
}
