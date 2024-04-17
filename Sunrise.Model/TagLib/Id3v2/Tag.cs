using System.Collections;
using System.Globalization;
using System.Text;

namespace Sunrise.Model.TagLib.Id3v2;

public class Tag : TagLib.Tag, IEnumerable<Frame>, ICloneable
{
    private static string _language = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
    private static byte _default_version = 3;

    private Header _header;
    private ExtendedHeader _extendedHeader;
    private readonly List<Frame> _frameList = [];
    private string[] _performersRole;

    public Tag() { }

    public Tag(File file, long position, ReadStyle style)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Mode = File.AccessMode.Read;

        if (position < 0 || position > file.Length - Header.Size)
            throw new ArgumentOutOfRangeException(nameof(position));

        Read(file, position, style);
    }

    public Tag(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Count < Header.Size)
            throw new CorruptFileException("Does not contain enough header data.");

        _header = new Header(data);

        if (_header.TagSize == 0)
            return;

        if (data.Count - Header.Size < _header.TagSize)
            throw new CorruptFileException("Does not contain enough tag data.");

        Parse(data.Mid((int)Header.Size, (int)_header.TagSize), null, 0, ReadStyle.None);
    }

    public string? GetTextAsString(ByteVector ident)
    {
        Frame frame = ident[0] == 'W' ? UrlLinkFrame.Get(this, ident, false) : TextInformationFrame.Get(this, ident, false);
        string result = frame?.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }

    public IEnumerable<Frame> GetFrames() => _frameList;

    public IEnumerable<Frame> GetFrames(ByteVector ident)
    {
        ArgumentNullException.ThrowIfNull(ident);

        if (ident.Count != 4)
            throw new ArgumentException("Identifier must be four bytes long.", nameof(ident));

        foreach (Frame f in _frameList)
        {
            if (f.FrameId?.Equals(ident) ?? false)
                yield return f;
        }
    }

    public IEnumerable<T> GetFrames<T>()
        where T : Frame
    {
        foreach (Frame f in _frameList)
        {
            if (f is T tf)
                yield return tf;
        }
    }

    public IEnumerable<T> GetFrames<T>(ByteVector ident)
        where T : Frame
    {
        ArgumentNullException.ThrowIfNull(ident);

        if (ident.Count != 4)
            throw new ArgumentException("Identifier must be four bytes long.", nameof(ident));

        foreach (Frame f in _frameList)
        {
            if (f is T tf && (f.FrameId?.Equals(ident) ?? false))
                yield return tf;
        }
    }

    public void AddFrame(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _frameList.Add(frame);
    }

    public void ReplaceFrame(Frame oldFrame, Frame newFrame)
    {
        ArgumentNullException.ThrowIfNull(oldFrame);
        ArgumentNullException.ThrowIfNull(newFrame);

        if (oldFrame == newFrame)
            return;

        int i = _frameList.IndexOf(oldFrame);

        if (i >= 0)
            _frameList[i] = newFrame;
        else
            _frameList.Add(newFrame);
    }

    public void RemoveFrame(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _frameList.Remove(frame);
    }

    public void RemoveFrames(ByteVector ident)
    {
        ArgumentNullException.ThrowIfNull(ident);

        if (ident.Count != 4)
            throw new ArgumentException("Identifier must be four bytes long", nameof(ident));

        for (int i = _frameList.Count - 1; i >= 0; i--)
        {
            if (_frameList[i].FrameId?.Equals(ident) ?? false)
                _frameList.RemoveAt(i);
        }
    }

    public void SetTextFrame(ByteVector ident, params string?[] text)
    {
        ArgumentNullException.ThrowIfNull(ident);

        if (ident.Count != 4)
            throw new ArgumentException("Identifier must be four bytes long", nameof(ident));

        bool empty = true;

        if (text is not null)
        {
            for (int i = 0; empty && i < text.Length; i++)
            {
                if (!string.IsNullOrEmpty(text[i]))
                    empty = false;
            }
        }

        if (empty)
        {
            RemoveFrames(ident);
            return;
        }

        if (ident[0] == 'W')
        {
            var urlFrame = UrlLinkFrame.Get(this, ident, true);

            if (urlFrame is not null)
            {
                urlFrame.Text = text ?? [];
                urlFrame.TextEncoding = DefaultEncoding;
            }

            return;
        }

        var frame = TextInformationFrame.Get(this, ident, true);

        if (frame is not null)
        {
            frame.Text = text ?? [];
            frame.TextEncoding = DefaultEncoding;
        }
    }

    public void SetNumberFrame(ByteVector ident, uint number, uint count, string format = "0")
    {
        ArgumentNullException.ThrowIfNull(ident);

        if (ident.Count != 4)
            throw new ArgumentException("Identifier must be four bytes long.", nameof(ident));

        if (number == 0 && count == 0)
            RemoveFrames(ident);
        else if (count != 0)
            SetTextFrame(ident, string.Format(CultureInfo.InvariantCulture, "{0:" + format + "}/{1}", number, count));
        else
            SetTextFrame(ident, number.ToString(format, CultureInfo.InvariantCulture));
    }

    public ByteVector Render()
    {
        string[] ret = null;

        if (_performersRole is not null)
        {
            var map = new Dictionary<string, string>();

            for (int i = 0; i < _performersRole.Length; i++)
            {
                var insts = _performersRole[i];

                if (string.IsNullOrEmpty(insts))
                    continue;

                var instlist = insts.Split(';');

                foreach (var iinst in instlist)
                {
                    var inst = iinst.Trim();

                    if (i < Performers.Length)
                    {
                        var perf = Performers[i];

                        if (!map.TryAdd(inst, perf))
                            map[inst] += ", " + perf;
                    }
                }
            }

            ret = new string[map.Count * 2];
            int j = 0;

            foreach (var dict in map)
            {
                ret[j++] = dict.Key;
                ret[j++] = dict.Value;
            }
        }

        SetTextFrame(FrameType.TMCL, ret);
        bool has_footer = (_header.Flags & HeaderFlags.FooterPresent) != 0;
        bool unsynchAtFrameLevel = (_header.Flags & HeaderFlags.Unsynchronisation) != 0 && Version >= 4;
        bool unsynchAtTagLevel = (_header.Flags & HeaderFlags.Unsynchronisation) != 0 && Version < 4;
        _header.MajorVersion = has_footer ? (byte)4 : Version;
        var tag_data = new ByteVector();
        _header.Flags &= ~HeaderFlags.ExtendedHeader;

        foreach (Frame frame in _frameList)
        {
            if (unsynchAtFrameLevel)
                frame.Flags |= FrameFlags.Unsynchronisation;

            if ((frame.Flags & FrameFlags.TagAlterPreservation) != 0)
                continue;

            try
            {
                tag_data.Add(frame.Render(_header.MajorVersion));
            }
            catch (NotImplementedException) { }
        }

        if (unsynchAtTagLevel)
            SynchData.UnsynchByteVector(tag_data);

        if (!has_footer)
            tag_data.Add(new ByteVector((int)((tag_data.Count < _header.TagSize) ? (_header.TagSize - tag_data.Count) : 1024)));

        _header.TagSize = (uint)tag_data.Count;
        tag_data.Insert(0, _header.Render());

        if (has_footer)
            tag_data.Add(new Footer(_header).Render());

        return tag_data;
    }

    public HeaderFlags Flags
    {
        get => _header.Flags;
        set => _header.Flags = value;
    }

    public byte Version
    {
        get => ForceDefaultVersion ? DefaultVersion : _header.MajorVersion;
        set
        {
            if (value < 2 || value > 4)
                throw new ArgumentOutOfRangeException(nameof(value), "Version must be 2, 3, or 4");

            _header.MajorVersion = value;
        }
    }

    public static string Language
    {
        get => _language;
        set => _language = (value is null || value.Length < 3) ? "   " : value.Substring(0, 3);
    }

    public static byte DefaultVersion
    {
        get => _default_version;
        set
        {
            if (value < 2 || value > 4)
                throw new ArgumentOutOfRangeException(nameof(value), "Version must be 2, 3, or 4");

            _default_version = value;
        }
    }

    public static bool ForceDefaultVersion { get; set; } = false;

    public static StringType DefaultEncoding { get; set; } = StringType.UTF8;

    public static bool ForceDefaultEncoding { get; set; } = false;

    public static bool UseNumericGenres { get; set; } = true;

    protected void Read(File file, long position, ReadStyle style)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Mode = File.AccessMode.Read;

        if (position < 0 || position > file.Length - Header.Size)
            throw new ArgumentOutOfRangeException(nameof(position));

        file.Seek(position);
        _header = new Header(file.ReadBlock((int)Header.Size));

        if (_header.TagSize == 0)
            return;

        position += Header.Size;
        Parse(null, file, position, style);
    }

    protected void Parse(ByteVector? data, File? file, long position, ReadStyle style)
    {
        bool fullTagUnsynch = (_header.MajorVersion < 4) && (_header.Flags & HeaderFlags.Unsynchronisation) != 0;

        if (data is null && file is not null
            && (fullTagUnsynch || _header.TagSize < 1024 || (style & ReadStyle.PictureLazy) == 0 || (_header.Flags & HeaderFlags.ExtendedHeader) != 0))
        {
            file.Seek(position);
            data = file.ReadBlock((int)_header.TagSize);
        }

        if (fullTagUnsynch)
            SynchData.ResynchByteVector(data);

        int frame_data_position = data is not null ? 0 : (int)position;
        int frame_data_endposition = (data is not null ? data.Count : (int)_header.TagSize) + frame_data_position - (int)FrameHeader.Size(_header.MajorVersion);

        if ((_header.Flags & HeaderFlags.ExtendedHeader) != 0)
        {
            _extendedHeader = new ExtendedHeader(data, _header.MajorVersion);

            if (_extendedHeader.Size <= data.Count)
            {
                frame_data_position += (int)_extendedHeader.Size;
                frame_data_endposition -= (int)_extendedHeader.Size;
            }
        }

        TextInformationFrame tdrc = null;
        TextInformationFrame tyer = null;
        TextInformationFrame tdat = null;
        TextInformationFrame time = null;

        while (frame_data_position < frame_data_endposition)
        {
            Frame frame;

            try
            {
                frame = FrameFactory.CreateFrame(data, file, ref frame_data_position, _header.MajorVersion, fullTagUnsynch);
            }
            catch (NotImplementedException)
            {
                continue;
            }
            catch (CorruptFileException)
            {
                throw;
            }

            if (frame is null)
                break;

            if (frame.Size == 0)
                continue;

            AddFrame(frame);

            if (_header.MajorVersion == 4)
                continue;

            if (frame.FrameId is not null)
            {
                if (tdrc is null && frame.FrameId.Equals(FrameType.TDRC))
                    tdrc = frame as TextInformationFrame;
                else if (tyer is null && frame.FrameId.Equals(FrameType.TYER))
                    tyer = frame as TextInformationFrame;
                else if (tdat is null && frame.FrameId.Equals(FrameType.TDAT))
                    tdat = frame as TextInformationFrame;
                else if (time is null && frame.FrameId.Equals(FrameType.TIME))
                    time = frame as TextInformationFrame;
            }
        }

        if (tdrc is null || tdat is null || tdrc.ToString().Length > 4)
            return;

        string year = tdrc.ToString();

        if (year.Length != 4)
            return;

        var tdrc_text = new StringBuilder();
        tdrc_text.Append(year);
        string tdat_text = tdat.ToString();

        if (tdat_text.Length == 4)
        {
            tdrc_text.Append('-').Append(tdat_text, 0, 2).Append('-').Append(tdat_text, 2, 2);

            if (time is not null)
            {
                string time_text = time.ToString();

                if (time_text.Length == 4)
                    tdrc_text.Append('T').Append(time_text, 0, 2).Append(':').Append(time_text, 2, 2);

                RemoveFrames(FrameType.TIME);
            }
        }

        RemoveFrames(FrameType.TDAT);
        tdrc.Text = [tdrc_text.ToString()];
    }

    private string?[] GetTextAsArray(ByteVector ident)
    {
        var frame = TextInformationFrame.Get(this, ident, false);
        return frame?.Text ?? [];
    }

    private uint GetTextAsUInt32(ByteVector ident, int index)
    {
        string text = GetTextAsString(ident);

        if (text is null)
            return 0;

        string[] values = text.Split(['/'], index + 2);

        if (values.Length < index + 1)
            return 0;

        if (uint.TryParse(values[index], out var result))
            return result;

        return 0;
    }

    private string? GetUserTextAsString(string description, bool caseSensitive)
    {
        var frame = UserTextInformationFrame.Get(this, description, DefaultEncoding, false, caseSensitive);
        string result = frame is null ? null : string.Join(";", frame.Text);
        return string.IsNullOrEmpty(result) ? null : result;
    }

    private string? GetUserTextAsString(string description) => GetUserTextAsString(description, true);

    public void SetUserTextAsString(string description, string? text, bool caseSensitive)
    {
        var frame = UserTextInformationFrame.Get(this, description, DefaultEncoding, true, caseSensitive);

        if (frame is null)
            return;

        if (!string.IsNullOrEmpty(text))
            frame.Text = text.Split(';');
        else
            RemoveFrame(frame);
    }

    public void SetUserTextAsString(string description, string text) => SetUserTextAsString(description, text, true);

    private string? GetUfidText(string owner)
    {
        var frame = UniqueFileIdentifierFrame.Get(this, owner, false);
        string result = frame?.Identifier?.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }

    private void SetUfidText(string owner, string text)
    {
        var frame = UniqueFileIdentifierFrame.Get(this, owner, true);

        if (frame is null)
            return;

        if (!string.IsNullOrEmpty(text))
        {
            var identifier = ByteVector.FromString(text, StringType.UTF8);
            frame.Identifier = identifier;
        }
        else
            RemoveFrame(frame);
    }

    private void MakeFirstOfType(Frame frame)
    {
        ByteVector type = frame.FrameId;
        Frame swapping = null;

        for (int i = 0; i < _frameList.Count; i++)
        {
            if (swapping is null)
            {
                if (_frameList[i].FrameId?.Equals(type) ?? false)
                    swapping = frame;
                else
                    continue;
            }

            (swapping, _frameList[i]) = (_frameList[i], swapping);

            if (swapping == frame)
                return;
        }

        if (swapping is not null)
            _frameList.Add(swapping);
    }

    public IEnumerator<Frame> GetEnumerator() => _frameList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _frameList.GetEnumerator();

    public override TagTypes TagTypes => TagTypes.Id3v2;

    public override string? Title
    {
        get => GetTextAsString(FrameType.TIT2);
        set => SetTextFrame(FrameType.TIT2, value);
    }

    public override string? TitleSort
    {
        get => GetTextAsString(FrameType.TSOT);
        set => SetTextFrame(FrameType.TSOT, value);
    }

    public override string? Subtitle
    {
        get => GetTextAsString(FrameType.TIT3);
        set => SetTextFrame(FrameType.TIT3, value);
    }

    public override string? Description
    {
        get => GetUserTextAsString("Description");
        set => SetUserTextAsString("Description", value);
    }

    public override string?[] Performers
    {
        get => GetTextAsArray(FrameType.TPE1);
        set => SetTextFrame(FrameType.TPE1, value);
    }

    public override string?[] PerformersSort
    {
        get => GetTextAsArray(FrameType.TSOP);
        set => SetTextFrame(FrameType.TSOP, value);
    }

    public override string[] PerformersRole
    {
        get
        {
            if (_performersRole is not null)
                return _performersRole;

            var perfref = Performers;

            if (Performers is null)
                return _performersRole = [];

            string?[] map = GetTextAsArray(FrameType.TMCL);
            _performersRole = new string[Performers.Length];

            for (int i = 0; i + 1 < map.Length; i += 2)
            {
                string inst = map[i];
                string perfs = map[i + 1];

                if (string.IsNullOrEmpty(inst) || string.IsNullOrEmpty(perfs))
                    continue;

                var perflist = perfs.Split(',');

                foreach (string iperf in perflist)
                {
                    if (iperf is null)
                        continue;

                    var perf = iperf.Trim();

                    if (string.IsNullOrEmpty(perf))
                        continue;

                    for (int j = 0; j < perfref.Length; j++)
                    {
                        if (perfref[j] == perf)
                            _performersRole[j] = _performersRole[j] is null ? inst : _performersRole[j] + "; " + inst;
                    }
                }
            }

            return _performersRole;
        }
        set => _performersRole = value ?? [];
    }

    public override string?[] AlbumArtistsSort
    {
        get => GetTextAsArray(FrameType.TSO2);
        set => SetTextFrame(FrameType.TSO2, value);
    }

    public override string?[] AlbumArtists
    {
        get => GetTextAsArray(FrameType.TPE2);
        set => SetTextFrame(FrameType.TPE2, value);
    }

    public override string?[] Composers
    {
        get => GetTextAsArray(FrameType.TCOM);
        set => SetTextFrame(FrameType.TCOM, value);
    }

    public override string?[] ComposersSort
    {
        get => GetTextAsArray(FrameType.TSOC);
        set => SetTextFrame(FrameType.TSOC, value);
    }

    public override string? Album
    {
        get => GetTextAsString(FrameType.TALB);
        set => SetTextFrame(FrameType.TALB, value);
    }

    public override string? AlbumSort
    {
        get => GetTextAsString(FrameType.TSOA);
        set => SetTextFrame(FrameType.TSOA, value);
    }

    public override string? Comment
    {
        get => CommentsFrame.GetPreferred(this, string.Empty, Language)?.ToString();
        set
        {
            CommentsFrame frame;

            if (string.IsNullOrEmpty(value))
            {
                while ((frame = CommentsFrame.GetPreferred(this, string.Empty, Language)) is not null)
                    RemoveFrame(frame);

                return;
            }

            frame = CommentsFrame.Get(this, string.Empty, Language, true);

            if (frame is null)
                return;

            frame.Text = value;
            frame.TextEncoding = DefaultEncoding;
            MakeFirstOfType(frame);
        }
    }

    public override string?[] Genres
    {
        get
        {
            string?[] text = GetTextAsArray(FrameType.TCON);

            if (text.Length == 0)
                return text;

            var list = new List<string>();

            foreach (string genre in text)
            {
                if (string.IsNullOrEmpty(genre))
                    continue;

                string genre_from_index = TagLib.Genres.IndexToAudio(genre);

                if (genre_from_index != null)
                    list.Add(genre_from_index);
                else
                    list.Add(genre);
            }

            return [.. list];
        }
        set
        {
            if (value is null || !UseNumericGenres)
            {
                SetTextFrame(FrameType.TCON, value);
                return;
            }

            value = (string[])value.Clone();

            for (int i = 0; i < value.Length; i++)
            {
                int index = TagLib.Genres.AudioToIndex(value[i]);

                if (index != 255)
                    value[i] = index.ToString(CultureInfo.InvariantCulture);
            }

            SetTextFrame(FrameType.TCON, value);
        }
    }

    public override uint Year
    {
        get
        {
            string text = GetTextAsString(FrameType.TDRC);

            if (text is null || text.Length < 4)
                return 0;

            if (uint.TryParse(text.AsSpan(0, 4), out var value))
                return value;

            return 0;
        }
        set
        {
            if (value > 9999)
                value = 0;

            SetNumberFrame(FrameType.TDRC, value, 0);
        }
    }

    public override uint Track
    {
        get => GetTextAsUInt32(FrameType.TRCK, 0);
        set => SetNumberFrame(FrameType.TRCK, value, TrackCount, "00");
    }

    public override uint TrackCount
    {
        get => GetTextAsUInt32(FrameType.TRCK, 1);
        set => SetNumberFrame(FrameType.TRCK, Track, value, "00");
    }

    public override uint Disc
    {
        get => GetTextAsUInt32(FrameType.TPOS, 0);
        set => SetNumberFrame(FrameType.TPOS, value, DiscCount);
    }

    public override uint DiscCount
    {
        get => GetTextAsUInt32(FrameType.TPOS, 1);
        set => SetNumberFrame(FrameType.TPOS, Disc, value);
    }

    public override string? Lyrics
    {
        get => UnsynchronisedLyricsFrame.GetPreferred(this, string.Empty, Language)?.ToString();
        set
        {
            UnsynchronisedLyricsFrame frame;

            if (string.IsNullOrEmpty(value))
            {
                while ((frame = UnsynchronisedLyricsFrame.GetPreferred(this, string.Empty, Language)) != null)
                    RemoveFrame(frame);

                return;
            }

            frame = UnsynchronisedLyricsFrame.Get(this, string.Empty, Language, true);

            if (frame is null)
                return;

            frame.Text = value;
            frame.TextEncoding = DefaultEncoding;
        }
    }

    public override string? Grouping
    {
        get => GetTextAsString(FrameType.TIT1);
        set => SetTextFrame(FrameType.TIT1, value);
    }

    public override uint BeatsPerMinute
    {
        get
        {
            string text = GetTextAsString(FrameType.TBPM);

            if (text is null)
                return 0;

            if (double.TryParse(text, out var result) && result >= 0.0)
                return (uint)Math.Round(result);

            return 0;
        }
        set => SetNumberFrame(FrameType.TBPM, value, 0);
    }

    public override string? Conductor
    {
        get => GetTextAsString(FrameType.TPE3);
        set => SetTextFrame(FrameType.TPE3, value);
    }

    public override string? Copyright
    {
        get => GetTextAsString(FrameType.TCOP);
        set => SetTextFrame(FrameType.TCOP, value);
    }

    public override DateTime? DateTagged
    {
        get
        {
            string value = GetTextAsString(FrameType.TDTG);

            if (value is not null)
            {
                value = value.Replace('T', ' ');

                if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out var date))
                    return date;
            }

            return null;
        }
        set
        {
            string date = null;

            if (value is not null)
            {
                date = $"{value:yyyy-MM-dd HH:mm:ss}";
                date = date.Replace(' ', 'T');
            }

            SetTextFrame(FrameType.TDTG, date);
        }
    }

    public override string? MusicBrainzArtistId
    {
        get => GetUserTextAsString("MusicBrainz Artist Id", false);
        set => SetUserTextAsString("MusicBrainz Artist Id", value);
    }

    public override string? MusicBrainzReleaseGroupId
    {
        get => GetUserTextAsString("MusicBrainz Release Group Id", false);
        set => SetUserTextAsString("MusicBrainz Release Group Id", value);
    }

    public override string? MusicBrainzReleaseId
    {
        get => GetUserTextAsString("MusicBrainz Album Id", false);
        set => SetUserTextAsString("MusicBrainz Album Id", value);
    }

    public override string? MusicBrainzReleaseArtistId
    {
        get => GetUserTextAsString("MusicBrainz Album Artist Id", false);
        set => SetUserTextAsString("MusicBrainz Album Artist Id", value);
    }

    public override string? MusicBrainzTrackId
    {
        get => GetUfidText("http://musicbrainz.org");
        set => SetUfidText("http://musicbrainz.org", value);
    }

    public override string? MusicBrainzDiscId
    {
        get => GetUserTextAsString("MusicBrainz Disc Id", false);
        set => SetUserTextAsString("MusicBrainz Disc Id", value);
    }

    public override string? MusicIpId
    {
        get => GetUserTextAsString("MusicIP PUID");
        set => SetUserTextAsString("MusicIP PUID", value);
    }

    public override string? AmazonId
    {
        get => GetUserTextAsString("ASIN");
        set => SetUserTextAsString("ASIN", value);
    }

    public override string? MusicBrainzReleaseStatus
    {
        get => GetUserTextAsString("MusicBrainz Album Status", false);
        set => SetUserTextAsString("MusicBrainz Album Status", value);
    }

    public override string? MusicBrainzReleaseType
    {
        get => GetUserTextAsString("MusicBrainz Album Type", false);
        set => SetUserTextAsString("MusicBrainz Album Type", value);
    }

    public override string? MusicBrainzReleaseCountry
    {
        get => GetUserTextAsString("MusicBrainz Album Release Country", false);
        set => SetUserTextAsString("MusicBrainz Album Release Country", value);
    }

    public override double ReplayGainTrackGain
    {
        get
        {
            string text = GetUserTextAsString("REPLAYGAIN_TRACK_GAIN", false);

            if (text is null)
                return double.NaN;

            if (text.EndsWith("db", StringComparison.InvariantCultureIgnoreCase))
                text = text.Substring(0, text.Length - 2).Trim();

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;

            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                SetUserTextAsString("REPLAYGAIN_TRACK_GAIN", null, false);
            else
            {
                string text = value.ToString("0.00 dB", CultureInfo.InvariantCulture);
                SetUserTextAsString("REPLAYGAIN_TRACK_GAIN", text, false);
            }
        }
    }

    public override double ReplayGainTrackPeak
    {
        get
        {
            string text;

            if ((text = GetUserTextAsString("REPLAYGAIN_TRACK_PEAK", false)) != null
                && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                SetUserTextAsString("REPLAYGAIN_TRACK_PEAK", null, false);
            else
            {
                string text = value.ToString("0.000000", CultureInfo.InvariantCulture);
                SetUserTextAsString("REPLAYGAIN_TRACK_PEAK", text, false);
            }
        }
    }

    public override double ReplayGainAlbumGain
    {
        get
        {
            string text = GetUserTextAsString("REPLAYGAIN_ALBUM_GAIN", false);

            if (text is null)
                return double.NaN;

            if (text.EndsWith("db", StringComparison.InvariantCultureIgnoreCase))
                text = text.Substring(0, text.Length - 2).Trim();

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;

            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                SetUserTextAsString("REPLAYGAIN_ALBUM_GAIN", null, false);
            else
            {
                string text = value.ToString("0.00 dB", CultureInfo.InvariantCulture);
                SetUserTextAsString("REPLAYGAIN_ALBUM_GAIN", text, false);
            }
        }
    }

    public override double ReplayGainAlbumPeak
    {
        get
        {
            string text;

            if ((text = GetUserTextAsString("REPLAYGAIN_ALBUM_PEAK", false)) != null
                && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return double.NaN;
        }
        set
        {
            if (double.IsNaN(value))
                SetUserTextAsString("REPLAYGAIN_ALBUM_PEAK", null, false);
            else
            {
                string text = value.ToString("0.000000", CultureInfo.InvariantCulture);
                SetUserTextAsString("REPLAYGAIN_ALBUM_PEAK", text, false);
            }
        }
    }

    public override string? InitialKey
    {
        get => GetTextAsString(FrameType.TKEY);
        set => SetTextFrame(FrameType.TKEY, value);
    }

    public override string? RemixedBy
    {
        get => GetTextAsString(FrameType.TPE4);
        set => SetTextFrame(FrameType.TPE4, value);
    }

    public override string? Publisher
    {
        get => GetTextAsString(FrameType.TPUB);
        set => SetTextFrame(FrameType.TPUB, value);
    }

    public override string? ISRC
    {
        get => GetTextAsString(FrameType.TSRC);
        set => SetTextFrame(FrameType.TSRC, value);
    }

    public override string? Length
    {
        get => GetTextAsString(FrameType.TLEN);
        set => SetTextFrame(FrameType.TLEN, value);
    }

    public override IPicture[] Pictures
    {
        get => GetFrames<AttachmentFrame>().ToArray();
        set
        {
            RemoveFrames(FrameType.APIC);
            RemoveFrames(FrameType.GEOB);

            if (value is null || value.Length == 0)
                return;

            foreach (var picture in value)
            {
                if (picture is not AttachmentFrame frame)
                    frame = new AttachmentFrame(picture);

                AddFrame(frame);
            }
        }
    }

    public override bool IsEmpty => _frameList.Count == 0;

    public override void Clear() => _frameList.Clear();

    public bool IsCompilation
    {
        get
        {
            string val = GetTextAsString(FrameType.TCMP);
            return !string.IsNullOrEmpty(val) && val != "0";
        }
        set => SetTextFrame(FrameType.TCMP, value ? "1" : null);
    }

    public override void CopyTo(TagLib.Tag target, bool overwrite)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target is not Tag match)
        {
            base.CopyTo(target, overwrite);
            return;
        }

        var frames = new List<Frame>(_frameList);

        while (frames.Count > 0)
        {
            ByteVector ident = frames[0].FrameId;
            bool copy = true;

            if (overwrite)
                match.RemoveFrames(ident);
            else
            {
                foreach (Frame f in match._frameList)
                {
                    if (f.FrameId?.Equals(ident) ?? false)
                    {
                        copy = false;
                        break;
                    }
                }
            }

            for (int i = 0; i < frames.Count;)
            {
                if (frames[i].FrameId?.Equals(ident) ?? false)
                {
                    if (copy)
                        match._frameList.Add(frames[i].Clone());

                    frames.RemoveAt(i);
                }
                else
                    i++;
            }
        }
    }

    public Tag Clone()
    {
        var tag = new Tag
        {
            _header = _header,
        };

        if (tag._extendedHeader is not null)
            tag._extendedHeader = _extendedHeader.Clone();

        foreach (Frame frame in _frameList)
            tag._frameList.Add(frame.Clone());

        return tag;
    }

    object ICloneable.Clone() => Clone();
}
