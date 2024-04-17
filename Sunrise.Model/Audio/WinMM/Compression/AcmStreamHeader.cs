using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Compression;

internal class AcmStreamHeader : IDisposable
{
    private readonly AcmStreamHeaderStruct _streamHeader;
    private GCHandle _hSourceBuffer;
    private GCHandle _hDestBuffer;
    private readonly IntPtr _streamHandle;
    private bool _firstTime;

    public AcmStreamHeader(IntPtr streamHandle, int sourceBufferLength, int destBufferLength)
    {
        _streamHeader = new AcmStreamHeaderStruct();
        SourceBuffer = new byte[sourceBufferLength];
        _hSourceBuffer = GCHandle.Alloc(SourceBuffer, GCHandleType.Pinned);

        DestBuffer = new byte[destBufferLength];
        _hDestBuffer = GCHandle.Alloc(DestBuffer, GCHandleType.Pinned);

        _streamHandle = streamHandle;
        _firstTime = true;
        //Prepare();
    }

    private void Prepare()
    {
        if (SourceBuffer is null)
            throw new InvalidOperationException(nameof(SourceBuffer));

        if (DestBuffer is null)
            throw new InvalidOperationException(nameof(DestBuffer));

        _streamHeader.cbStruct = Marshal.SizeOf(_streamHeader);
        _streamHeader.sourceBufferLength = SourceBuffer.Length;
        _streamHeader.sourceBufferPointer = _hSourceBuffer.AddrOfPinnedObject();
        _streamHeader.destBufferLength = DestBuffer.Length;
        _streamHeader.destBufferPointer = _hDestBuffer.AddrOfPinnedObject();
        MmException.Try(AcmInterop.acmStreamPrepareHeader(_streamHandle, _streamHeader, 0), "acmStreamPrepareHeader");
    }

    private void Unprepare()
    {
        if (SourceBuffer is null || DestBuffer is null)
            return;

        _streamHeader.sourceBufferLength = SourceBuffer.Length;
        _streamHeader.sourceBufferPointer = _hSourceBuffer.AddrOfPinnedObject();
        _streamHeader.destBufferLength = DestBuffer.Length;
        _streamHeader.destBufferPointer = _hDestBuffer.AddrOfPinnedObject();

        MmResult result = AcmInterop.acmStreamUnprepareHeader(_streamHandle, _streamHeader, 0);

        if (result != MmResult.NoError) // if (result == MmResult.AcmHeaderUnprepared)
            throw new MmException(result, "acmStreamUnprepareHeader");
    }

    public void Reposition() => _firstTime = true;

    public int Convert(int bytesToConvert, out int sourceBytesConverted)
    {
        Prepare();

        try
        {
            _streamHeader.sourceBufferLength = bytesToConvert;
            _streamHeader.sourceBufferLengthUsed = bytesToConvert;
            AcmStreamConvertFlags flags = _firstTime ? (AcmStreamConvertFlags.Start | AcmStreamConvertFlags.BlockAlign) : AcmStreamConvertFlags.BlockAlign;
            MmException.Try(AcmInterop.acmStreamConvert(_streamHandle, _streamHeader, flags), "acmStreamConvert");
            _firstTime = false;
            //System.Diagnostics.Debug.Assert(streamHeader.destBufferLength == DestBuffer.Length, "Codecs should not change dest buffer length");
            sourceBytesConverted = _streamHeader.sourceBufferLengthUsed;
        }
        finally
        {
            Unprepare();
        }

        return _streamHeader.destBufferLengthUsed;
    }

    public byte[]? SourceBuffer { get; private set; }

    public byte[]? DestBuffer { get; private set; }

    #region IDisposable Members

    private bool _disposed = false;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            //Unprepare();
            SourceBuffer = null;
            DestBuffer = null;
            _hSourceBuffer.Free();
            _hDestBuffer.Free();
        }

        _disposed = true;
    }

    ~AcmStreamHeader()
    {
        System.Diagnostics.Debug.Assert(false, "AcmStreamHeader dispose was not called");
        Dispose(false);
    }

    #endregion
}
