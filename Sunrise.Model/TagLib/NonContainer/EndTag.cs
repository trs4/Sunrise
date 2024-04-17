namespace Sunrise.Model.TagLib.NonContainer;

public class EndTag : CombinedTag
{
    private static readonly int _readSize = (int)Math.Max(Math.Max(Ape.Footer.Size, Id3v2.Footer.Size), Id3v1.Tag.Size);

    private readonly TagLib.File _file;

    public EndTag(TagLib.File file)
        => _file = file;

    public long TotalSize
    {
        get
        {
            long start = _file.Length;
            while (ReadTagInfo(ref start) != TagTypes.None) ;
            return _file.Length - start;
        }
    }

    public long Read(ReadStyle style)
    {
        TagLib.Tag tag;
        ClearTags();
        long start = _file.Length;

        while ((tag = ReadTag(ref start, style)) is not null)
            InsertTag(0, tag);

        return start;
    }

    public ByteVector Render()
    {
        var data = new ByteVector();

        foreach (TagLib.Tag t in Tags)
        {
            if (t is Ape.Tag tag)
                data.Add(tag.Render());
            else if (t is Id3v2.Tag tag1)
                data.Add(tag1.Render());
            else if (t is Id3v1.Tag tag2)
                data.Add(tag2.Render());
        }

        return data;
    }

    public long Write()
    {
        long total_size = TotalSize;
        ByteVector data = Render();
        _file.Insert(data, _file.Length - total_size, total_size);
        return _file.Length - data.Count;
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

        if (type == TagTypes.Id3v1)
            tag = new Id3v1.Tag();
        else if (type == TagTypes.Id3v2)
        {
            Id3v2.Tag tag32 = new Id3v2.Tag { Version = 4 };
            tag32.Flags |= Id3v2.HeaderFlags.FooterPresent;
            tag = tag32;
        }
        else if (type == TagTypes.Ape)
            tag = new Ape.Tag();

        if (tag is not null)
        {
            copy?.CopyTo(tag, true);

            if (type == TagTypes.Id3v1)
                AddTag(tag);
            else
                InsertTag(0, tag);
        }

        return tag;
    }

    private TagLib.Tag? ReadTag(ref long end, ReadStyle style)
    {
        long start = end;
        TagTypes type = ReadTagInfo(ref start);
        TagLib.Tag tag = null;

        try
        {
            switch (type)
            {
                case TagTypes.Ape:
                    tag = new Ape.Tag(_file, end - Ape.Footer.Size);
                    break;
                case TagTypes.Id3v2:
                    tag = new Id3v2.Tag(_file, start, style);
                    break;
                case TagTypes.Id3v1:
                    tag = new Id3v1.Tag(_file, start);
                    break;
            }

            end = start;
        }
        catch (CorruptFileException) { }

        return tag;
    }

    private TagTypes ReadTagInfo(ref long position)
    {
        if (position - _readSize < 0)
            return TagTypes.None;

        _file.Seek(position - _readSize);
        ByteVector data = _file.ReadBlock(_readSize);

        try
        {
            int offset = (int)(data.Count - Ape.Footer.Size);

            if (data.ContainsAt(Ape.Footer.FileIdentifier, offset))
            {
                Ape.Footer footer = new Ape.Footer(data.Mid(offset));

                if (footer.CompleteTagSize == 0 || (footer.Flags & Ape.FooterFlags.IsHeader) != 0)
                    return TagTypes.None;

                position -= footer.CompleteTagSize;
                return TagTypes.Ape;
            }

            offset = (int)(data.Count - Id3v2.Footer.Size);

            if (data.ContainsAt(Id3v2.Footer.FileIdentifier, offset))
            {
                var footer = new Id3v2.Footer(data.Mid(offset));

                position -= footer.CompleteTagSize;
                return TagTypes.Id3v2;
            }

            if (data.StartsWith(Id3v1.Tag.FileIdentifier))
            {
                position -= Id3v1.Tag.Size;
                return TagTypes.Id3v1;
            }
        }
        catch (CorruptFileException) { }

        return TagTypes.None;
    }

}
