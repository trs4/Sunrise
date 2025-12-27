using System.Runtime.Serialization;

namespace Sunrise.Model.Discovery;

[DataContract, KnownType(typeof(NotifyMessage))]
public class SearchMessage
{
    [DataMember(Order = 1)]
    public string Name { get; set; }

    [DataMember(Order = 2)]
    public string DeviceName { get; set; }

    [DataMember(Order = 3)]
    public byte[]? IPAddress { get; set; }

    [DataMember(Order = 4)]
    public int Port { get; set; }
}
