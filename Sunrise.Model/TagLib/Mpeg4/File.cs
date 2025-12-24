namespace Sunrise.Model.TagLib.Mpeg4;

/// <summary>This class extends <see cref="TagLib.File" /> to provide tagging and properties support for MPEG-4 files</summary>
[SupportedMimeType("taglib/m4a", "m4a")]
[SupportedMimeType("taglib/m4b", "m4b")]
[SupportedMimeType("taglib/m4v", "m4v")]
[SupportedMimeType("taglib/m4p", "m4p")]
[SupportedMimeType("taglib/mp4", "mp4")]
[SupportedMimeType("audio/mp4")]
[SupportedMimeType("audio/x-m4a")]
[SupportedMimeType("video/mp4")]
[SupportedMimeType("video/x-m4v")]
public class File : TagLib.File
{
    /// <summary>Contains the Apple tag</summary>
    private AppleTag? _appleTag;

    /// <summary>Contains the combined tag</summary>
    /// <remarks>TODO: Add support for ID3v2 tags</remarks>
    private CombinedTag _tag;

    /// <summary>Contains the media properties</summary>
    private Properties _properties;

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="File" /> for a specified file abstraction and
    ///    specified read style
    /// </summary>
    /// <param name="abstraction">
    ///    A <see cref="File.IFileAbstraction" /> object to use when
    ///    reading from and writing to the file
    /// </param>
    /// <param name="propertiesStyle">
    ///    A <see cref="ReadStyle" /> value specifying at what level
    ///    of accuracy to read the media properties, or <see
    ///    cref="ReadStyle.None" /> to ignore the properties
    /// </param>
    public File(IFileAbstraction abstraction, ReadStyle propertiesStyle)
        : base(abstraction)
        => Read(propertiesStyle);

    /// <summary>
    ///    Constructs and initializes a new instance of <see
    ///    cref="File" /> for a specified file abstraction with an
    ///    average read style
    /// </summary>
    /// <param name="abstraction">
    ///    A <see cref="IFileAbstraction" /> object to use when
    ///    reading from and writing to the file
    /// </param>
    public File(IFileAbstraction abstraction) : this(abstraction, ReadStyle.Average) { }

    /// <summary>
    ///    Gets a abstract representation of all tags stored in the
    ///    current instance
    /// </summary>
    /// <value>
    ///    A <see cref="TagLib.Tag" /> object representing all tags
    ///    stored in the current instance
    /// </value>
    public override Tag Tag => _tag;

    /// <summary>
    ///    Gets the media properties of the file represented by the
    ///    current instance
    /// </summary>
    /// <value>
    ///    A <see cref="TagLib.Properties" /> object containing the
    ///    media properties of the file represented by the current
    ///    instance
    /// </value>
    public override Properties Properties => _properties;

    protected List<IsoUserDataBox> UdtaBoxes { get; } = [];

    /// <summary>Saves the changes made in the current instance to the file it represents</summary>
    public override void Save()
    {
        if (UdtaBoxes.Count == 0)
        {
            IsoUserDataBox udtaBox = new IsoUserDataBox();
            UdtaBoxes.Add(udtaBox);
        }

        // Try to get into write mode
        Mode = AccessMode.Write;

        try
        {
            FileParser parser = new FileParser(this);
            parser.ParseBoxHeaders();

            InvariantStartPosition = parser.MdatStartPosition;
            InvariantEndPosition = parser.MdatEndPosition;

            // To avoid rewriting udta blocks which might not have been modified,
            // the code here will work correctly if:
            // 1. There is a single udta for the entire file
            //   - OR -
            // 2. There are multiple utdtas, but only 1 of them contains the Apple ILST box.
            // We should be OK in the vast majority of cases
            var udtaBox = FindAppleTagUdta() ?? new IsoUserDataBox();
            ByteVector tag_data = udtaBox.Render();
            long sizeChange;
            long writePosition;

            // If we don't have a "udta" box to overwrite...
            if (udtaBox.ParentTree is null || udtaBox.ParentTree.Length == 0)
            {

                // Stick the box at the end of the moov box
                BoxHeader moov_header = parser.MoovTree![^1];
                sizeChange = tag_data.Count;
                writePosition = moov_header.Position + moov_header.TotalBoxSize;
                Insert(tag_data, writePosition, 0);

                // Overwrite the parent box sizes
                for (int i = parser.MoovTree.Length - 1; i >= 0; i--)
                    sizeChange = parser.MoovTree[i].Overwrite(this, sizeChange);
            }
            else
            {
                // Overwrite the old box
                BoxHeader udta_header = udtaBox.ParentTree[^1];
                sizeChange = tag_data.Count - udta_header.TotalBoxSize;
                writePosition = udta_header.Position;
                Insert(tag_data, writePosition, udta_header.TotalBoxSize);

                // Overwrite the parent box sizes
                for (int i = udtaBox.ParentTree.Length - 2; i >= 0; i--)
                    sizeChange = udtaBox.ParentTree[i].Overwrite(this, sizeChange);
            }

            // If we've had a size change, we may need to adjust chunk offsets
            if (sizeChange != 0)
            {
                // We may have moved the offset boxes, so we need to reread
                parser.ParseChunkOffsets();
                InvariantStartPosition = parser.MdatStartPosition;
                InvariantEndPosition = parser.MdatEndPosition;

                foreach (Box box in parser.ChunkOffsetBoxes)
                {
                    if (box is IsoChunkLargeOffsetBox co64)
                    {
                        co64.Overwrite(this, sizeChange, writePosition);
                        continue;
                    }

                    if (box is IsoChunkOffsetBox stco)
                    {
                        stco.Overwrite(this, sizeChange, writePosition);
                        continue;
                    }
                }
            }

            TagTypesOnDisk = TagTypes;
        }
        finally
        {
            Mode = AccessMode.Closed;
        }
    }

