using IcyRain;

namespace Sunrise.Model.Discovery;

internal sealed class DiscoveryServerInfo
{
    public DiscoveryServerInfo(NotifyMessage message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Data = Serialization.SerializeSegment<SearchMessage>(message);
    }

    public NotifyMessage Message { get; }

    public ArraySegment<byte> Data { get; }
}
