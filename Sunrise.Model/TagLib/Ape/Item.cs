namespace Sunrise.Model.TagLib.Ape;

public class Item : ICloneable
{
    private ReadOnlyByteVector _data;
    private string[] _text;

    public Item(ByteVector data, int offset)
    {
        ArgumentNullException.ThrowIfNull(data);
        Parse(data, offset);
    }

    public Item(string key, string value)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        ArgumentNullException.ThrowIfNull(value);
        _text = [value];
    }

    public Item(string key, params string?[] value)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        ArgumentNullException.ThrowIfNull(value);
        _text = (string[])value.Clone();
    }

    public Item(string key, ByteVector value)
    {
        Key = key;
        Type = ItemType.Binary;
        _data = (value as ReadOnlyByteVector) ?? new ReadOnlyByteVector(value);
    }

    private Item(Item item)
    {
        Type = item.Type;
        Key = item.Key;

        if (item._data is not null)
            _data = new ReadOnlyByteVector(item._data);

        if (item._text != null)
            _text = (string[])item._text.Clone();

        ReadOnly = item.ReadOnly;
        Size = item.Size;
    }

    public string Key { get; private set; }

    public ByteVector? Value => (Type == ItemType.Binary) ? _data : null;

    public int Size { get; private set; }

    public ItemType Type { get; set; } = ItemType.Text;

    public bool ReadOnly { get; set; }

    public bool IsEmpty
    {
        get
        {
            if (Type != ItemType.Binary)
                return _text == null || _text.Length == 0;
            else
                return _data is null || _data.IsEmpty;
        }
    }

    public override string ToString() => Type == ItemType.Binary || _text is null ? string.Empty : string.Join(", ", _text);

    public string[] ToStringArray() => Type == ItemType.Binary || _text is null ? [] : _text;

    public ByteVector Render()
    {
        uint flags = (uint)((ReadOnly) ? 1 : 0) |
            ((uint)Type << 1);

        if (IsEmpty)
            return [];

        ByteVector result = null;

        if (Type == ItemType.Binary)
        {
            if (_text == null && _data is not null)
                result = _data;
        }

        if (result is null && _text is not null)
        {
            result = [];

            for (int i = 0; i < _text.Length; i++)
            {
                if (i != 0)
                    result.Add(0);

                result.Add(ByteVector.FromString(_text[i], StringType.UTF8));
            }
        }

        if (result is null || result.Count == 0)
            return [];

        var output = new ByteVector
        {
            ByteVector.FromUInt((uint)result.Count, false),
            ByteVector.FromUInt(flags, false),
            ByteVector.FromString(Key, StringType.UTF8),
            0,
            result,
        };

        Size = output.Count;
        return output;
    }

    protected void Parse(ByteVector data, int offset)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);

        if (data.Count < offset + 11)
            throw new CorruptFileException("Not enough data for APE Item");

        uint value_length = data.Mid(offset, 4).ToUInt(false);
        uint flags = data.Mid(offset + 4, 4).ToUInt(false);

        ReadOnly = (flags & 1) == 1;
        Type = (ItemType)((flags >> 1) & 3);

        int pos = data.Find(ByteVector.TextDelimiter(StringType.UTF8), offset + 8);

        Key = data.ToString(StringType.UTF8, offset + 8, pos - offset - 8);

        if (value_length > data.Count - pos - 1)
            throw new CorruptFileException("Invalid data length.");

        Size = pos + 1 + (int)value_length - offset;

        if (Type == ItemType.Binary)
            _data = new ReadOnlyByteVector(data.Mid(pos + 1, (int)value_length));
        else
            _text = data.Mid(pos + 1, (int)value_length).ToStrings(StringType.UTF8, 0);
    }

    public Item Clone() => new Item(this);

    object ICloneable.Clone() => Clone();
}
