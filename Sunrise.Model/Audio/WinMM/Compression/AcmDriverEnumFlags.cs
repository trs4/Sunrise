﻿namespace Sunrise.Model.Audio.Compression;

[Flags]
internal enum AcmDriverEnumFlags
{
    /// <summary>ACM_DRIVERENUMF_NOLOCAL, Only global drivers should be included in the enumeration</summary>
    NoLocal = 0x40000000,

    /// <summary>ACM_DRIVERENUMF_DISABLED, Disabled ACM drivers should be included in the enumeration</summary>
    Disabled = unchecked((int)0x80000000),
}
