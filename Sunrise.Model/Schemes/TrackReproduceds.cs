using RedLight;

namespace Sunrise.Model.Schemes;

public enum TrackReproduceds
{
    [Column(ColumnType.Integer), PrimaryKey(nameof(AppId), nameof(TrackId))] AppId,
    [Column(ColumnType.Integer)] TrackId,
    [Column(ColumnType.Integer)] Reproduced,
}
