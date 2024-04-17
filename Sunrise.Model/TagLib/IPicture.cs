namespace Sunrise.Model.TagLib;

public interface IPicture
{
    string MimeType { get; set; }

    PictureType Type { get; set; }

    string Filename { get; set; }

    string? Description { get; set; }

    ByteVector Data { get; set; }
}
