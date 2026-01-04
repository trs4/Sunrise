using Grpc.Core;
using IcyRain.Tables;
using Sunrise.Model.Communication.Data;
using Sunrise.Model.Discovery;
using Sunrise.Model.Model;
using Sunrise.Model.Model.Exchange;
using Sunrise.Model.Schemes;

namespace Sunrise.Model.Communication;

public sealed class SyncDispatcher
{
    private readonly Action<DiscoveryDeviceInfo>? _onDeviceDetected;
    private IServerStreamWriter<SubscriptionTicket>? _subscription;
    private TaskCompletionSource<bool>? _waitEvent;
    private bool _isSynchronizing;
    private MediaLibraryData? _mediaLibraryData;

    public SyncDispatcher(Player player, Action<DiscoveryDeviceInfo>? onDeviceDetected = null)
    { 
        Player = player ?? throw new ArgumentNullException(nameof(player));
        _onDeviceDetected = onDeviceDetected;
    }

    public Player Player { get; }

    public bool IsInitialized { get; private set; }

    public TaskCompletionSource<bool> Initialize(DiscoveryDeviceInfo deviceInfo, IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (IsInitialized)
            throw new InvalidOperationException(nameof(IsInitialized));

        _onDeviceDetected?.Invoke(deviceInfo);
        IsInitialized = true;
        _subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        var waitEvent = new TaskCompletionSource<bool>();

        token.Register(() =>
        {
            try
            {
                waitEvent.SetResult(true);
            }
            catch { }
        });

        return _waitEvent = waitEvent;
    }

    public Task SynchronizeAsync(CancellationToken token = default)
        => SynchronizeCoreAsync(GetUpdatingMediaAsync, token);
    
    public Task SynchronizePlaylistsAsync(CancellationToken token = default)
        => SynchronizeCoreAsync(GetUpdatingPlaylistsMediaAsync, token);

    private async Task SynchronizeCoreAsync(Func<TracksScreenshot, Dictionary<string, Playlist>, CategoriesScreenshot,
        ExistingMedia, Task<UpdatingMedia>> getUpdatingMediaAsync, CancellationToken token)
    {
        var subscription = _subscription;

        if (_isSynchronizing || subscription is null)
            return;

        _isSynchronizing = true;

        try
        {
            var existingMedia = await GetExistingMediaAsync(subscription, token);

            var tracksScreenshot = await Player.GetTracksAsync(token);
            var playlists = await Player.GetPlaylistsAsync(token);
            var categoriesScreenshot = await Player.GetCategoriesAsync(token);

            var updatingMedia = await getUpdatingMediaAsync(tracksScreenshot, playlists, categoriesScreenshot, existingMedia);

            await UploadTracksWithFilesAsync(updatingMedia.Tracks, subscription, token);
            await UploadPlaylistsAsync(updatingMedia.Playlists, subscription, token);
            await UploadCategoriesAsync(updatingMedia.Categories, subscription, token);
        }
        finally
        {
            _isSynchronizing = false;
        }
    }

#pragma warning disable CA1822 // Пометьте члены как статические
    public Task ClearAsync(CancellationToken token = default)
#pragma warning restore CA1822 // Пометьте члены как статические
    {
        // %%TODO

        return Task.CompletedTask;
    }

    private async Task<MediaLibraryData> LoadMediaLibraryAsync(IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        await subscription.WriteAsync(new MediaLibraryTicket(), token);

        while (true)
        {
            await Task.Delay(25, token).ConfigureAwait(false);
            var mediaLibraryData = _mediaLibraryData;

            if (mediaLibraryData is null)
                continue;

            _mediaLibraryData = null;
            return mediaLibraryData;
        }
    }

    public void SetMediaLibrary(MediaLibraryData data) => _mediaLibraryData = data;

