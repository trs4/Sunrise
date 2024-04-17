using System.Globalization;

namespace Sunrise.Model.TagLib.Id3v1;

public class Tag : TagLib.Tag
{
    private string? _title;
    private string? _artist;
    private string? _album;
    private string? _year;
    private string? _comment;
    private byte _track;
    private byte _genre;

    public const uint Size = 128;
    public static readonly ReadOnlyByteVector FileIdentifier = "TAG";

    public Tag() => Clear();

    public Tag(File file, long position)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Mode = File.AccessMode.Read;

        if (position < 0 || position > file.Length - Size)
            throw new ArgumentOutOfRangeException(nameof(position));

        file.Seek(position);
        ByteVector data = file.ReadBlock((int)Size);

        if (!data.StartsWith(FileIdentifier))
            throw new CorruptFileException("ID3v1 data does not start with identifier.");

        Parse(data);
    }

    public Tag(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Count < Size)
            throw new CorruptFileException("ID3v1 data is less than 128 bytes long.");

        if (!data.StartsWith(FileIdentifier))
            throw new CorruptFileException("ID3v1 data does not start with identifier.");

        Parse(data);
    }

    public ByteVector Render() => new ByteVector
    {
        FileIdentifier,
        DefaultStringHandler.Render (_title).Resize (30),
        DefaultStringHandler.Render (_artist).Resize (30),
        DefaultStringHandler.Render (_album).Resize (30),
        DefaultStringHandler.Render (_year).Resize (4),
        DefaultStringHandler.Render (_comment).Resize (28),
        0,
        _track,
        _genre,
    };

    public static StringHandler DefaultStringHandler { get; set; } = new();

    private void Parse(ByteVector data)
    {
        _title = DefaultStringHandler.Parse(data.Mid(3, 30));
        _artist = DefaultStringHandler.Parse(data.Mid(33, 30));
        _album = DefaultStringHandler.Parse(data.Mid(63, 30));
        _year = DefaultStringHandler.Parse(data.Mid(93, 4));

        if (data[125] == 0 && data[126] != 0) // ID3v1.1 detected
        {
            _comment = DefaultStringHandler.Parse(data.Mid(97, 28));
            _track = data[126];
        }
        else
            _comment = DefaultStringHandler.Parse(data.Mid(97, 30));

        _genre = data[127];
    }

    public override TagTypes TagTypes => TagTypes.Id3v1;

    public override string? Title
    {
        get => string.IsNullOrEmpty(_title) ? null : _title;
        set => _title = value is not null ? value.Trim() : string.Empty;
    }

    public override string?[] Performers
    {
        get => string.IsNullOrEmpty(_artist) ? [] : _artist.Split(';');
        set => _artist = value is not null ? string.Join(";", value) : string.Empty;
    }

    public override string? Album
    {
        get => string.IsNullOrEmpty(_album) ? null : _album;
        set => _album = value is not null ? value.Trim() : string.Empty;
    }

    public override string? Comment
    {
        get => string.IsNullOrEmpty(_comment) ? null : _comment;
        set => _comment = value is not null ? value.Trim() : string.Empty;
    }

    public override string?[] Genres
    {
        get
        {
            string genre_name = TagLib.Genres.IndexToAudio(_genre);
            return genre_name is not null ? [genre_name] : [];
        }
        set
        {
            string? str = value?.FirstOrDefault();
            _genre = str is null ? (byte)255 : TagLib.Genres.AudioToIndex(str.Trim());
        }
    }

    public override uint Year
    {
        get => uint.TryParse(_year, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
        set => _year = (value > 0 && value < 10000) ? value.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }

    public override uint Track
    {
        get => _track;
        set => _track = (byte)(value < 256 ? value : 0);
    }

    public override void Clear()
    {
        _title = _artist = _album = _year = _comment = null;
        _track = 0;
        _genre = 255;
    }

}
