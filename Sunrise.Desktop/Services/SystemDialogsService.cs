using System;
using Microsoft.Win32;
using Sunrise.Model.Resources;
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

    public bool SaveFile(out string? filePath)
    {
        var dialog = new SaveFileDialog()
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            FileName = Texts.MediaLibrary + ".json",
            Filter = "Json (*.json)|*.json",
        };

        bool result = dialog.ShowDialog().GetValueOrDefault();
        filePath = result ? dialog.FileName : null;
        return result;
    }

    //private static Window? GetOwnerWindow()
    //{
    //    if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //        return desktop.Windows.FirstOrDefault(t => t.IsActive) ?? desktop.MainWindow;

    //    return null;
    //}

}
