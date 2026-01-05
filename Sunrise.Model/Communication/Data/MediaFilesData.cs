using System.Runtime.Serialization;

namespace Sunrise.Model.Communication.Data;

[DataContract]
public class MediaFilesData
{
    [DataMember(Order = 1)]
    public List<string> FilePaths { get; set; }
}
