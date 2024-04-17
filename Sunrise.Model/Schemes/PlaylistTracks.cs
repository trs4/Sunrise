using RedLight;

namespace Sunrise.Model.Schemes;

public enum PlaylistTracks
{
    [Column(ColumnType.Integer)] PlaylistId,
    [Column(ColumnType.Integer)] TrackId,
    [Column(ColumnType.Integer)] Position,
}
