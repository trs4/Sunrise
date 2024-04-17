using RedLight;

namespace Sunrise.Model.Schemes;

public enum TrackPictures
{
    [Column(ColumnType.Integer), PrimaryKey] Id,
    [Column(ColumnType.String, size: 255)] MimeType,
    [Column(ColumnType.ByteArray)] Data,
}
