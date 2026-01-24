using RedLight;

namespace Sunrise.Model;

public enum PlaylistParameterOperator
{
    /// <summary>Равно</summary>
    Equal = Op.Equal,

    /// <summary>Не равно</summary>
    NotEqual = Op.NotEqual,

    /// <summary>Больше</summary>
    GreaterThan = Op.GreaterThan,

    /// <summary>Больше или равно</summary>
    GreaterThanOrEqual = Op.GreaterThanOrEqual,

    /// <summary>Меньше</summary>
    LessThan = Op.LessThan,

    /// <summary>Меньше или равно</summary>
    LessThanOrEqual = Op.LessThanOrEqual,
}
