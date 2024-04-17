namespace Sunrise.Model.TagLib.Id3v2;

[Flags]
public enum HeaderFlags : byte
{
    None = 0,
    Unsynchronisation = 0x80,
    ExtendedHeader = 0x40,
    ExperimentalIndicator = 0x20,
    FooterPresent = 0x10,
}
