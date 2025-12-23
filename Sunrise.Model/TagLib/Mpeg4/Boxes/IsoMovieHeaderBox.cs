namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>
///    This class extends <see cref="FullBox" /> to provide an
///    implementation of a ISO/IEC 14496-12 MovieHeaderBox
/// </summary>
public class IsoMovieHeaderBox : FullBox
{
    /// <summary>Contains the creation time of the movie</summary>
    private readonly ulong _creationTime;

    /// <summary>Contains the modification time of the movie</summary>
    private readonly ulong _modificationTime;

    /// <summary>Contains the timescale</summary>
    private readonly uint _timescale;

    /// <summary>Contains the duration</summary>
    private readonly ulong _duration;

    /// <summary>Contains the rate</summary>
    private readonly uint _rate;

    /// <summary>Contains the volume</summary>
    private readonly ushort _volume;

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="IsoMovieHeaderBox" /> with a provided header and
    ///    handler by reading the contents from a specified file
    /// </summary>
    /// <param name="header">
    ///    A <see cref="BoxHeader" /> object containing the header
    ///    to use for the new instance
    /// </param>
    /// <param name="file">
    ///    A <see cref="TagLib.File" /> object to read the contents
    ///    of the box from
    /// </param>
    /// <param name="handler">
    ///    A <see cref="IsoHandlerBox" /> object containing the
    ///    handler that applies to the new instance
    /// </param>
    public IsoMovieHeaderBox(BoxHeader header, TagLib.File file, IsoHandlerBox handler)
        : base(header, file, handler)
    {
        ArgumentNullException.ThrowIfNull(file);
        int bytes_remaining = DataSize;
        ByteVector data;

        if (Version == 1)
        {
            // Read version one (large integers)
            data = file.ReadBlock(Math.Min(28, bytes_remaining));

            if (data.Count >= 8)
                _creationTime = data.Mid(0, 8).ToULong();

            if (data.Count >= 16)
                _modificationTime = data.Mid(8, 8).ToULong();

            if (data.Count >= 20)
                _timescale = data.Mid(16, 4).ToUInt();

            if (data.Count >= 28)
                _duration = data.Mid(20, 8).ToULong();

            bytes_remaining -= 28;
        }
        else
        {
            // Read version zero (normal integers)
            data = file.ReadBlock(Math.Min(16, bytes_remaining));

            if (data.Count >= 4)
                _creationTime = data.Mid(0, 4).ToUInt();

            if (data.Count >= 8)
                _modificationTime = data.Mid(4, 4).ToUInt();

            if (data.Count >= 12)
                _timescale = data.Mid(8, 4).ToUInt();

            if (data.Count >= 16)
                _duration = data.Mid(12, 4).ToUInt();

            bytes_remaining -= 16;
        }

        data = file.ReadBlock(Math.Min(6, bytes_remaining));

        if (data.Count >= 4)
            _rate = data.Mid(0, 4).ToUInt();

        if (data.Count >= 6)
            _volume = data.Mid(4, 2).ToUShort();

        file.Seek(file.Tell + 70);
        bytes_remaining -= 76;
        data = file.ReadBlock(Math.Min(4, bytes_remaining));

        if (data.Count >= 4)
            NextTrackId = data.Mid(0, 4).ToUInt();
    }

    /// <summary>Gets the creation time of movie represented by the current instance</summary>
    /// <value>A <see cref="DateTime" /> value containing the creation time of the movie represented by the current instance</value>
    public DateTime CreationTime => new DateTime(1904, 1, 1, 0, 0, 0).AddTicks((long)(10000000 * _creationTime));

    /// <summary>Gets the modification time of movie represented by the current instance</summary>
    /// <value>A <see cref="DateTime" /> value containing the modification time of the movie represented by the current instance</value>
    public DateTime ModificationTime => new DateTime(1904, 1, 1, 0, 0, 0).AddTicks((long)(10000000 * _modificationTime));

    /// <summary>Gets the duration of the movie represented by the current instance</summary>
    /// <value>A <see cref="TimeSpan" /> value containing the duration of the movie represented by the current instance</value>
    public TimeSpan Duration => TimeSpan.FromSeconds(_duration / (double)_timescale);

    /// <summary>Gets the playback rate of the movie represented by the current instance</summary>
    /// <value>A <see cref="double" /> value containing the playback rate of the movie represented by the current instance</value>
    public double Rate => _rate / ((double)0x10000);

    /// <summary>Gets the playback volume of the movie represented by the current instance</summary>
    /// <value>A <see cref="double" /> value containing the playback volume of the movie represented by the current instance</value>
    public double Volume => _volume / ((double)0x100);

    /// <summary>Gets the ID of the next track in the movie represented by the current instance</summary>
    /// <value>A <see cref="uint" /> value containing the ID of the next track in the movie represented by the current instance</value>
    public uint NextTrackId { get; }
}
