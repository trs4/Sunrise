using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Sunrise.Converters;
using Sunrise.Model;
using Sunrise.Model.Resources;
using Sunrise.ViewModels.Columns;

namespace Sunrise.ViewModels;

public sealed class MainDesktopViewModel : MainViewModel
{
    public MainDesktopViewModel() { } // For designer

    public MainDesktopViewModel(Player player)
        : base(player)
    {
        AddFolderCommand = new AsyncRelayCommand(AddFolderAsync);
        DoubleClickCommand = new RelayCommand<TrackViewModel>(OnDoubleClick);
        InitTracksColumns();
    }

    public Window Owner { get; internal set; }

    public IRelayCommand AddFolderCommand { get; }

    public IRelayCommand DoubleClickCommand { get; }

    public ObservableCollection<ColumnViewModel> TracksColumns { get; } = [];

    protected override TrackPlayViewModel CreateTrackPlay(Player player) => new TrackPlayDesktopViewModel(this, player);

    protected override Task SelectTracksAsync(object tracksOwner, bool changeTracks = true, CancellationToken token = default)
    {
        var pickedColumn = TracksColumns.First(c => c.Name == nameof(TrackViewModel.Picked));
        pickedColumn.IsVisible = tracksOwner is SongsRubricViewModel;

        return base.SelectTracksAsync(tracksOwner, changeTracks, token);
    }

    private async Task AddFolderAsync(CancellationToken token)
    {
        await MediaFoldersViewModel.ShowAsync(Owner, TrackPlay.Player, token);

        if (TrackPlay.Player.IsTracksLoaded())
            return;

        await ReloadTracksAsync(token);
    }

    private async void OnDoubleClick(TrackViewModel? trackViewModel)
    {
        if (trackViewModel is not null)
            await TrackPlay.PlayItBeginAsync(trackViewModel);
    }

    private void InitTracksColumns()
    {
        TracksColumns.Add(new IsPlayingColumnViewModel());
        TracksColumns.Add(new CheckedColumnViewModel<TrackViewModel>(nameof(TrackViewModel.Picked), t => t.Picked, (t, v) => t.Picked = v));

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Title), t => t.Title ?? string.Empty)
        {
            Caption = Texts.Title,
            Width = 230,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, int?>(nameof(TrackViewModel.Year), t => t.Year)
        {
            Caption = Texts.Year,
            Width = 50,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Duration), t => DurationConverter.Convert(t.Duration))
        {
            Caption = Texts.Duration,
            Width = 50,
        });

        TracksColumns.Add(new RatingColumnViewModel());

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Artist), t => t.Artist ?? string.Empty)
        {
            Caption = Texts.Artist,
            Width = 200,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Genre), t => t.Genre ?? string.Empty)
        {
            Caption = Texts.Genre,
            Width = 100,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, int>(nameof(TrackViewModel.Reproduced), t => t.Reproduced)
        {
            Caption = Texts.Reproduced,
            Width = 100,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Album), t => t.Album ?? string.Empty)
        {
            Caption = Texts.Album,
            Width = 200,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Created), t => t.Created.ToString("g"))
        {
            Caption = Texts.Created,
            Width = 120,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, string>(nameof(TrackViewModel.Added), t => t.Added.ToString("g"))
        {
            Caption = Texts.Added,
            Width = 100,
            IsVisible = false,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, int>(nameof(TrackViewModel.Bitrate), t => (int)t.Bitrate)
        {
            Caption = Texts.Bitrate,
            Width = 100,
            IsVisible = false,
        });

        TracksColumns.Add(new TextColumnViewModel<TrackViewModel, long>(nameof(TrackViewModel.Size), t => t.Size)
        {
            Caption = Texts.Size,
            Width = 100,
            IsVisible = false,
        });
    }

    protected override async Task DeletePlaylistAsync()
    {
        var selectedPlaylist = SelectedPlaylist;

        if (!await TrackPlay.Player.DeletePlaylistAsync(selectedPlaylist?.Playlist))
            return;

        SelectedPlaylist = null;
        Playlists.Remove(selectedPlaylist);
        await SelectSongsAsync();
    }

    protected override async Task DeleteCategoryAsync()
    {
        var selectedCategory = SelectedCategory;

        if (!await TrackPlay.Player.DeleteCategoryAsync(selectedCategory?.Category))
            return;

        SelectedCategory = null;
        Categories.Remove(selectedCategory);
    }

    public override Task OnNextListAsync() => Task.CompletedTask;

    public override void OnExit() => Owner.Close();
}
