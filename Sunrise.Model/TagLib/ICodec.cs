namespace Sunrise.Model.TagLib;

public interface ICodec
{
    TimeSpan Duration { get; }

    MediaTypes MediaTypes { get; }

    string Description { get; }
}
