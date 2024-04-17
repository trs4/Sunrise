using RedLight;

namespace Sunrise.Model.Schemes;

public enum Devices
{
    [IdentityColumn, PrimaryKey] Id,
    [Column(ColumnType.Guid)] Guid,
    [Column(ColumnType.String, size: 255)] Name,
    [Column(ColumnType.Boolean)] IsMain,
}
