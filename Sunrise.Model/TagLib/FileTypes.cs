using static Sunrise.Model.TagLib.File;

namespace Sunrise.Model.TagLib;

public static class FileTypes
{
    private static Dictionary<string, Func<IFileAbstraction, ReadStyle, File>> _fileTypes;

    private static readonly (Type Type, Func<IFileAbstraction, ReadStyle, File> Create)[] _staticFileTypes =
        [
            (typeof(Aac.File), (a, s) => new Aac.File(a, s)),
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
            (typeof(Mpeg4.File), (a, s) => new Mpeg4.File(a, s)),
            (typeof(Mpeg.AudioFile), (a, s) => new Mpeg.AudioFile(a, s)),
            (typeof(Mpeg.File), (a, s) => new Mpeg.File(a, s)),
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

    public static bool TryGetCreate(string mimeType, out Func<IFileAbstraction, ReadStyle, File>? create)
        => _fileTypes.TryGetValue(mimeType, out create);

    internal static void Init()
    {
        if (_fileTypes is not null)
            return;

        _fileTypes = [];

        foreach (var (type, create) in _staticFileTypes)
            Register(type, create);
    }

    public static void Register(Type type, Func<IFileAbstraction, ReadStyle, File> create)
    {
        var attributes = Attribute.GetCustomAttributes(type, typeof(SupportedMimeType), false);

        if (attributes.Length == 0)
            return;

        foreach (var attribute in attributes.OfType<SupportedMimeType>())
            _fileTypes.Add(attribute.MimeType, create);
    }

}