    private async Task<ExistingMedia> GetExistingMediaAsync(IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        var data = await LoadMediaLibraryAsync(subscription, token);
        var stream = new MemoryStream(data.Data);
        var document = MediaExporter.Deserialize(stream) ?? throw new InvalidOperationException(nameof(MediaExporter));

        var existingTracks = new Dictionary<Guid, TrackElement>(document.Tracks?.Count ?? 0);
        var existingPlaylists = new Dictionary<Guid, PlaylistElement>(document.Playlists?.Count ?? 0);
        var existingCategories = new Dictionary<Guid, CategoryElement>(document.Categories?.Count ?? 0);

        foreach (var track in document.Tracks ?? [])
            existingTracks[track.Guid] = track;

        foreach (var playlist in document.Playlists ?? [])
            existingPlaylists[playlist.Guid] = playlist;

        foreach (var category in document.Categories ?? [])
            existingCategories[category.Guid] = category;

        return new(existingTracks, existingPlaylists, existingCategories);
    }

    private static async Task<UpdatingMedia> GetUpdatingMediaAsync(TracksScreenshot tracksScreenshot, Dictionary<string, Playlist> playlists,
        CategoriesScreenshot categoriesScreenshot, ExistingMedia existingMedia)
    {
        var updatingTracks = new List<Track>(tracksScreenshot.Tracks.Count);
        var updatingPlaylists = new List<Playlist>(playlists.Count);
        var updatingCategories = new List<Category>(categoriesScreenshot.Categories.Count);

        foreach (var track in tracksScreenshot.Tracks)
        {
            if (existingMedia.Tracks.TryGetValue(track.Guid, out var existingTrack) && track.Updated == existingTrack.Updated)
                continue;

            updatingTracks.Add(track);
        }

        foreach (var playlist in playlists.Values)
        {
            if (existingMedia.Playlists.TryGetValue(playlist.Guid, out var existingPlaylist) && playlist.Updated == existingPlaylist.Updated)
                continue;

            updatingPlaylists.Add(playlist);
        }

        foreach (var category in categoriesScreenshot.Categories)
        {
            if (existingMedia.Categories.TryGetValue(category.Guid, out var existingCategory) && category.Name == existingCategory.Name)
                continue;

            updatingCategories.Add(category);
        }

        return new(updatingTracks, updatingPlaylists, updatingCategories);
    }

    private static async Task<UpdatingMedia> GetUpdatingPlaylistsMediaAsync(TracksScreenshot tracksScreenshot, Dictionary<string, Playlist> playlists,
        CategoriesScreenshot categoriesScreenshot, ExistingMedia existingMedia)
    {
        var playlistsTracks = playlists.Values.SelectMany(p => p.Tracks.Select(t => t.Guid)).ToHashSet();

        var updatingTracks = new List<Track>(tracksScreenshot.Tracks.Count);
        var updatingPlaylists = new List<Playlist>(playlists.Count);
        var updatingCategories = new List<Category>(categoriesScreenshot.Categories.Count);

        foreach (var track in tracksScreenshot.Tracks)
        {
            if (!playlistsTracks.Contains(track.Guid))
                continue;

            if (existingMedia.Tracks.TryGetValue(track.Guid, out var existingTrack) && track.Updated == existingTrack.Updated)
                continue;

            updatingTracks.Add(track);
        }

        foreach (var playlist in playlists.Values)
        {
            if (existingMedia.Playlists.TryGetValue(playlist.Guid, out var existingPlaylist) && playlist.Updated == existingPlaylist.Updated)
                continue;

            updatingPlaylists.Add(playlist);
        }

        foreach (var category in categoriesScreenshot.Categories)
        {
            if (existingMedia.Categories.TryGetValue(category.Guid, out var existingCategory) && category.Name == existingCategory.Name)
                continue;

            updatingCategories.Add(category);
        }

        return new(updatingTracks, updatingPlaylists, updatingCategories);
    }

    private static Task UploadTrackFileAsync(Track track, IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        byte[] content = File.ReadAllBytes(track.Path);
        var data = new UploadTrackFileData() { Guid = track.Guid, Path = track.Path, Content = content };
        return subscription.WriteAsync(data, token);
    }

