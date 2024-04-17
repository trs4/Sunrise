using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sunrise.ViewModels;

public sealed class MediaFolderViewModel : ObservableObject
{
    public MediaFolderViewModel(string folderPath)
        => FolderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));

    public string FolderPath { get; }

    public override string ToString() => FolderPath;
}
