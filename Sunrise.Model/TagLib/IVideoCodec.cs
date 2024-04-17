namespace Sunrise.Model.TagLib;

public interface IVideoCodec : ICodec
{
    int VideoWidth { get; }

    int VideoHeight { get; }
}
