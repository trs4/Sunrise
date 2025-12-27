using System.Runtime.Serialization;
using IcyRain.Tables;

namespace Sunrise.Model.Communication.Data;

[DataContract]
public class MediaLibraryData
{
    /// <summary>Треки</summary>
    [DataMember(Order = 1)]
    public DataTable Tracks { get; set; }

    /// <summary>Плейлисты</summary>
    [DataMember(Order = 2)]
    public DataTable Playlists { get; set; }

    /// <summary>Треки в плейлистах</summary>
    [DataMember(Order = 3)]
    public DataTable PlaylistTracks { get; set; }

    /// <summary>Категории</summary>
    [DataMember(Order = 4)]
    public DataTable Categories { get; set; }
}
