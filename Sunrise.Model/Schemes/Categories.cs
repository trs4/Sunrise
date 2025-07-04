﻿using RedLight;

namespace Sunrise.Model.Schemes;

public enum Categories
{
    [IdentityColumn, PrimaryKey] Id,
    [Column(ColumnType.Guid)] Guid,
    [Column(ColumnType.String, size: 255)] Name,
}
