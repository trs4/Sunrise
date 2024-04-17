namespace Sunrise.Model.TagLib;

public interface IPhotoCodec : ICodec
{
    int PhotoWidth { get; }

    int PhotoHeight { get; }

    int PhotoQuality { get; }
}
