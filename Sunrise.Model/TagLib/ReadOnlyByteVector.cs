namespace Sunrise.Model.TagLib;

public sealed class ReadOnlyByteVector : ByteVector
{
    public ReadOnlyByteVector() { }

    public ReadOnlyByteVector(int size, byte value) : base(size, value) { }

    public ReadOnlyByteVector(int size) : this(size, 0) { }

    public ReadOnlyByteVector(ByteVector vector) : base(vector) { }

    public ReadOnlyByteVector(byte[] data, int length) : base(data, length) { }

    public ReadOnlyByteVector(params byte[] data) : base(data) { }

    public static implicit operator ReadOnlyByteVector(byte value) => new ReadOnlyByteVector(value);

    public static implicit operator ReadOnlyByteVector(byte[] value) => new ReadOnlyByteVector(value);

    public static implicit operator ReadOnlyByteVector(string value) => new ReadOnlyByteVector(FromString(value, StringType.UTF8));

    public override bool IsReadOnly => true;

    public override bool IsFixedSize => true;
}