    private async Task UploadTracksAsync(IReadOnlyCollection<Track> tracks,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        var tracksDataTable = new DataTable() { RowCapacity = tracks.Count, RowCount = tracks.Count };
        tracksDataTable.AddGuidColumn(nameof(Tracks.Guid)).Values.AddRange(tracks.Select(t => t.Guid));
        tracksDataTable.AddStringColumn(nameof(Tracks.Path)).Values.AddRange(tracks.Select(t => t.Path));
        tracksDataTable.AddStringColumn(nameof(Tracks.Title)).Values.AddRange(tracks.Select(t => t.Title));
        tracksDataTable.AddNullableInt32Column(nameof(Tracks.Year)).Values.AddRange(tracks.Select(t => t.Year));
        tracksDataTable.AddTimeSpanColumn(nameof(Tracks.Duration)).Values.AddRange(tracks.Select(t => t.Duration));
        tracksDataTable.AddByteColumn(nameof(Tracks.Rating)).Values.AddRange(tracks.Select(t => t.Rating));
        tracksDataTable.AddStringColumn(nameof(Tracks.Artist)).Values.AddRange(tracks.Select(t => t.Artist));
        tracksDataTable.AddStringColumn(nameof(Tracks.Artists)).Values.AddRange(tracks.Select(t => t.Artists));
        tracksDataTable.AddStringColumn(nameof(Tracks.Genre)).Values.AddRange(tracks.Select(t => t.Genre));
        tracksDataTable.AddStringColumn(nameof(Tracks.Album)).Values.AddRange(tracks.Select(t => t.Album));
        tracksDataTable.AddDateTimeColumn(nameof(Tracks.Created)).Values.AddRange(tracks.Select(t => t.Created));
        tracksDataTable.AddDateTimeColumn(nameof(Tracks.Added)).Values.AddRange(tracks.Select(t => t.Added));
        tracksDataTable.AddDateTimeColumn(nameof(Tracks.Updated)).Values.AddRange(tracks.Select(t => t.Updated));
        tracksDataTable.AddInt32Column(nameof(Tracks.Bitrate)).Values.AddRange(tracks.Select(t => t.Bitrate));
        tracksDataTable.AddInt64Column(nameof(Tracks.Size)).Values.AddRange(tracks.Select(t => t.Size));
        tracksDataTable.AddDateTimeColumn(nameof(Tracks.LastWrite)).Values.AddRange(tracks.Select(t => t.LastWrite));
        tracksDataTable.AddBooleanColumn(nameof(Tracks.HasPicture)).Values.AddRange(tracks.Select(t => t.HasPicture));
        var mimeTypeColumn = tracksDataTable.AddStringColumn(nameof(TrackPictures.MimeType));
        var pictureColumn = tracksDataTable.AddByteArrayColumn(nameof(TrackPictures.Data));
        int row = 0;
        var trackPictures = await Player.LoadPicturesAsync(tracks, token);
        var trackPicturesMap = trackPictures.ToDictionary(t => t.Id);

        foreach (var track in tracks)
        {
            if (track.HasPicture && trackPicturesMap.TryGetValue(track.Id, out var trackPicture))
            {
                mimeTypeColumn.Set(row, trackPicture.MimeType);
                pictureColumn.Set(row, trackPicture.Data);
            }

            row++;
        }

        var data = new UploadTracksData() { Tracks = tracksDataTable };
        await subscription.WriteAsync(data, token);
    }

    private async Task UploadTracksWithFilesAsync(IReadOnlyCollection<Track> tracks,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        foreach (var track in tracks)
            await UploadTrackFileAsync(track, subscription, token);

        await UploadTracksAsync(tracks, subscription, token);
    }

    private static async Task UploadTrackReproducedsAsync(IReadOnlyCollection<Track> tracks,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        // %%TODO




        ///// <summary>Треки</summary>
        //[DataMember(Order = 1)]
        //public DataTable TrackReproduceds { get; set; }
        ////{
        ////    [Column(ColumnType.Integer), PrimaryKey(nameof(AppId), nameof(TrackId))] AppId,
        ////    [Column(ColumnType.Integer)] TrackId,
        ////    [Column(ColumnType.Integer)] Reproduced,
        ////}

        await Task.Delay(1, token); // %%TODO 
    }

