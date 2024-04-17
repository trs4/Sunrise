namespace Sunrise.Model.TagLib.Mpeg;

public enum Marker
{
    Corrupt = -1,
    Zero = 0,
    SystemSyncPacket = 0xBA,
    VideoSyncPacket = 0xB3,
    SystemPacket = 0xBB,
    PaddingPacket = 0xBE,
    AudioPacket = 0xC0,
    VideoPacket = 0xE0,
    EndOfStream = 0xB9,
}
