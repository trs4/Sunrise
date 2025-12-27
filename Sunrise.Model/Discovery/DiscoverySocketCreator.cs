using System.Net;
using System.Net.Sockets;
using Sunrise.Model.Common;

namespace Sunrise.Model.Discovery;

internal static class DiscoverySocketCreator
{
    public static DiscoverySocket Create()
    {
        var ipAddress = Network.GetMachineIPAddress();
        var socket = CreateSocket(ipAddress);

        try
        {
            return CreateWrapper(socket, ipAddress);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    public static DiscoverySocket CreateMulticast()
    {
        var ipAddress = Network.GetMachineIPAddress();
        var socket = CreateSocket(ipAddress);

        try
        {
            socket.ExclusiveAddressUse = false;

            socket.MulticastLoopback = true;
            return CreateWrapper(socket, ipAddress);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    private static Socket CreateSocket(IPAddress ipAddress)
        => new Socket(ipAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

    private static DiscoverySocket CreateWrapper(Socket socket, IPAddress ipAddress)
    {
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, DiscoveryConsts.DefaultMulticastTimeToLive);
        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(DiscoveryConsts.MulticastIPAddress, ipAddress));
        return new DiscoverySocket(socket, ipAddress);
    }

}
