namespace Sunrise.Services;

public interface ISystemDialogsService
{
    bool ShowSelectFolder(out string? folderPath);

    bool ShowSelectFile(out string? folderPath);
}
