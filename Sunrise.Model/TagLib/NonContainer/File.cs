namespace Sunrise.Model.TagLib.NonContainer;

public abstract class File : TagLib.File
{
    private Tag _tag;
    private Properties? _properties;

    protected File(string path, ReadStyle propertiesStyle)
        : base(path)
        => Read(propertiesStyle);

    protected File(string path) : this(path, ReadStyle.Average) { }

    protected File(IFileAbstraction abstraction, ReadStyle propertiesStyle)
        : base(abstraction)
        => Read(propertiesStyle);

    protected File(IFileAbstraction abstraction) : this(abstraction, ReadStyle.Average) { }

    public override TagLib.Tag Tag => _tag;

    public override Properties? Properties => _properties;

    public override void Save()
    {
        PreSave();
        Mode = AccessMode.Write;

        try
        {
            _tag.Write(out var start, out var end);
            InvariantStartPosition = start;
            InvariantEndPosition = end;
            TagTypesOnDisk = TagTypes;
        }
        finally
        {
            Mode = AccessMode.Closed;
        }
    }

    public override void RemoveTags(TagTypes types) => _tag.RemoveTags(types);

    protected StartTag StartTag => _tag.StartTag;

    protected EndTag EndTag => _tag.EndTag;

    protected virtual void ReadStart(long start, ReadStyle propertiesStyle) { }

    protected virtual void ReadEnd(long end, ReadStyle propertiesStyle) { }

    protected abstract Properties ReadProperties(long start, long end, ReadStyle propertiesStyle);

    private void Read(ReadStyle propertiesStyle)
    {
        Mode = AccessMode.Read;

        try
        {
            _tag = new Tag(this);
            InvariantStartPosition = _tag.ReadStart(propertiesStyle);
            TagTypesOnDisk |= StartTag.TagTypes;
            ReadStart(InvariantStartPosition, propertiesStyle);
            InvariantEndPosition = InvariantStartPosition == Length ? Length : _tag.ReadEnd(propertiesStyle);
            TagTypesOnDisk |= EndTag.TagTypes;
            ReadEnd(InvariantEndPosition, propertiesStyle);
            _properties = (propertiesStyle & ReadStyle.Average) != 0 ? ReadProperties(InvariantStartPosition, InvariantEndPosition, propertiesStyle) : null;
        }
        finally
        {
            Mode = AccessMode.Closed;
        }
    }

}
