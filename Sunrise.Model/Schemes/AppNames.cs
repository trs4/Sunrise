using RedLight;

namespace Sunrise.Model.Schemes;

public enum AppNames
{
    [IdentityColumn, PrimaryKey] Id,
    [Column(ColumnType.String, size: 255)] Name,
}
