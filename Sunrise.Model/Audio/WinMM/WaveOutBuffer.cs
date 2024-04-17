﻿using System.Runtime.InteropServices;
using Sunrise.Model.Audio.Wave;

namespace Sunrise.Model.Audio;

/// <summary>A buffer of Wave samples for streaming to a Wave Output device</summary>
public sealed class WaveOutBuffer : IDisposable
{
    private readonly WaveHeader _header;
    private readonly int _bufferSize; // allocated bytes, may not be the same as bytes read
    private readonly byte[] _buffer;
    private readonly IWaveProvider _waveStream;
    private readonly object _waveOutLock;
    private GCHandle _hBuffer;
    private IntPtr _hWaveOut;
    private GCHandle _hHeader; // we need to pin the header structure
    private GCHandle _hThis; // for the user callback

    /// <summary>Creates a new wavebuffer</summary>
    /// <param name="hWaveOut">WaveOut device to write to</param>
    /// <param name="bufferSize">Buffer size in bytes</param>
    /// <param name="bufferFillStream">Stream to provide more data</param>
    /// <param name="waveOutLock">Lock to protect WaveOut API's from being called on >1 thread</param>
    public WaveOutBuffer(IntPtr hWaveOut, int bufferSize, IWaveProvider bufferFillStream, object waveOutLock)
    {
        _bufferSize = bufferSize;
        _buffer = new byte[bufferSize];
        _hBuffer = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        _hWaveOut = hWaveOut;
        _waveStream = bufferFillStream;
        _waveOutLock = waveOutLock;

        _header = new WaveHeader();
        _hHeader = GCHandle.Alloc(_header, GCHandleType.Pinned);
        _header.dataBuffer = _hBuffer.AddrOfPinnedObject();
        _header.bufferLength = bufferSize;
        _header.loops = 1;
        _hThis = GCHandle.Alloc(this);
        _header.userData = (IntPtr)_hThis;

        lock (waveOutLock)
            MmException.Try(WaveInterop.waveOutPrepareHeader(hWaveOut, _header, Marshal.SizeOf(_header)), "waveOutPrepareHeader");
    }

    #region Dispose Pattern

    /// <summary>Finalizer for this wave buffer</summary>
    ~WaveOutBuffer()
    {
        OnDispose(false);
        System.Diagnostics.Debug.Assert(true, "WaveBuffer was not disposed");
    }

    /// <summary>Releases resources held by this WaveBuffer</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        OnDispose(true);
    }

    /// <summary>Releases resources held by this WaveBuffer</summary>
    private void OnDispose(bool disposing)
    {
        if (_hHeader.IsAllocated)
            _hHeader.Free();

        if (_hBuffer.IsAllocated)
            _hBuffer.Free();

        if (_hThis.IsAllocated)
            _hThis.Free();

        if (_hWaveOut != IntPtr.Zero)
        {
            lock (_waveOutLock)
                WaveInterop.waveOutUnprepareHeader(_hWaveOut, _header, Marshal.SizeOf(_header));

            _hWaveOut = IntPtr.Zero;
        }
    }

    #endregion

    /// this is called by the WAVE callback and should be used to refill the buffer
    public bool OnDone()
    {
        int bytes;

        lock (_waveStream)
            bytes = _waveStream.Read(_buffer, 0, _buffer.Length);

        if (bytes == 0)
            return false;

        for (int n = bytes; n < _buffer.Length; n++)
            _buffer[n] = 0;

        WriteToWaveOut();
        return true;
    }

    /// <summary>Whether the header's in queue flag is set</summary>
    public bool InQueue => (_header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;

    /// <summary>The buffer size in bytes</summary>
    public int BufferSize => _bufferSize;

    private void WriteToWaveOut()
    {
        MmResult result;

        lock (_waveOutLock)
            result = WaveInterop.waveOutWrite(_hWaveOut, _header, Marshal.SizeOf(_header));

        if (result != MmResult.NoError)
            throw new MmException(result, "waveOutWrite");

        GC.KeepAlive(this);
    }

}
