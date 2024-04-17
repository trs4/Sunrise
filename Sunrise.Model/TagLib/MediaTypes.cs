namespace Sunrise.Model.TagLib;

[Flags]
public enum MediaTypes
{
    None = 0,
    Audio = 1,
    Video = 2,
    Photo = 4,
    Text = 8,
}
