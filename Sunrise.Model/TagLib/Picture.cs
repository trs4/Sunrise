namespace Sunrise.Model.TagLib;

public class Picture : IPicture
{
#pragma warning disable IDE0300 // Simplify collection initialization
    private static readonly string[] _lutExtensionMime = new[] {
#pragma warning restore IDE0300 // Simplify collection initialization
            "aac", "audio/aac", // AAC audio file
			"abw", "application/x-abiword", // AbiWord document
			"arc", "application/octet-stream", // Archive document (multiple files embedded)
			"avi", "video/x-msvideo", // AVI: Audio Video Interleave
			"azw", "application/vnd.amazon.ebook", // Amazon Kindle eBook format
			"bin", "application/octet-stream", // Any kind of binary data
			"bmp", "image/bmp", // BMP image data
			"bmp", "image/x-windows-bmp", // BMP image data
			"bm", "image/bmp", // BMP image data
			"bz", "application/x-bzip", // BZip archive
			"bz2", "application/x-bzip2", // BZip2 archive
			"csh", "application/x-csh", // C-Shell script
			"css", "text/css", // Cascading Style Sheets (CSS)
			"csv", "text/csv", // Comma-separated values (CSV)
			"doc", "application/msword", // Microsoft Word
			"eot", "application/vnd.ms-fontobject", // MS Embedded OpenType fonts
			"epub", "application/epub+zip", // Electronic publication (EPUB)
			"gif", "image/gif", // Graphics Interchange Format (GIF)
			"htm", "text/html", // HyperText Markup Language (HTML)text / html
			"html", "text/html", // HyperText Markup Language (HTML)text / html
			"ico", "image/x-icon", // Icon format
			"ics", "text/calendar", // iCalendar format
			"jar", "application/java-archive", // Java Archive (JAR)
			"jpg", "image/jpeg", // JPEG images
			"jpeg", "image/jpeg", // JPEG images
			"js", "application/javascript", // JavaScript (ECMAScript)
			"json", "application/json", // JSON format
			"mid", "audio/midi", // Musical Instrument Digital Interface (MIDI)
			"midi", "audio/midi", // Musical Instrument Digital Interface (MIDI)
			"mp3", "audio/mpeg",
            "mp1", "audio/mpeg",
            "mp2", "audio/mpeg",
            "mpg", "video/mpeg",
            "mpeg", "video/mpeg", // MPEG Video
			"m4a", "audio/mp4",
            "mp4", "video/mp4",
            "m4v", "video/mp4",
            "mpkg", "application/vnd.apple.installer+xml", // Apple Installer Package
			"odp", "application/vnd.oasis.opendocument.presentation", // OpenDocuemnt presentation document
			"ods", "application/vnd.oasis.opendocument.spreadsheet", // OpenDocuemnt spreadsheet document
			"odt", "application/vnd.oasis.opendocument.text", // OpenDocument text document
			"oga", "audio/ogg", // OGG audio
			"ogg", "audio/ogg",
            "ogx", "application/ogg", // OGG
			"ogv", "video/ogg",
            "otf", "font/otf", // OpenType font
			"png", "image/png", // Portable Network Graphics
			"pdf", "application/pdf", // Adobe Portable Document Format (PDF)
			"ppt", "application/vnd.ms-powerpoint", // Microsoft PowerPoint
			"rar", "application/x-rar-compressed", // RAR archive
			"rtf", "application/rtf", // Rich Text Format (RTF)
			"sh", "application/x-sh", // Bourne shell script
			"svg", "image/svg+xml", // Scalable Vector Graphics (SVG)
			"swf", "application/x-shockwave-flash", // Small web format (SWF) or Adobe Flash document
			"tar", "application/x-tar", // Tape Archive (TAR)
			"tif", "image/tiff", //  Tagged Image File Format(TIFF)
			"tiff", "image/tiff", //  Tagged Image File Format(TIFF)
			"ts", "video/vnd.dlna.mpeg-tts", // Typescript file
			"ttf", "font/ttf", // TrueType Font
			"vsd", "application/vnd.visio", // Microsoft Visio
			"wav", "audio/x-wav", // Waveform Audio Format
			"weba", "audio/webm", // WEBM audio
			"webm", "video/webm", // WEBM video
			"webp", "image/webp", // WEBP image
			"woff", "font/woff", // Web Open Font Format (WOFF)
			"woff2", "font/woff2", // Web Open Font Format (WOFF)
			"xhtml", "application/xhtml+xml", // XHTML
			"xls", "application/vnd.ms", // excel application
			"xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // excel 2007 application
			"xml", "application/xml", // XML
			"xul", "application/vnd.mozilla.xul+xml", // XUL
			"zip", "application/zip", // ZIP archive
			"3gp", "video/3gpp", // 3GPP audio/video container
			"3g2", "video/3gpp2", // 3GPP2 audio/video container
			"7z", "application/x-7z-compressed", // 7-zip archive
		};

