using System.Runtime.Serialization;

namespace Sunrise.Model.Communication.Data;

[DataContract]
public class ConnectParameters
{
    [DataMember(Order = 1)]
    public string Name { get; set; }

    [DataMember(Order = 2)]
    public byte[] IPAddress { get; set; }
}
