namespace Sunrise.Model;

/// <summary>Приложение</summary>
public sealed class AppName
{
    /// <summary>Идентификатор</summary>
    public int Id { get; set; }

    /// <summary>Наименование</summary>
    public string Name { get; set; }

    public override string ToString() => Name;
}
