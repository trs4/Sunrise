using System.Net;

namespace Sunrise.Model.Discovery;

internal static class DiscoveryConsts
{
    public const string Name = "Sunrise Player";

    public const int Port = 21_450;

    public static readonly IPAddress MulticastIPAddress = IPAddress.Parse("239.255.255.250");

    public static readonly IPEndPoint MulticastEndPoint = new IPEndPoint(MulticastIPAddress, Port);

    public static readonly IPEndPoint AnyEndPoint = new IPEndPoint(IPAddress.Any, 0);

    public const int DefaultMulticastTimeToLive = 4;

    public const int DefaultUdpSocketBufferSize = 1024; // %%TODO

    public static readonly TimeSpan BroadcastTime = TimeSpan.FromSeconds(5);
}
