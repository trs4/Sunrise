using RedLight;

namespace Sunrise.Model.Schemes;

public enum Updates
{
    [IdentityColumn, PrimaryKey] Id,
    [Column(ColumnType.Integer)] Version,
    [Column(ColumnType.DateTime)] Date,
}
