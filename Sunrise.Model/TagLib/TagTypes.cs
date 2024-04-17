namespace Sunrise.Model.TagLib;

[Flags]
public enum TagTypes : uint
{
    None = 0x00000000,
    Xiph = 0x00000001,
    Id3v1 = 0x00000002,
    Id3v2 = 0x00000004,
    Ape = 0x00000008,
    Apple = 0x00000010,
    Asf = 0x00000020,
    RiffInfo = 0x00000040,
    MovieId = 0x00000080,
    DivX = 0x00000100,
    FlacMetadata = 0x00000200,
    TiffIFD = 0x00000400,
    XMP = 0x00000800,
    JpegComment = 0x00001000,
    GifComment = 0x00002000,
    Png = 0x00004000,
    IPTCIIM = 0x00008000,
    AudibleMetadata = 0x00010000,
    Matroska = 0x00020000,
    AllTags = 0xFFFFFFFF,
}