    private static async Task DeleteTracksAsync(IReadOnlyCollection<Track> tracks,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        var guids = new Guid[tracks.Count];
        int i = 0;

        foreach (var track in tracks)
            guids[i++] = track.Guid;

        var data = new DeleteTracksData() { Tracks = guids };
        await subscription.WriteAsync(data, token);
    }

    private static async Task UploadPlaylistsAsync(IReadOnlyCollection<Playlist> playlists,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (playlists is null || playlists.Count == 0)
            return;

        var playlistsDataTable = new DataTable() { RowCapacity = playlists.Count, RowCount = playlists.Count };
        playlistsDataTable.AddGuidColumn(nameof(Playlists.Guid)).Values.AddRange(playlists.Select(t => t.Guid));
        playlistsDataTable.AddStringColumn(nameof(Playlists.Name)).Values.AddRange(playlists.Select(t => t.Name));
        playlistsDataTable.AddDateTimeColumn(nameof(Playlists.Created)).Values.AddRange(playlists.Select(t => t.Created));
        playlistsDataTable.AddDateTimeColumn(nameof(Playlists.Updated)).Values.AddRange(playlists.Select(t => t.Updated));

        var playlistTracksCount = playlists.Sum(p => p.Tracks.Count);
        var playlistTracksDataTable = new DataTable() { RowCapacity = playlistTracksCount, RowCount = playlistTracksCount };
        var playlistColumn = playlistsDataTable.AddGuidColumn(nameof(PlaylistTracks.PlaylistId));
        var trackColumn = playlistsDataTable.AddGuidColumn(nameof(PlaylistTracks.TrackId));
        int row = 0;

        foreach (var playlist in playlists)
        {
            foreach (var track in playlist.Tracks)
            {
                playlistColumn.Set(row, playlist.Guid);
                trackColumn.Set(row, track.Guid);

                row++;
            }
        }

        var data = new UploadPlaylistsData() { Playlists = playlistsDataTable, PlaylistTracks = playlistTracksDataTable };
        await subscription.WriteAsync(data, token);
    }

    private static async Task DeletePlaylistsAsync(IReadOnlyCollection<Playlist> playlists,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (playlists is null || playlists.Count == 0)
            return;

        var guids = new Guid[playlists.Count];
        int i = 0;

        foreach (var playlist in playlists)
            guids[i++] = playlist.Guid;

        var data = new DeletePlaylistsData() { Playlists = guids };
        await subscription.WriteAsync(data, token);
    }

    private static async Task UploadCategoriesAsync(IReadOnlyCollection<Category> categories,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (categories is null || categories.Count == 0)
            return;

        var categoriesDataTable = new DataTable() { RowCapacity = categories.Count, RowCount = categories.Count };
        categoriesDataTable.AddGuidColumn(nameof(Categories.Guid)).Values.AddRange(categories.Select(t => t.Guid));
        categoriesDataTable.AddStringColumn(nameof(Categories.Name)).Values.AddRange(categories.Select(t => t.Name));

        var data = new UploadCategoriesData() { Categories = categoriesDataTable };
        await subscription.WriteAsync(data, token);
    }

    private static async Task DeleteCategoriesAsync(IReadOnlyCollection<Category> categories,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (categories is null || categories.Count == 0)
            return;

        var guids = new Guid[categories.Count];
        int i = 0;

        foreach (var category in categories)
            guids[i++] = category.Guid;

        var data = new DeleteCategoriesData() { Categories = guids };
        await subscription.WriteAsync(data, token);
    }

    public async ValueTask DisconnectAsync()
    {
        var subscription = _subscription;
        IsInitialized = false;

        if (subscription is null)
            return;

        await subscription.WriteAsync(new DisconnectTicket());
        _subscription = null;

        try
        {
            _waitEvent?.SetResult(true);
        }
        catch { }
    }

    private record ExistingMedia(Dictionary<Guid, TrackElement> Tracks, Dictionary<Guid, PlaylistElement> Playlists, Dictionary<Guid, CategoryElement> Categories);

    private record UpdatingMedia(List<Track> Tracks, List<Playlist> Playlists, List<Category> Categories);
}
