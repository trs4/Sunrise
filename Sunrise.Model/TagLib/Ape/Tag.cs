using System.Collections;
using System.Globalization;

namespace Sunrise.Model.TagLib.Ape;

public class Tag : TagLib.Tag, IEnumerable<string>
{
    private static readonly string[] _picture_item_names =
        [
            "Cover Art (other)",
            "Cover Art (icon)",
            "Cover Art (other icon)",
            "Cover Art (front)",
            "Cover Art (back)",
            "Cover Art (leaflet)",
            "Cover Art (media)",
            "Cover Art (lead)",
            "Cover Art (artist)",
            "Cover Art (conductor)",
            "Cover Art (band)",
            "Cover Art (composer)",
            "Cover Art (lyricist)",
            "Cover Art (studio)",
            "Cover Art (recording)",
            "Cover Art (performance)",
            "Cover Art (movie scene)",
            "Cover Art (colored fish)",
            "Cover Art (illustration)",
            "Cover Art (band logo)",
            "Cover Art (publisher logo)",
            "Embedded Object"
        ];

    private Footer _footer;
    private readonly List<Item> _items = [];

    public Tag() { }

    public Tag(File file, long position)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (position < 0 || position > file.Length - Footer.Size)
            throw new ArgumentOutOfRangeException(nameof(position));