    /// <summary>
    ///    Gets a tag of a specified type from the current instance,
    ///    optionally creating a new tag if possible
    /// </summary>
    /// <param name="type">
    ///    A <see cref="TagTypes" /> value indicating the
    ///    type of tag to read
    /// </param>
    /// <param name="create">
    ///    A <see cref="bool" /> value specifying whether or not to
    ///    try and create the tag if one is not found
    /// </param>
    /// <returns>
    ///    A <see cref="Tag" /> object containing the tag that was
    ///    found in or added to the current instance. If no
    ///    matching tag was found and none was created, <see
    ///    langword="null" /> is returned
    /// </returns>
    /// <remarks>
    ///    At the time of this writing, only <see cref="AppleTag" />
    ///    is supported. All other tag types will be ignored
    /// </remarks>
    public override Tag? GetTag(TagTypes type, bool create)
    {
        if (type == TagTypes.Apple)
        {
            if (_appleTag == null && create)
            {
                IsoUserDataBox udtaBox = FindAppleTagUdta();

                if (null == udtaBox)
                    udtaBox = new IsoUserDataBox();

                _appleTag = new AppleTag(udtaBox);
                _tag.SetTags(_appleTag);
            }

            return _appleTag;
        }

        return null;
    }

    /// <summary>
    ///    Removes a set of tag types from the current instance
    /// </summary>
    /// <param name="types">
    ///    A bitwise combined <see cref="TagLib.TagTypes" /> value
    ///    containing tag types to be removed from the file
    /// </param>
    /// <remarks>
    ///    In order to remove all tags from a file, pass <see
    ///    cref="TagTypes.AllTags" /> as <paramref name="types" />
    /// </remarks>
    public override void RemoveTags(TagTypes types)
    {
        if ((types & TagTypes.Apple) != TagTypes.Apple || _appleTag is null)
            return;

        _appleTag.DetachIlst();
        _appleTag = null;
        _tag.SetTags();
    }

    /// <summary>
    ///    Reads the file with a specified read style
    /// </summary>
    /// <param name="propertiesStyle">
    ///    A <see cref="ReadStyle" /> value specifying at what level
    ///    of accuracy to read the media properties, or <see
    ///    cref="ReadStyle.None" /> to ignore the properties
    /// </param>
    private void Read(ReadStyle propertiesStyle)
    {
        // TODO: Support Id3v2 boxes!!!
        _tag = new CombinedTag();
        Mode = AccessMode.Read;

        try
        {
            var parser = new FileParser(this);

            if (propertiesStyle == ReadStyle.None)
                parser.ParseTag();
            else
                parser.ParseTagAndProperties();

            InvariantStartPosition = parser.MdatStartPosition;
            InvariantEndPosition = parser.MdatEndPosition;

            UdtaBoxes.AddRange(parser.UserDataBoxes);

            // Ensure our collection contains at least a single empty box
            if (UdtaBoxes.Count == 0)
            {
                IsoUserDataBox dummy = new IsoUserDataBox();
                UdtaBoxes.Add(dummy);
            }

            // Check if a udta with ILST actually exists
            if (IsAppleTagUdtaPresent())
                TagTypesOnDisk |= TagTypes.Apple; // There is an udta present with ILST info

            // Find the udta box with the Apple Tag ILST
            var udtaBox = FindAppleTagUdta() ?? new IsoUserDataBox();
            _appleTag = new AppleTag(udtaBox);
            _tag.SetTags(_appleTag);

            // If we're not reading properties, we're done
            if (propertiesStyle == ReadStyle.None)
            {
                Mode = AccessMode.Closed;
                return;
            }

            // Get the movie header box
            IsoMovieHeaderBox mvhd_box = parser.MovieHeaderBox;

            if (mvhd_box is null)
            {
                Mode = AccessMode.Closed;
                throw new CorruptFileException("mvhd box not found.");
            }

            IsoAudioSampleEntry audio_sample_entry = parser.AudioSampleEntry;
            IsoVisualSampleEntry visual_sample_entry = parser.VisualSampleEntry;

            // Read the properties
            _properties = new Properties(mvhd_box.Duration, audio_sample_entry, visual_sample_entry);
        }
        finally
        {
            Mode = AccessMode.Closed;
        }
    }

    /// <summary>
    ///    Find the udta box within our collection that contains the Apple ILST data
    /// </summary>
    /// <remarks>
    ///		If there is a single udta in a file, we return that.
    ///		If there are multiple udtas, we search for the one that contains the ILST box
    /// </remarks>
    private IsoUserDataBox? FindAppleTagUdta()
    {
        if (UdtaBoxes.Count == 1)
            return UdtaBoxes[0];   //Single udta - just return it

        // multiple udta : pick out the shallowest node which has an ILst tag
        return UdtaBoxes
            .Where(box => box.GetChildRecursively(BoxType.Ilst) is not null)
            .OrderBy(box => box.ParentTree.Length)
            .FirstOrDefault();
    }

    /// <summary>Returns true if there is a udta with ILST present in our collection</summary>
    private bool IsAppleTagUdtaPresent()
    {
        foreach (IsoUserDataBox udtaBox in UdtaBoxes)
        {
            if (udtaBox.GetChild(BoxType.Meta) != null && udtaBox.GetChild(BoxType.Meta)?.GetChild(BoxType.Ilst) is not null)
                return true;
        }

        return false;
    }

}
