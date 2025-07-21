using RedLight;

namespace Sunrise.Model.Schemes;

public enum PlaylistCategories
{
    [Column(ColumnType.Integer)] PlaylistId,
    [Column(ColumnType.Integer)] CategoryId,
}
