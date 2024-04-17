namespace Sunrise.Model.TagLib.Id3v2;

public static class FrameFactory
{
    public delegate Frame FrameCreator(ByteVector data, int offset, FrameHeader header, byte version);

    private static readonly List<FrameCreator> _frameCreators = [];

    public static Frame? CreateFrame(ByteVector data, File? file, ref int offset, byte version, bool alreadyUnsynched)
    {
        int position = 0;

        if (data is null)
        {
            if (file is not null)
            {
                file.Seek(offset);
                data = file.ReadBlock((int)FrameHeader.Size(version));
            }
        }
        else
        {
            file = null;
            position = offset;
        }

        if (data is null || data[position] == 0)
            return null;

        var header = new FrameHeader(data.Mid(position, (int)FrameHeader.Size(version)), version);
        int fileposition = offset + (int)FrameHeader.Size(version);
        offset += (int)(header.FrameSize + FrameHeader.Size(version));

        if (header.FrameId is null)
            return null; // throw new NotImplementedException();

        foreach (byte b in header.FrameId)
        {
            char c = (char)b;

            if ((c < 'A' || c > 'Z') && (c < '0' || c > '9'))
                return null;
        }

        if (alreadyUnsynched)
            header.Flags &= ~FrameFlags.Unsynchronisation;

        if (header.FrameSize == 0)
        {
            header.Flags |= FrameFlags.TagAlterPreservation;
            return new UnknownFrame(data, position, header, version);
        }

        if ((header.Flags & FrameFlags.Compression) != 0)
            throw new NotImplementedException();

        if ((header.Flags & FrameFlags.Encryption) != 0)
            throw new NotImplementedException();

        foreach (FrameCreator creator in _frameCreators)
        {
            Frame frame = creator(data, position, header, version);

            if (frame is not null)
                return frame;
        }

        if (file is not null)
        {
            // Attached Picture (frames 4.14)
            // General Encapsulated Object (frames 4.15)
            if (header.FrameId == FrameType.APIC || header.FrameId == FrameType.GEOB)
                return new AttachmentFrame(file.FileAbstraction, fileposition, offset - fileposition, header, version);

            // Read remaining part of the frame for the non lazy Frames
            file.Seek(fileposition);
            data.Add(file.ReadBlock(offset - fileposition));
        }

        // Text Identification (frames 4.2)
        if (header.FrameId == FrameType.TXXX)
            return new UserTextInformationFrame(data, position, header, version);

        if (header.FrameId[0] == (byte)'T')
            return new TextInformationFrame(data, position, header, version);

        // Involved People List (frames 4.4 in 2.3. in 2.4 this is a TIPL frame)
        if (header.FrameId == FrameType.IPLS)
            return new TextInformationFrame(data, position, header, version);

        // Unique File Identifier (frames 4.1)
        if (header.FrameId == FrameType.UFID)
            return new UniqueFileIdentifierFrame(data, position, header, version);

        // Music CD Identifier (frames 4.5)
        if (header.FrameId == FrameType.MCDI)
            return new MusicCdIdentifierFrame(data, position, header, version);

        // Unsynchronized Lyrics (frames 4.8)
        if (header.FrameId == FrameType.USLT)
            return new UnsynchronisedLyricsFrame(data, position, header, version);

        // Synchronized Lyrics (frames 4.9)
        if (header.FrameId == FrameType.SYLT)
            return new SynchronisedLyricsFrame(data, position, header, version);

        // Comments (frames 4.10)
        if (header.FrameId == FrameType.COMM)
            return new CommentsFrame(data, position, header, version);

        // Relative Volume Adjustment (frames 4.11)
        if (header.FrameId == FrameType.RVA2)
            return new RelativeVolumeFrame(data, position, header, version);

        // Attached Picture (frames 4.14)
        // General Encapsulated Object (frames 4.15)
        if (header.FrameId == FrameType.APIC || header.FrameId == FrameType.GEOB)
            return new AttachmentFrame(data, position, header, version);

        // Play Count (frames 4.16)
        if (header.FrameId == FrameType.PCNT)
            return new PlayCountFrame(data, position, header, version);

        // Play Count (frames 4.17)
        if (header.FrameId == FrameType.POPM)
            return new PopularimeterFrame(data, position, header, version);

        // Terms of Use (frames 4.22)
        if (header.FrameId == FrameType.USER)
            return new TermsOfUseFrame(data, position, header, version);

        // Private (frames 4.27)
        if (header.FrameId == FrameType.PRIV)
            return new PrivateFrame(data, position, header, version);

        // User Url Link (frames 4.3.2)
        if (header.FrameId == FrameType.WXXX)
            return new UserUrlLinkFrame(data, position, header, version);

        // Url Link (frames 4.3.1)
        if (header.FrameId[0] == (byte)'W')
            return new UrlLinkFrame(data, position, header, version);

        // Event timing codes (frames 4.6)
        if (header.FrameId == FrameType.ETCO)
            return new EventTimeCodesFrame(data, position, header, version);

        // Chapter (ID3v2 Chapter Frame Addendum)
        if (header.FrameId == FrameType.CHAP)
            return new ChapterFrame(data, position, header, version);

        // Table of Contents (ID3v2 Chapter Frame Addendum)
        if (header.FrameId == FrameType.CTOC)
            return new TableOfContentsFrame(data, position, header, version);

        return new UnknownFrame(data, position, header, version);
    }

    public static void AddFrameCreator(FrameCreator creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        _frameCreators.Insert(0, creator);
    }

}
