using System.Net;

namespace Sunrise.Model.Discovery;

public sealed class DiscoveryDeviceInfo
{
    private readonly SearchMessage _message;

    internal DiscoveryDeviceInfo(SearchMessage message)
        => _message = message ?? throw new ArgumentNullException(nameof(message));

    public string DeviceName => _message.DeviceName;

    public IPAddress? IPAddress => _message.IPAddress is null ? null : new IPAddress(_message.IPAddress);

    public int Port => _message.Port;

    public DateTime CreationDate { get; } = DateTime.Now;

    public override int GetHashCode() => DeviceName.GetHashCode();

    public override bool Equals(object? obj) => obj is DiscoveryDeviceInfo other && DeviceName == other.DeviceName;

    public override string ToString() => DeviceName;
}
