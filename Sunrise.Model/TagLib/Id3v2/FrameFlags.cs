namespace Sunrise.Model.TagLib.Id3v2;

[Flags]
public enum FrameFlags : ushort
{
    None = 0,
    TagAlterPreservation = 0x4000,
    FileAlterPreservation = 0x2000,
    ReadOnly = 0x1000,
    GroupingIdentity = 0x0040,
    Compression = 0x0008,
    Encryption = 0x0004,
    Unsynchronisation = 0x0002,
    DataLengthIndicator = 0x0001,
}