    public Picture() { }

    public Picture(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        Data = ByteVector.FromPath(path);
        Filename = Path.GetFileName(path);
        Description = Filename;
        MimeType = GetMimeFromExtension(Filename);
        Type = MimeType.StartsWith("image/") ? PictureType.FrontCover : PictureType.NotAPicture;
    }

    public Picture(File.IFileAbstraction abstraction)
    {
        ArgumentNullException.ThrowIfNull(abstraction);
        Data = ByteVector.FromFile(abstraction);
        Filename = abstraction.Name;
        Description = abstraction.Name;

        if (!string.IsNullOrEmpty(Filename) && Filename.Contains('.'))
        {
            MimeType = GetMimeFromExtension(Filename);
            Type = MimeType.StartsWith("image/") ? PictureType.FrontCover : PictureType.NotAPicture;
        }
        else
        {
            string ext = GetExtensionFromData(Data);
            MimeType = GetMimeFromExtension(ext);

            if (ext is not null)
            {
                Type = PictureType.FrontCover;
                Filename = Description = "cover" + ext;
            }
            else
            {
                Type = PictureType.NotAPicture;
                Filename = "UnknownType";
            }
        }
    }

    public Picture(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Data = new ByteVector(data);
        string ext = GetExtensionFromData(data);
        MimeType = GetMimeFromExtension(ext);

        if (ext is not null)
        {
            Type = PictureType.FrontCover;
            Filename = Description = "cover" + ext;
        }
        else
        {
            Type = PictureType.NotAPicture;
            Filename = "UnknownType";
        }
    }

    public Picture(IPicture picture)
    {
        MimeType = picture.MimeType;
        Type = picture.Type;
        Filename = picture.Filename;
        Description = picture.Description;
        Data = picture.Data;
    }

    public string MimeType { get; set; }

    public PictureType Type { get; set; }

    public string Filename { get; set; }

    public string? Description { get; set; }

    public ByteVector Data { get; set; }

    public static string? GetExtensionFromData(ByteVector data)
    {
        string ext = null;

        if (data.Count >= 4) // No picture, unless it is corrupted, can fit in a file of less than 4 bytes
        {
            if (data[1] == 'P' && data[2] == 'N' && data[3] == 'G')
                ext = ".png";
            else if (data[0] == 'G' && data[1] == 'I' && data[2] == 'F')
                ext = ".gif";
            else if (data[0] == 'B' && data[1] == 'M')
                ext = ".bmp";
            else if (data[0] == 0xFF && data[1] == 0xD8 && data[^2] == 0xFF && data[^1] == 0xD9)
                ext = ".jpg";
        }

        return ext;
    }

    public static string? GetExtensionFromMime(string mime)
    {
        string ext = null;

        for (int i = 1; i < _lutExtensionMime.Length; i += 2)
        {
            if (_lutExtensionMime[i] == mime)
            {
                ext = _lutExtensionMime[i - 1];
                break;
            }
        }

        return ext;
    }

    public static string GetMimeFromExtension(string name)
    {
        string mime_type = "application/octet-stream";

        if (string.IsNullOrEmpty(name))
            return mime_type;

        var ext = Path.GetExtension(name);

        if (string.IsNullOrEmpty(ext))
            ext = name;
        else
            ext = ext.Substring(1);

        ext = ext.ToLower();

        for (int i = 0; i < _lutExtensionMime.Length; i += 2)
        {
            if (_lutExtensionMime[i] == ext)
            {
                mime_type = _lutExtensionMime[i + 1];
                break;
            }
        }

        return mime_type;
    }

}
