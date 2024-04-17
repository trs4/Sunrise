using System.Globalization;

namespace Sunrise.Model.TagLib;

public abstract class File : IDisposable
{
    public enum AccessMode
    {
        Read,
        Write,
        Closed,
    }

    public delegate File FileTypeResolver(IFileAbstraction abstraction, string mimetype, ReadStyle style);

    private Stream? _fileStream;
    protected IFileAbstraction _fileAbstraction;
    private const int _bufferSize = 1024;
    private static readonly List<FileTypeResolver> _fileTypeResolvers = [];
    private List<string> _corruptionReasons;

    public static uint BufferSize => _bufferSize;

    protected File(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        _fileAbstraction = new LocalFileAbstraction(path);
    }

    protected File(IFileAbstraction abstraction)
    {
        ArgumentNullException.ThrowIfNull(abstraction);
        _fileAbstraction = abstraction;
    }

    public abstract Tag Tag { get; }

    public abstract Properties? Properties { get; }

    public TagTypes TagTypesOnDisk { get; protected set; } = TagTypes.None;

    public TagTypes TagTypes => Tag?.TagTypes ?? TagTypes.None;

    public string Name => _fileAbstraction.Name;

    public string MimeType { get; internal set; }

    public long Tell => (Mode == AccessMode.Closed) ? 0 : _fileStream?.Position ?? 0;

    public long Length => (Mode == AccessMode.Closed) ? 0 : _fileStream?.Length ?? 0;

    public long InvariantStartPosition { get; protected set; } = -1;

    public long InvariantEndPosition { get; protected set; } = -1;

    public AccessMode Mode
    {
        get
        {
            if (_fileStream is null)
                return AccessMode.Closed;

            if (_fileStream.CanWrite)
                return AccessMode.Write;

            return AccessMode.Read;
        }
        set
        {
            var mode = Mode;

            if (mode == value || (mode == AccessMode.Write && value == AccessMode.Read))
                return;

            if (_fileStream is not null)
                _fileAbstraction.CloseStream(_fileStream);

            _fileStream = null;

            if (value == AccessMode.Read)
                _fileStream = _fileAbstraction.ReadStream;
            else if (value == AccessMode.Write)
                _fileStream = _fileAbstraction.WriteStream;

            //Mode = value;
        }
    }

    public IFileAbstraction FileAbstraction => _fileAbstraction;

    public virtual bool Writeable => !PossiblyCorrupt;

    public bool PossiblyCorrupt => _corruptionReasons != null;

    public IEnumerable<string> CorruptionReasons => _corruptionReasons;

    internal void MarkAsCorrupt(string reason) => (_corruptionReasons ??= []).Add(reason);

    public void Dispose()
    {
        Mode = AccessMode.Closed;
        GC.SuppressFinalize(this);
    }

    public abstract void Save();

    public abstract void RemoveTags(TagTypes types);

    public abstract Tag? GetTag(TagTypes type, bool create);

    public Tag? GetTag(TagTypes type) => GetTag(type, false);

    public ByteVector ReadBlock(int length)
    {
        if (length < 0)
            throw new ArgumentException("Length must be non-negative", nameof(length));

        if (length == 0)
            return [];

        ArgumentNullException.ThrowIfNull(_fileStream);
        Mode = AccessMode.Read;
        byte[] buffer = new byte[length];
        int count = 0, read = 0, needed = length;

        do
        {
            count = _fileStream.Read(buffer, read, needed);
            read += count;
            needed -= count;
        } while (needed > 0 && count != 0);

        return new ByteVector(buffer, read);
    }

    public void WriteBlock(ByteVector data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(_fileStream);
        Mode = AccessMode.Write;
        _fileStream.Write(data.Data, 0, data.Count);
    }

    public long Find(ByteVector pattern, long startPosition, ByteVector? before)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(_fileStream);
        Mode = AccessMode.Read;

        if (pattern.Count > _bufferSize)
            return -1;

        long buffer_offset = startPosition;
        long original_position = _fileStream.Position;

