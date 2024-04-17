namespace Sunrise.Model.TagLib.NonContainer;

public class StartTag : CombinedTag
{
    private readonly TagLib.File _file;
    private readonly int _readSize = (int)Math.Max(Ape.Footer.Size, Id3v2.Header.Size);

    public StartTag(TagLib.File file)
        => _file = file;

    public long TotalSize
    {
        get
        {
            long size = 0;
            while (ReadTagInfo(ref size) != TagTypes.None) ;
            return size;
        }
    }

    public long Read(ReadStyle style)
    {
        TagLib.Tag tag;
        ClearTags();
        long end = 0;

        while ((tag = ReadTag(ref end, style)) is not null)
            AddTag(tag);

        return end;
    }

    public ByteVector Render()
    {
        var data = new ByteVector();

        foreach (TagLib.Tag t in Tags)
        {
            if (t is Ape.Tag aTag)
                data.Add(aTag.Render());
            else if (t is Id3v2.Tag id3v2Tag)
                data.Add(id3v2Tag.Render());
        }

        return data;
    }

    public long Write()
    {
        ByteVector data = Render();
        _file.Insert(data, 0, TotalSize);
        return data.Count;
    }

    public void RemoveTags(TagTypes types)
    {
        for (int i = Tags.Length - 1; i >= 0; i--)
        {
            var tag = Tags[i];

            if (types == TagTypes.AllTags || (tag.TagTypes & types) == tag.TagTypes)
                RemoveTag(tag);
        }
    }

    public TagLib.Tag? AddTag(TagTypes type, TagLib.Tag copy)
    {
        TagLib.Tag tag = null;

        if (type == TagTypes.Id3v2)
            tag = new Id3v2.Tag();
        else if (type == TagTypes.Ape)
        {
            tag = new Ape.Tag();
            ((Ape.Tag)tag).HeaderPresent = true;
        }

        if (tag is not null)
        {
            copy?.CopyTo(tag, true);
            AddTag(tag);
        }

        return tag;
    }

    private TagLib.Tag? ReadTag(ref long start, ReadStyle style)
    {
        long end = start;
        TagTypes type = ReadTagInfo(ref end);
        TagLib.Tag tag = null;

        switch (type)
        {
            case TagTypes.Ape:
                tag = new Ape.Tag(_file, start);
                break;
            case TagTypes.Id3v2:
                tag = new Id3v2.Tag(_file, start, style);
                break;
        }

        start = end;
        return tag;
    }

    private TagTypes ReadTagInfo(ref long position)
    {
        _file.Seek(position);
        ByteVector data = _file.ReadBlock(_readSize);

        try
        {
            if (data.StartsWith(Ape.Footer.FileIdentifier))
            {
                var footer = new Ape.Footer(data);
                position += footer.CompleteTagSize;
                return TagTypes.Ape;
            }

            if (data.StartsWith(Id3v2.Header.FileIdentifier))
            {
                var header = new Id3v2.Header(data);
                position += header.CompleteTagSize;
                return TagTypes.Id3v2;
            }
        }
        catch (CorruptFileException) { }

        return TagTypes.None;
    }

}
