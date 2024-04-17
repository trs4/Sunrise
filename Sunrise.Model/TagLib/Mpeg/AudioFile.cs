namespace Sunrise.Model.TagLib.Mpeg;

[SupportedMimeType("taglib/mp3", "mp3")]
[SupportedMimeType("audio/x-mp3")]
[SupportedMimeType("application/x-id3")]
[SupportedMimeType("audio/mpeg")]
[SupportedMimeType("audio/x-mpeg")]
[SupportedMimeType("audio/x-mpeg-3")]
[SupportedMimeType("audio/mpeg3")]
[SupportedMimeType("audio/mp3")]
[SupportedMimeType("taglib/m2a", "m2a")]
[SupportedMimeType("taglib/mp2", "mp2")]
[SupportedMimeType("taglib/mp1", "mp1")]
[SupportedMimeType("audio/x-mp2")]
[SupportedMimeType("audio/x-mp1")]
public class AudioFile : NonContainer.File
{
    private AudioHeader _firstHeader;

    public AudioFile(string path, ReadStyle propertiesStyle) : base(path, propertiesStyle) { }

    public AudioFile(string path) : base(path) { }

    public AudioFile(IFileAbstraction abstraction, ReadStyle propertiesStyle) : base(abstraction, propertiesStyle) { }

    public AudioFile(IFileAbstraction abstraction) : base(abstraction) { }

    public static bool CreateID3Tags { get; set; } = true;

    public override Tag? GetTag(TagTypes type, bool create)
    {
        Tag t = (Tag as NonContainer.Tag)?.GetTag(type);

        if (t is not null || !create)
            return t;

        return type switch
        {
            TagTypes.Id3v1 => EndTag.AddTag(type, Tag),
            TagTypes.Id3v2 => StartTag.AddTag(type, Tag),
            TagTypes.Ape => EndTag.AddTag(type, Tag),
            _ => null,
        };
    }

    protected override void ReadStart(long start, ReadStyle propertiesStyle)
    {
        if ((propertiesStyle & ReadStyle.Average) != 0 && !AudioHeader.Find(out _firstHeader, this, start, 0x4000))
            throw new CorruptFileException("MPEG audio header not found");
    }

    protected override void ReadEnd(long end, ReadStyle propertiesStyle)
    {
        GetTag(TagTypes.Id3v1, CreateID3Tags);
        GetTag(TagTypes.Id3v2, CreateID3Tags);
    }

    protected override Properties ReadProperties(long start, long end, ReadStyle propertiesStyle)
    {
        _firstHeader.SetStreamLength(end - start);
        return new Properties(TimeSpan.Zero, _firstHeader);
    }

}
