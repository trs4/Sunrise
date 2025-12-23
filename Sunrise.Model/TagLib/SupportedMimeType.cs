namespace Sunrise.Model.TagLib;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SupportedMimeType : Attribute
{
    private static readonly List<SupportedMimeType> _mimeTypes = [];

    static SupportedMimeType()
        => FileTypes.Init();

    public SupportedMimeType(string mimeType)
    {
        MimeType = mimeType;
        _mimeTypes.Add(this);
    }

    public SupportedMimeType(string mimeType, string extension)
        : this(mimeType)
        => Extension = extension;

    public string MimeType { get; }

    public string Extension { get; }

    public static IEnumerable<string> AllMimeTypes
    {
        get
        {
            foreach (SupportedMimeType type in _mimeTypes)
                yield return type.MimeType;
        }
    }

    public static IEnumerable<string> AllExtensions
    {
        get
        {
            foreach (SupportedMimeType type in _mimeTypes)
            {
                if (type.Extension is not null)
                    yield return type.Extension;
            }
        }
    }

}