        try
        {
            _fileStream.Position = startPosition;

            for (var buffer = ReadBlock(_bufferSize); buffer.Count > 0; buffer = ReadBlock(_bufferSize))
            {
                var location = buffer.Find(pattern);

                if (before is not null)
                {
                    var beforeLocation = buffer.Find(before);

                    if (beforeLocation < location)
                        return -1;
                }

                if (location >= 0)
                    return buffer_offset + location;

                buffer_offset += _bufferSize - pattern.Count;

                if (before is not null && before.Count > pattern.Count)
                    buffer_offset -= before.Count - pattern.Count;

                _fileStream.Position = buffer_offset;
            }

            return -1;
        }
        finally
        {
            _fileStream.Position = original_position;
        }
    }

    public long Find(ByteVector pattern, long startPosition) => Find(pattern, startPosition, null);

    public long Find(ByteVector pattern) => Find(pattern, 0);

    private long RFind(ByteVector pattern, long startPosition, ByteVector? after)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(_fileStream);
        Mode = AccessMode.Read;

        if (pattern.Count > _bufferSize)
            return -1;

        ByteVector buffer;
        long original_position = _fileStream.Position;
        long buffer_offset = Length - startPosition;
        int read_size = _bufferSize;
        read_size = (int)Math.Min(buffer_offset, _bufferSize);
        buffer_offset -= read_size;
        _fileStream.Position = buffer_offset;

        for (buffer = ReadBlock(read_size); buffer.Count > 0; buffer = ReadBlock(read_size))
        {
            long location = buffer.RFind(pattern);

            if (location >= 0)
            {
                _fileStream.Position = original_position;
                return buffer_offset + location;
            }

            if (after is not null && buffer.RFind(after) >= 0)
            {
                _fileStream.Position = original_position;
                return -1;
            }

            read_size = (int)Math.Min(buffer_offset, _bufferSize);
            buffer_offset -= read_size;

            if (read_size + pattern.Count > _bufferSize)
                buffer_offset += pattern.Count;

            _fileStream.Position = buffer_offset;
        }

        _fileStream.Position = original_position;
        return -1;
    }

    public long RFind(ByteVector pattern, long startPosition) => RFind(pattern, startPosition, null);

    public long RFind(ByteVector pattern) => RFind(pattern, 0);

    public void Insert(ByteVector data, long start, long replace)
    {
        ArgumentNullException.ThrowIfNull(data);
        Insert(data, data.Count, start, replace);
    }

    public void Insert(ByteVector data, long start) => Insert(data, start, 0);

    public void Insert(long size, long start) => Insert(null, size, start, 0);

    public void RemoveBlock(long start, long length)
    {
        ArgumentNullException.ThrowIfNull(_fileStream);

        if (length <= 0)
            return;

        Mode = AccessMode.Write;
        int buffer_length = _bufferSize;
        long read_position = start + length;
        long write_position = start;
        ByteVector buffer = 1;

        while (buffer.Count != 0)
        {
            _fileStream.Position = read_position;
            buffer = ReadBlock(buffer_length);
            read_position += buffer.Count;
            _fileStream.Position = write_position;
            WriteBlock(buffer);
            write_position += buffer.Count;
        }

        Truncate(write_position);
    }

    public void Seek(long offset, SeekOrigin origin)
    {
        ArgumentNullException.ThrowIfNull(_fileStream);

        if (Mode != AccessMode.Closed)
            _fileStream.Seek(offset, origin);
    }

    public void Seek(long offset) => Seek(offset, SeekOrigin.Begin);

    public static File Create(string path) => Create(path, null, ReadStyle.Average);

    public static File Create(IFileAbstraction abstraction) => Create(abstraction, null, ReadStyle.Average);

    public static File Create(string path, ReadStyle propertiesStyle) => Create(path, null, propertiesStyle);

    public static File Create(IFileAbstraction abstraction, ReadStyle propertiesStyle) => Create(abstraction, null, propertiesStyle);

    public static File Create(string path, string? mimetype, ReadStyle propertiesStyle) => Create(new LocalFileAbstraction(path), mimetype, propertiesStyle);

    public static File Create(IFileAbstraction abstraction, string? mimetype, ReadStyle propertiesStyle)
    {
        if (mimetype is null)
        {
            string ext = string.Empty;
            int index = abstraction.Name.LastIndexOf('.') + 1;

            if (index >= 1 && index < abstraction.Name.Length)
                ext = abstraction.Name.Substring(index, abstraction.Name.Length - index);

            mimetype = "taglib/" + ext.ToLowerInvariant();
        }

        foreach (var resolver in _fileTypeResolvers)
        {
            var file = resolver(abstraction, mimetype, propertiesStyle);

            if (file is not null)
                return file;
        }

        if (!FileTypes.AvailableTypes.TryGetValue(mimetype, out Type? value))
            throw new UnsupportedFormatException(string.Format(CultureInfo.InvariantCulture, "{0} ({1})", abstraction.Name, mimetype));

        var file_type = value;

        try
        {
            var file = (File)Activator.CreateInstance(file_type, new object[] { abstraction, propertiesStyle })
                ?? throw new InvalidOperationException();

            file.MimeType = mimetype;
            return file;
        }
        catch (System.Reflection.TargetInvocationException e)
        {
            throw e.InnerException;
        }
    }

    public static void AddFileTypeResolver(FileTypeResolver resolver)
    {
        if (resolver is not null)
            _fileTypeResolvers.Insert(0, resolver);
    }

    protected void PreSave()
    {
        if (!Writeable)
            throw new InvalidOperationException("File not writeable");

        if (PossiblyCorrupt)
            throw new CorruptFileException("Corrupted file cannot be saved");

        if (Tag?.Pictures is not null)
        {
            foreach (var pic in Tag.Pictures)
            {
                if (pic is ILazy lazy)
                    lazy.Load();
            }
        }
    }

    private void Insert(ByteVector? data, long size, long start, long replace)
    {
        ArgumentNullException.ThrowIfNull(_fileStream);
        Mode = AccessMode.Write;

        if (size == replace)
        {
            if (data is not null)
            {
                _fileStream.Position = start;
                WriteBlock(data);
            }

            return;
        }
        else if (size < replace)
        {
            if (data is not null)
            {
                _fileStream.Position = start;
                WriteBlock(data);
            }

            RemoveBlock(start + size, replace - size);
            return;
        }

        int buffer_length = (int)(size - replace);
        int modulo = buffer_length % _bufferSize;

        if (modulo != 0)
            buffer_length += _bufferSize - modulo;

        long read_position = start + replace;
        long write_position = start;
        byte[] buffer;
        byte[] about_to_overwrite;
        _fileStream.Position = read_position;
        about_to_overwrite = ReadBlock(buffer_length).Data;
        read_position += buffer_length;

        if (data is not null)
        {
            _fileStream.Position = write_position;
            WriteBlock(data);
        }
        else if (start + size > Length)
            _fileStream.SetLength(start + size);

        write_position += size;
        buffer = new byte[about_to_overwrite.Length];
        Array.Copy(about_to_overwrite, 0, buffer, 0, about_to_overwrite.Length);

        while (buffer_length != 0)
        {
            _fileStream.Position = read_position;
            int bytes_read = _fileStream.Read(about_to_overwrite, 0, buffer_length < about_to_overwrite.Length ? buffer_length : about_to_overwrite.Length);
            read_position += buffer_length;
            _fileStream.Position = write_position;
            _fileStream.Write(buffer, 0, buffer_length < buffer.Length ? buffer_length : buffer.Length);
            write_position += buffer_length;
            Array.Copy(about_to_overwrite, 0, buffer, 0, bytes_read);
            buffer_length = bytes_read;
        }
    }

    protected void Truncate(long length)
    {
        ArgumentNullException.ThrowIfNull(_fileStream);
        var old_mode = Mode;
        Mode = AccessMode.Write;
        _fileStream.SetLength(length);
        Mode = old_mode;
    }

    public class LocalFileAbstraction : IFileAbstraction
    {
        private readonly string _name;

        public LocalFileAbstraction(string path)
            => _name = path ?? throw new ArgumentNullException(nameof(path));

        public string Name => _name;

        public Stream ReadStream => System.IO.File.Open(Name, FileMode.Open, FileAccess.Read, FileShare.Read);

        public Stream WriteStream => System.IO.File.Open(Name, FileMode.Open, FileAccess.ReadWrite);

        public void CloseStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            stream.Close();
        }

    }

    public interface IFileAbstraction
    {
        string Name { get; }

        Stream ReadStream { get; }

        Stream WriteStream { get; }

        void CloseStream(Stream stream);
    }

}
