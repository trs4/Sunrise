using System.Runtime.Serialization;

namespace Sunrise.Model.Communication.Data;

[DataContract]
public class MediaLibraryData // %%TODO Заменить на Stream
{
    /// <summary>Информация о медиатеке</summary>
    [DataMember(Order = 1)]
    public byte[] Data { get; set; }
}
