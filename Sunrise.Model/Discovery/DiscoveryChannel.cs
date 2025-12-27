using System.Buffers;
using System.Net;
using System.Net.Sockets;
using IcyRain;
using Sunrise.Model.Common;

namespace Sunrise.Model.Discovery;

public abstract class DiscoveryChannel : IDisposable
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly object _socketCreateSync = new();
    private readonly object _socketSync = new();
    private DiscoverySocket? _socket;
    private CancellationTokenSource? _cts;
    private readonly object _broadcastListenSocketCreateSync = new();
    private readonly object _broadcastListenSocketSync = new();
    private DiscoverySocket? _broadcastListenSocket;
#pragma warning restore CA2213 // Disposable fields should be disposed

    public bool IsDisposed { get; private set; }

    protected void BeginListeningForBroadcasts()
    {
        var socket = _broadcastListenSocket;

        if (socket is not null)
            return;

        lock (_broadcastListenSocketCreateSync)
        {
            if (_broadcastListenSocket is not null)
                return;

            socket = _broadcastListenSocket = DiscoverySocketCreator.CreateMulticast();
        }

        Listen(socket, _broadcastListenSocketSync);
    }

    protected void StopListeningForBroadcasts()
        => Tasks.StartOnDefaultScheduler(OnStopListeningForBroadcasts);

    private void OnStopListeningForBroadcasts()
    {
        _broadcastListenSocket?.Close();
        _broadcastListenSocket = null;
    }

    protected void Send(ArraySegment<byte> message)
        => Tasks.StartOnDefaultScheduler(OnSend, message);

    internal DiscoverySocket GetSocket(out bool create)
    {
        create = false;
        var socket = _socket;

        if (socket is not null)
            return socket;

        lock (_socketCreateSync)
        {
            socket = _socket;

            if (socket is not null)
                return socket;

            socket = _socket = DiscoverySocketCreator.Create();
            ReleaseToken();
            _cts = new();
            create = true;
        }

        return socket;
    }

    private void OnSend(ArraySegment<byte> message)
    {
        if (IsDisposed)
            return;

        var socket = GetSocket(out bool create);

        if (create)
            Listen(socket, _socketSync);

        try
        {
            socket.SendTo(message);
        }
        catch
        {
            Network.ClearCache();
            socket.Dispose();
            _socket = null;
            ReleaseToken();
        }
    }

    protected void StopListeningForResponses()
        => Tasks.StartOnDefaultScheduler(OnStopListeningForResponses);

    protected void OnStopListeningForResponses()
    {
        _socket?.Close();
        _socket = null;
        ReleaseToken();
    }

    private void ReleaseToken()
    {
        var cts = _cts;

        if (cts is null)
            return;

        cts.Cancel();
        cts.Dispose();
        _cts = null;
    }

    private void Listen(DiscoverySocket socket, object sync)
        => Tasks.StartOnDefaultScheduler(OnListen, socket, sync);

    private async void OnListen(DiscoverySocket socket, object sync)
    {
        while (true)
        {
            int receivedBytes;
            EndPoint endPoint = DiscoveryConsts.AnyEndPoint;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(DiscoveryConsts.DefaultUdpSocketBufferSize);

            try
            {
                if (IsDisposed || socket.IsClosed)
                    break;

                var udpBuffer = new ArraySegment<byte>(buffer, 0, DiscoveryConsts.DefaultUdpSocketBufferSize);
                var token = _cts?.Token ?? default;

                var result = await socket.Socket.ReceiveFromAsync(
                    udpBuffer, SocketFlags.None, endPoint, token).ConfigureAwait(true);

                if (token.IsCancellationRequested)
                    continue;

                receivedBytes = result.ReceivedBytes;
                endPoint = result.RemoteEndPoint;

                if (receivedBytes <= 0)
                    continue;

                var message = Serialization.Deserialize<SearchMessage>(buffer, 0, receivedBytes);

                if (string.IsNullOrWhiteSpace(message?.Name) || string.IsNullOrWhiteSpace(message?.DeviceName) || IsDisposed)
                    continue;

                OnListenSendResponse(message);
            }
            catch
            {
                break;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    protected abstract void OnListenSendResponse(SearchMessage message);

    protected virtual void OnDispose()
    {
        if (_broadcastListenSocket is not null)
            StopListeningForBroadcasts();

        if (_socket is not null)
            StopListeningForResponses();
    }

    public void Dispose()
    {
        try
        {
            IsDisposed = true;
            OnDispose();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

}
