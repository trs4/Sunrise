using RedLight;

namespace Sunrise.Model.Schemes;

public enum Tracks
{
    [IdentityColumn, PrimaryKey] Id,
    [Column(ColumnType.Guid)] Guid,
    [Column(ColumnType.String, size: 255)] Path,
    [Column(ColumnType.Boolean, defaultValue: "true")] Picked,
    [Column(ColumnType.String, size: 255)] Title,
    [Column(ColumnType.Integer, nullable: true)] Year,
    [Column(ColumnType.TimeSpan)] Duration,
    [Column(ColumnType.Byte)] Rating,
    [Column(ColumnType.String, size: 255)] Artist,
    [Column(ColumnType.String, size: 255)] Artists,
    [Column(ColumnType.String, size: 255)] Genre,
    [Column(ColumnType.DateTime, nullable: true)] LastPlay,
    [Column(ColumnType.Integer)] Reproduced,
    [Column(ColumnType.Integer)] SelfReproduced,
    [Column(ColumnType.String, size: 255)] Album,
    [Column(ColumnType.DateTime)] Created,
    [Column(ColumnType.DateTime)] Added,
    [Column(ColumnType.Integer)] Bitrate,
    [Column(ColumnType.Long)] Size,
    [Column(ColumnType.DateTime)] LastWrite,
    [Column(ColumnType.Boolean)] HasPicture,
    [Column(ColumnType.String, size: 255)] RootFolder,
    [Column(ColumnType.String, size: 255)] RelationFolder,
    [Column(ColumnType.String, size: 255)] OriginalText,
    [Column(ColumnType.String, size: 255)] TranslateText,
    [Column(ColumnType.String, size: 255)] Language,
}
