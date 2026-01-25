using RedLight;
using Sunrise.Model.Resources;

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

public static class PlaylistParameterOperatorExtensions
{
    public static string GetName(this PlaylistParameterOperator @operator) => @operator switch
    {
        PlaylistParameterOperator.Equal => Texts.Equal,
        PlaylistParameterOperator.NotEqual => Texts.NotEqual,
        PlaylistParameterOperator.GreaterThan => Texts.GreaterThan,
        PlaylistParameterOperator.GreaterThanOrEqual => Texts.GreaterThanOrEqual,
        PlaylistParameterOperator.LessThan => Texts.LessThan,
        PlaylistParameterOperator.LessThanOrEqual => Texts.LessThanOrEqual,
        _ => throw new NotSupportedException(@operator.ToString()),
    };
}
