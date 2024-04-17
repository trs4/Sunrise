namespace Sunrise.Model.TagLib;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SupportedMimeType : Attribute
{
    private static readonly List<SupportedMimeType> _mimetypes = [];

    static SupportedMimeType()
        => FileTypes.Init();

    public SupportedMimeType(string mimetype)
    {
        MimeType = mimetype;
        _mimetypes.Add(this);
    }

    public SupportedMimeType(string mimetype, string extension)
        : this(mimetype)
        => Extension = extension;

    public string MimeType { get; private set; }

    public string Extension { get; private set; }

    public static IEnumerable<string> AllMimeTypes
    {
        get
        {
            foreach (SupportedMimeType type in _mimetypes)
                yield return type.MimeType;
        }
    }

    public static IEnumerable<string> AllExtensions
    {
        get
        {
            foreach (SupportedMimeType type in _mimetypes)
            {
                if (type.Extension is not null)
                    yield return type.Extension;
            }
        }
    }

}
