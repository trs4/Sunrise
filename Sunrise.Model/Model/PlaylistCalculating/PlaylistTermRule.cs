namespace Sunrise.Model;

public class PlaylistTermRule
{
    public PlaylistParameter Parameter { get; set; }

    public PlaylistParameterOperator Operator { get; set; }

    public object? Value { get; set; }
}
