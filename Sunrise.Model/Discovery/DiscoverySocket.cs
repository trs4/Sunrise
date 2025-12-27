using System.Net;
using System.Net.Sockets;

namespace Sunrise.Model.Discovery;

internal sealed class DiscoverySocket : IDisposable
{
    private readonly IPEndPoint _endPoint;
    private readonly object _closeSync = new();
    private bool _isClosed;

    public DiscoverySocket(Socket socket, IPAddress ipAddress)
    {
        Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        _endPoint = new IPEndPoint(ipAddress, DiscoveryConsts.Port);
        socket.Bind(_endPoint);
    }

    public Socket Socket { get; }

    public bool IsClosed => _isClosed || IsDisposed;

    public bool IsDisposed { get; private set; }

    public void SendTo(ArraySegment<byte> message)
    {
        if (IsDisposed)
            return;

        Socket.SendTo(message.Array, message.Offset, message.Count, SocketFlags.None, DiscoveryConsts.MulticastEndPoint);
    }

    public void SendTo(ArraySegment<byte> message, IPAddress ipAddress)
    {
        if (IsDisposed)
            return;

        Socket.SendTo(message.Array, message.Offset, message.Count, SocketFlags.None, new IPEndPoint(ipAddress, DiscoveryConsts.Port));
    }

    public void Close()
    {
        if (_isClosed)
            return;

        lock (_closeSync)
        {
            if (_isClosed)
                return;

            _isClosed = true;

            try
            {
                if (!IsDisposed)
                {
                    Socket.SendTo([], 0, 0, SocketFlags.None, _endPoint);
                    Thread.Sleep(50);
                }

                Socket.Dispose();
            }
            catch { }
        }
    }

    public void Dispose()
    {
        try
        {
            IsDisposed = true;
            Socket.Dispose();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

}
