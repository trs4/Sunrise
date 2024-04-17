namespace Sunrise.Model.TagLib;

public interface ILazy
{
    bool IsLoaded { get; }

    void Load();
}
