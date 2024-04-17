using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Model;
using Sunrise.Services;
using Sunrise.Utils;
using Sunrise.Views;

namespace Sunrise.ViewModels;

public sealed class MediaFoldersViewModel : ObservableObject
{
    private readonly Player _player;
    private MediaFolderViewModel _selectedFolder;

    public MediaFoldersViewModel(Player player)
    {
        _player = player ?? throw new ArgumentNullException(nameof(player));
        AddCommand = new AsyncRelayCommand(AddFolderAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteFolderAsync, CanDeleteFolder);
    }

    public IRelayCommand AddCommand { get; }

    public IRelayCommand DeleteCommand { get; }

    public ObservableCollection<MediaFolderViewModel> Folders { get; } = [];

    public MediaFolderViewModel SelectedFolder
    {
        get => _selectedFolder;
        set => SetProperty(ref _selectedFolder, value);
    }

    public static async Task ShowAsync(Window owner, Player player, CancellationToken token)
    {
        var folders = await player.GetFoldersAsync(token);
        var foldersViewModel = new MediaFoldersViewModel(player);

        foreach (string folderPath in folders)
            foldersViewModel.Folders.Add(new MediaFolderViewModel(folderPath));

        var dialog = new MediaFoldersWindow() { DataContext = foldersViewModel };
        await dialog.ShowDialog(owner);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedFolder))
            DeleteCommand.NotifyCanExecuteChanged();
    }

    private async Task AddFolderAsync(CancellationToken token)
    {
        string? folderPath = null;

        if (!UIDispatcher.Run(() => AppServices.Get<ISystemDialogsService>().ShowSelectFolder(out folderPath)))
            return;

        if (!await _player.AddFolderAsync(folderPath, new ProgressWindowWrapper(), token))
            return;

        var folderViewModel = new MediaFolderViewModel(folderPath);
        Folders.Add(folderViewModel);
        SelectedFolder = folderViewModel;
    }

    private bool CanDeleteFolder() => _selectedFolder is not null;

    private async Task DeleteFolderAsync(CancellationToken token)
    {
        var folderViewModel = _selectedFolder;

        if (folderViewModel is null)
            return;

        await _player.DeleteFolderAsync(folderViewModel.FolderPath, token);
        Folders.Remove(folderViewModel);
    }

}
