using Microsoft.Win32;
using Sunrise.Services;

namespace Sunrise.Desktop.Services;

internal sealed class SystemDialogsService : ISystemDialogsService
{
    public bool ShowSelectFolder(out string? folderPath)
    {
        var dialog = new OpenFolderDialog();
        bool result = dialog.ShowDialog().GetValueOrDefault();
        folderPath = result ? dialog.FolderName : null;
        return result;
    }

    public bool ShowSelectFile(out string? folderPath)
    {
        var dialog = new OpenFileDialog();
        bool result = dialog.ShowDialog().GetValueOrDefault();
        folderPath = result ? dialog.FileName : null;
        return result;
    }

    //private static Window? GetOwnerWindow()
    //{
    //    if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //        return desktop.Windows.FirstOrDefault(t => t.IsActive) ?? desktop.MainWindow;

    //    return null;
    //}

}