        Read(file, position);
    }

    public Tag(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Count < Footer.Size)
            throw new CorruptFileException("Does not contain enough footer data");

        _footer = new Footer(data.Mid((int)(data.Count - Footer.Size)));

        if (_footer.TagSize == 0)
            throw new CorruptFileException("Tag size out of bounds");

        if ((_footer.Flags & FooterFlags.IsHeader) != 0)
            throw new CorruptFileException("Footer was actually header");

        if (data.Count < _footer.TagSize)
            throw new CorruptFileException("Does not contain enough tag data");

        Parse(data.Mid((int)(data.Count - _footer.TagSize), (int)(_footer.TagSize - Footer.Size)));
    }

    public bool HeaderPresent
    {
        get => (_footer.Flags & FooterFlags.HeaderPresent) != 0;
        set
        {
            if (value)
                _footer.Flags |= FooterFlags.HeaderPresent;
            else
                _footer.Flags &= ~FooterFlags.HeaderPresent;
        }
    }

    public void AddValue(string key, uint number, uint count)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (number == 0 && count == 0)
            return;

        if (count != 0)
            AddValue(key, $"{number}/{count}");
        else
            AddValue(key, number.ToString(CultureInfo.InvariantCulture));
    }

    public void SetValue(string key, uint number, uint count)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (number == 0 && count == 0)
            RemoveItem(key);
        else if (count != 0)
            SetValue(key, $"{number}/{count}");
        else
            SetValue(key, number.ToString(CultureInfo.InvariantCulture));
    }

    public void AddValue(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (string.IsNullOrEmpty(value))
            return;

        AddValue(key, new[] { value });
    }

    public void SetValue(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (string.IsNullOrEmpty(value))
            RemoveItem(key);
        else
            SetValue(key, new[] { value });
    }

    public void AddValue(string key, string[] value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (value is null || value.Length == 0)
            return;

        int index = GetItemIndex(key);
        var values = new List<string>();

        if (index >= 0)
            values.AddRange(_items[index].ToStringArray());

        values.AddRange(value);
        var item = new Item(key, values.ToArray());

        if (index >= 0)
            _items[index] = item;
        else
            _items.Add(item);
    }

    public void SetValue(string key, string?[] value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (value is null || value.Length == 0)
        {
            RemoveItem(key);
            return;
        }

        Item item = new Item(key, value);
        int index = GetItemIndex(key);

        if (index >= 0)
            _items[index] = item;
        else
            _items.Add(item);
    }

    public Item? GetItem(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        foreach (Item item in _items)
        {
            if (key.Equals(item.Key, StringComparison.InvariantCultureIgnoreCase))
                return item;
        }

        return null;
    }

    public void SetItem(Item item)
    {
        ArgumentNullException.ThrowIfNull(item);
        int index = GetItemIndex(item.Key);

        if (index >= 0)
            _items[index] = item;
        else
            _items.Add(item);
    }

    public void RemoveItem(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            if (key.Equals(_items[i].Key, StringComparison.InvariantCultureIgnoreCase))
                _items.RemoveAt(i);
        }
    }

    public bool HasItem(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return GetItemIndex(key) >= 0;
    }

    public ByteVector Render()
    {
        ByteVector data = [];
        uint item_count = 0;

        foreach (Item item in _items)
        {
            data.Add(item.Render());
            item_count++;
        }

        _footer.ItemCount = item_count;
        _footer.TagSize = (uint)(data.Count + Footer.Size);
        HeaderPresent = true;

        data.Insert(0, _footer.RenderHeader());
        data.Add(_footer.RenderFooter());
        return data;
    }

    protected void Read(File file, long position)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Mode = File.AccessMode.Read;

        if (position < 0 || position > file.Length - Footer.Size)
            throw new ArgumentOutOfRangeException(nameof(position));

        file.Seek(position);
        _footer = new Footer(file.ReadBlock((int)Footer.Size));

        if (_footer.TagSize == 0)
            throw new CorruptFileException("Tag size out of bounds");

        if ((_footer.Flags & FooterFlags.IsHeader) == 0)
            file.Seek(position + Footer.Size - _footer.TagSize);

        Parse(file.ReadBlock((int)(_footer.TagSize - Footer.Size)));
    }

    protected void Parse(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);
        int pos = 0;

        try
        {
            for (uint i = 0; i < _footer.ItemCount && pos <= data.Count - 11; i++)
            {
                Item item = new Item(data, pos);
                SetItem(item);
                pos += item.Size;
            }
        }
        catch (CorruptFileException) { }
    }

    private int GetItemIndex(string key)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (key.Equals(_items[i].Key, StringComparison.InvariantCultureIgnoreCase))
                return i;
        }

        return -1;
    }

    private string? GetItemAsString(string key) => GetItem(key)?.ToString();

    private string[] GetItemAsStrings(string key)
    {
        Item item = GetItem(key);
        return item is not null ? item.ToStringArray() : [];
    }

    private uint GetItemAsUInt32(string key, int index)
    {
        string text = GetItemAsString(key);

        if (text is null)
            return 0;

        string[] values = text.Split(['/'], index + 2);

        if (values.Length < index + 1)
            return 0;

        if (uint.TryParse(values[index], out var result))
            return result;

        return 0;
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (Item item in _items)
            yield return item.Key;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override TagTypes TagTypes => TagTypes.Ape;

    public override string? Title
    {
        get => GetItemAsString("Title");
        set => SetValue("Title", value);
    }

    public override string? TitleSort
    {
        get => GetItemAsString("TitleSort");
        set => SetValue("TitleSort", value);
    }

    public override string? Subtitle
    {
        get => GetItemAsString("Subtitle");
        set => SetValue("Subtitle", value);
    }

    public override string? Description
    {
        get => GetItemAsString("Description");
        set => SetValue("Description", value);
    }

    public override string?[] Performers
    {
        get => GetItemAsStrings("Artist");
        set => SetValue("Artist", value);
    }

    public override string?[] PerformersSort
    {
        get => GetItemAsStrings("ArtistSort");
        set => SetValue("ArtistSort", value);
    }

    public override string[] PerformersRole
    {
        get => GetItemAsStrings("PerformersRole");
        set => SetValue("PerformersRole", value);
    }

    public override string?[] AlbumArtists
    {
        get
        {
            string[] list = GetItemAsStrings("Album Artist");

            if (list.Length == 0)
                list = GetItemAsStrings("AlbumArtist");

            return list;
        }
        set
        {
            SetValue("Album Artist", value);

            if (HasItem("AlbumArtist"))
                SetValue("AlbumArtist", value);
        }
    }

    public override string?[] AlbumArtistsSort
    {
        get => GetItemAsStrings("AlbumArtistSort");
        set => SetValue("AlbumArtistSort", value);
    }

    public override string?[] Composers
    {
        get => GetItemAsStrings("Composer");
        set => SetValue("Composer", value);
    }

    public override string?[] ComposersSort
    {
        get => GetItemAsStrings("ComposerSort");
        set => SetValue("ComposerSort", value);
    }

    public override string? Album
    {
        get => GetItemAsString("Album");
        set => SetValue("Album", value);
    }

    public override string? AlbumSort
    {
        get => GetItemAsString("AlbumSort");
        set => SetValue("AlbumSort", value);
    }

    public override string? Comment
    {
        get => GetItemAsString("Comment");
        set => SetValue("Comment", value);
    }

    public override string?[] Genres
    {
        get => GetItemAsStrings("Genre");
        set => SetValue("Genre", value);
    }

    public override uint Year
    {
        get
        {
            string text = GetItemAsString("Year");

            if (text is null || text.Length == 0)
                return 0;

            if (uint.TryParse(text, out var value) || (text.Length >= 4 && uint.TryParse(text.AsSpan(0, 4), out value)))
                return value;

            return 0;
        }
        set => SetValue("Year", value, 0);
    }

    public override uint Track
    {
        get => GetItemAsUInt32("Track", 0);
        set => SetValue("Track", value, TrackCount);
    }

    public override uint TrackCount
    {
        get => GetItemAsUInt32("Track", 1);
        set => SetValue("Track", Track, value);
    }

    public override uint Disc
    {
        get => GetItemAsUInt32("Disc", 0);
        set => SetValue("Disc", value, DiscCount);
    }

    public override uint DiscCount
    {
        get => GetItemAsUInt32("Disc", 1);
        set => SetValue("Disc", Disc, value);
    }

    public override string? Lyrics
    {
        get => GetItemAsString("Lyrics");
        set => SetValue("Lyrics", value);
    }

    public override string? Grouping
    {
        get => GetItemAsString("Grouping");
        set => SetValue("Grouping", value);
    }

    public override uint BeatsPerMinute
    {
        get
        {
            string text = GetItemAsString("BPM");

            if (text is not null && double.TryParse(text, out var value))
                return (uint)Math.Round(value);

            return 0;
        }
        set => SetValue("BPM", value, 0);
    }

    public override string? Conductor
    {
        get => GetItemAsString("Conductor");
        set => SetValue("Conductor", value);
    }

    public override string? Copyright
    {
        get => GetItemAsString("Copyright");
        set => SetValue("Copyright", value);
    }

    public override DateTime? DateTagged
    {
        get
        {
            string value = GetItemAsString("DateTagged");
            return value is not null && DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out var date) ? date : null;
        }
        set => SetValue("DateTagged", value is null ? null : $"{value:yyyy-MM-dd HH:mm:ss}");
    }

    public override string? MusicBrainzArtistId
    {
        get => GetItemAsString("MUSICBRAINZ_ARTISTID");
        set => SetValue("MUSICBRAINZ_ARTISTID", value);
    }

    public override string? MusicBrainzReleaseGroupId
    {
        get => GetItemAsString("MUSICBRAINZ_RELEASEGROUPID");
        set => SetValue("MUSICBRAINZ_RELEASEGROUPID", value);
    }

    public override string? MusicBrainzReleaseId
    {
        get => GetItemAsString("MUSICBRAINZ_ALBUMID");
        set => SetValue("MUSICBRAINZ_ALBUMID", value);
    }

    public override string? MusicBrainzReleaseArtistId
    {
        get => GetItemAsString("MUSICBRAINZ_ALBUMARTISTID");
        set => SetValue("MUSICBRAINZ_ALBUMARTISTID", value);
    }

    public override string? MusicBrainzTrackId
    {
        get => GetItemAsString("MUSICBRAINZ_TRACKID");
        set => SetValue("MUSICBRAINZ_TRACKID", value);
    }

    public override string? MusicBrainzDiscId
    {
        get => GetItemAsString("MUSICBRAINZ_DISCID");
        set => SetValue("MUSICBRAINZ_DISCID", value);
    }

    public override string? MusicIpId
    {
        get => GetItemAsString("MUSICIP_PUID");
        set => SetValue("MUSICIP_PUID", value);
    }

    public override string? AmazonId
    {
        get => GetItemAsString("ASIN");
        set => SetValue("ASIN", value);
    }

    public override string? MusicBrainzReleaseStatus
    {
        get => GetItemAsString("MUSICBRAINZ_ALBUMSTATUS");
        set => SetValue("MUSICBRAINZ_ALBUMSTATUS", value);
    }

    public override string? MusicBrainzReleaseType
    {
        get => GetItemAsString("MUSICBRAINZ_ALBUMTYPE");
        set => SetValue("MUSICBRAINZ_ALBUMTYPE", value);
    }

    public override string? MusicBrainzReleaseCountry
    {
        get => GetItemAsString("RELEASECOUNTRY");
        set => SetValue("RELEASECOUNTRY", value);
    }

    public override double ReplayGainTrackGain
    {
        get
        {
            string text = GetItemAsString("REPLAYGAIN_TRACK_GAIN");

            if (text is null)
                return double.NaN;
            
            if (text.EndsWith("db", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(0, text.Length - 2).Trim();

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;
            
            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                RemoveItem("REPLAYGAIN_TRACK_GAIN");
            else
            {
                string text = value.ToString("0.00 dB", CultureInfo.InvariantCulture);
                SetValue("REPLAYGAIN_TRACK_GAIN", text);
            }
        }
    }

    public override double ReplayGainTrackPeak
    {
        get
        {
            string text;

            if ((text = GetItemAsString("REPLAYGAIN_TRACK_PEAK")) != null && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;
            
            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                RemoveItem("REPLAYGAIN_TRACK_PEAK");
            else
            {
                string text = value.ToString("0.000000", CultureInfo.InvariantCulture);
                SetValue("REPLAYGAIN_TRACK_PEAK", text);
            }
        }
    }

    public override double ReplayGainAlbumGain
    {
        get
        {
            string text = GetItemAsString("REPLAYGAIN_ALBUM_GAIN");

            if (text is null)
                return double.NaN;
            
            if (text.EndsWith("db", StringComparison.OrdinalIgnoreCase))
                text = text.Substring(0, text.Length - 2).Trim();

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;
            
            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                RemoveItem("REPLAYGAIN_ALBUM_GAIN");
            else
            {
                string text = value.ToString("0.00 dB", CultureInfo.InvariantCulture);
                SetValue("REPLAYGAIN_ALBUM_GAIN", text);
            }
        }
    }

    public override double ReplayGainAlbumPeak
    {
        get
        {
            string text;

            if ((text = GetItemAsString("REPLAYGAIN_ALBUM_PEAK")) != null && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;
            
            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                RemoveItem("REPLAYGAIN_ALBUM_PEAK");
            else
            {
                string text = value.ToString("0.000000", CultureInfo.InvariantCulture);
                SetValue("REPLAYGAIN_ALBUM_PEAK", text);
            }
        }
    }

    public override IPicture[] Pictures
    {
        get
        {
            var pictures = new List<IPicture>();
            var comparison = StringComparison.InvariantCultureIgnoreCase;

            foreach (Item item in _items)
            {
                if (item is null || item.Type != ItemType.Binary)
                    continue;

                int i;

                for (i = 0; i < _picture_item_names.Length; i++)
                {
                    if (_picture_item_names[i].Equals(item.Key, comparison))
                        break;
                }

                if (i >= _picture_item_names.Length || item.Value is null)
                    continue;

                int index = item.Value.Find(ByteVector.TextDelimiter(StringType.UTF8));

                if (index < 0)
                    continue;

                var pic = new Picture(item.Value.Mid(index + 1))
                {
                    Description = item.Value.ToString(StringType.UTF8, 0, index),
                    Type = i < _picture_item_names.Length - 1 ? (PictureType)i : PictureType.NotAPicture,
                };

                pictures.Add(pic);
            }

            return [.. pictures];
        }
        set
        {
            foreach (string item_name in _picture_item_names)
                RemoveItem(item_name);

            if (value is null || value.Length == 0)
                return;

            foreach (IPicture pic in value)
            {
                int type = (int)pic.Type;

                if (type >= _picture_item_names.Length)
                    type = _picture_item_names.Length - 1;

                string name = _picture_item_names[type];

                if (GetItem(name) is not null)
                    continue;

                var data = ByteVector.FromString(pic.Description, StringType.UTF8);
                data.Add(ByteVector.TextDelimiter(StringType.UTF8));
                data.Add(pic.Data);
                SetItem(new Item(name, data));
            }
        }
    }

    public override bool IsEmpty => _items.Count == 0;

    public override void Clear() => _items.Clear();

    public override void CopyTo(TagLib.Tag target, bool overwrite)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target is not Tag match)
        {
            base.CopyTo(target, overwrite);
            return;
        }

        foreach (Item item in _items)
        {
            if (!overwrite && match.GetItem(item.Key) is not null)
                continue;

            match._items.Add(item.Clone());
        }
    }

}
