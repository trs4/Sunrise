namespace Sunrise.Model.TagLib;

public static class FileTypes
{
    private static Dictionary<string, Type> _fileTypes;

    private static readonly Type[] _staticFileTypes =
        [
            //typeof(Aac.File),
            //typeof(Aiff.File),
            //typeof(Ape.File),
            //typeof(Asf.File),
            //typeof(Audible.File),
            //typeof(Dsf.File),
            //typeof(Flac.File),
            //typeof(Matroska.File),
            //typeof(Gif.File),
            //typeof(Image.NoMetadata.File),
            //typeof(Jpeg.File),
            //typeof(Mpeg4.File),
            typeof(Mpeg.AudioFile),
            typeof(Mpeg.File),
            //typeof(MusePack.File),
            //typeof(Ogg.File),
            //typeof(Png.File),
            //typeof(Riff.File),
            //typeof(Tiff.Arw.File),
            //typeof(Tiff.Cr2.File),
            //typeof(Tiff.Dng.File),
            //typeof(Tiff.File),
            //typeof(Tiff.Nef.File),
            //typeof(Tiff.Pef.File),
            //typeof(Tiff.Rw2.File),
            //typeof(WavPack.File)
        ];

    static FileTypes()
        => Init();

    public static IDictionary<string, Type> AvailableTypes => _fileTypes;

    internal static void Init()
    {
        if (_fileTypes is not null)
            return;

        _fileTypes = [];

        foreach (Type type in _staticFileTypes)
            Register(type);
    }

    public static void Register(Type type)
    {
        Attribute[] attrs = Attribute.GetCustomAttributes(type, typeof(SupportedMimeType), false);

        if (attrs.Length == 0)
            return;

        foreach (var attr in attrs.OfType<SupportedMimeType>())
            _fileTypes.Add(attr.MimeType, type);
    }

}
