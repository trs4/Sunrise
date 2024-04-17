namespace Sunrise.Model.TagLib.NonContainer;

public class Tag : CombinedTag
{
    public Tag(File file)
    {
        StartTag = new StartTag(file);
        EndTag = new EndTag(file);
        AddTag(StartTag);
        AddTag(EndTag);
    }

    public StartTag StartTag { get; private set; }

    public EndTag EndTag { get; private set; }

    public override TagTypes TagTypes => StartTag.TagTypes | EndTag.TagTypes;

    public override TagLib.Tag[] Tags => [.. StartTag.Tags, .. EndTag.Tags];

    public TagLib.Tag? GetTag(TagTypes type)
    {
        foreach (TagLib.Tag t in Tags)
        {
            if (type == TagTypes.Id3v1 && t is Id3v1.Tag)
                return t;

            if (type == TagTypes.Id3v2 && t is Id3v2.Tag)
                return t;

            if (type == TagTypes.Ape && t is Ape.Tag)
                return t;
        }

        return null;
    }

    public void RemoveTags(TagTypes types)
    {
        StartTag.RemoveTags(types);
        EndTag.RemoveTags(types);
    }

    public void Read(out long start, out long end)
    {
        start = ReadStart(ReadStyle.None);
        end = ReadEnd(ReadStyle.None);
    }

    public long ReadStart(ReadStyle style) => StartTag.Read(style);

    public long ReadEnd(ReadStyle style) => EndTag.Read(style);

    public void Write(out long start, out long end)
    {
        start = StartTag.Write();
        end = EndTag.Write();
    }

}
