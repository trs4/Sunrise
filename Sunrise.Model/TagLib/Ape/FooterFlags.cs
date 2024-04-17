namespace Sunrise.Model.TagLib.Ape;

[Flags]
public enum FooterFlags : uint
{
    FooterAbsent = 0x40000000,
    IsHeader = 0x20000000,
    HeaderPresent = 0x80000000
}
