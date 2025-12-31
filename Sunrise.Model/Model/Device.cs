namespace Sunrise.Model;

/// <summary>Устройство</summary>
public class Device
{
    /// <summary>Идентификатор</summary>
    public int Id { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Наименование</summary>
    public string Name { get; set; }

    /// <summary>Является основным</summary>
    public bool IsMain { get; set; }

    public override string ToString() => Name;
}
