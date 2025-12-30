namespace Sunrise.Model.TagLib;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SupportedMimeType : Attribute
{
    private static readonly List<SupportedMimeType> _mimeTypes = [];
    private static HashSet<string> _allMimeTypes;
    private static HashSet<string> _allExtensions;
    private static HashSet<string> _allAudioExtensions;

    static SupportedMimeType()
        => FileTypes.Init();

    public SupportedMimeType(string mimeType, FileType type = FileType.Audio)
    {
        MimeType = mimeType;
        Type = type;
        _mimeTypes.Add(this);
    }

    public SupportedMimeType(string mimeType, string extension, FileType type = FileType.Audio)
        : this(mimeType, type)
        => Extension = extension;

    public string MimeType { get; }

    public string Extension { get; }

    public FileType Type { get; }

    public static HashSet<string> AllMimeTypes
        => _allMimeTypes ??= _mimeTypes.Select(t => t.MimeType)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static HashSet<string> AllExtensions
        => _allExtensions ??= _mimeTypes.Where(t => t.Extension is not null).Select(t => t.Extension)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static HashSet<string> AllAudioExtensions
        => _allAudioExtensions ??= _mimeTypes.Where(t => t.Extension is not null && t.Type == FileType.Audio).Select(t => t.Extension)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
