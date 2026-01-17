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
    private const int _packetSize = 128;
    private readonly Action<DiscoveryDeviceInfo>? _onDeviceDetected;
    private IServerStreamWriter<SubscriptionTicket>? _subscription;
    private TaskCompletionSource<bool>? _waitEvent;
    private bool _isSynchronizing;
    private MediaLibraryData? _mediaLibraryData;
    private MediaFilesData? _mediaFilesData;

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
        => SynchronizeCoreAsync(GetUpdatingMedia, token);

    public Task SynchronizePlaylistsAsync(CancellationToken token = default)
        => SynchronizeCoreAsync(GetUpdatingPlaylistsMedia, token);

    private async Task SynchronizeCoreAsync(Func<TracksScreenshot, Dictionary<string, Playlist>, CategoriesScreenshot, List<string>,
        ExistingMedia, UpdatingMedia> getUpdatingMedia, CancellationToken token)
    {
        var subscription = _subscription;

        if (_isSynchronizing || subscription is null)
            return;

        _isSynchronizing = true;

        try
        {
            var existingMedia = await GetExistingMediaAsync(subscription, token);

            var folders = await Player.GetFoldersAsync(token);
            var tracksScreenshot = await Player.GetTracksAsync(token);
            var playlists = await Player.GetPlaylistsAsync(token);
            var categoriesScreenshot = await Player.GetCategoriesAsync(token);

            var updatingMedia = getUpdatingMedia(tracksScreenshot, playlists, categoriesScreenshot, folders, existingMedia);
            await UploadAsync(updatingMedia, existingMedia.TrackPaths, existingMedia.FilePaths, subscription, token);
        }
        finally
        {
            _isSynchronizing = false;
        }
    }

    public async Task ClearAsync(CancellationToken token = default)
    {
        var subscription = _subscription;

        if (_isSynchronizing || subscription is null)
            return;

        await subscription.WriteAsync(new DeleteData(), token);
    }

    private async Task<byte[]> LoadMediaLibraryAsync(IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        await subscription.WriteAsync(new MediaLibraryTicket(), token);

        while (true)
        {
            await Task.Delay(25, token).ConfigureAwait(false);
            var mediaLibraryData = _mediaLibraryData;

            if (mediaLibraryData is null)
                continue;

            _mediaLibraryData = null;
            return mediaLibraryData.Data;
        }
    }

    public void SetMediaLibrary(MediaLibraryData data) => _mediaLibraryData = data;

    private async Task<List<string>> LoadMediaFilesAsync(IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        await subscription.WriteAsync(new MediaFilesTicket(), token);

        while (true)
        {
            await Task.Delay(25, token).ConfigureAwait(false);
            var mediaFilesData = _mediaFilesData;

            if (mediaFilesData is null)
                continue;

            _mediaFilesData = null;
            return mediaFilesData.FilePaths;
        }
    }

    public void SetMediaFiles(MediaFilesData data) => _mediaFilesData = data;

    private async Task<ExistingMedia> GetExistingMediaAsync(IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        var bytes = await LoadMediaLibraryAsync(subscription, token);
        var mediaFilePaths = await LoadMediaFilesAsync(subscription, token);
        var stream = new MemoryStream(bytes);
        var document = MediaExporter.Deserialize(stream) ?? throw new InvalidOperationException(nameof(MediaExporter));

        var existingFolders = document.Folders?.ToList() ?? [];
        var existingTracks = new Dictionary<Guid, TrackElement>(document.Tracks?.Count ?? 0);
        var existingPlaylists = new Dictionary<Guid, PlaylistElement>(document.Playlists?.Count ?? 0);
        var existingCategories = new Dictionary<Guid, CategoryElement>(document.Categories?.Count ?? 0);
        var existingTrackPaths = new Dictionary<Guid, string>(document.Tracks?.Count ?? 0);

        foreach (var track in document.Tracks ?? [])
        {
            existingTracks[track.Guid] = track;
            existingTrackPaths[track.Guid] = GetTrackRelativePath(track.Path, existingFolders);
        }

        foreach (var playlist in document.Playlists ?? [])
            existingPlaylists[playlist.Guid] = playlist;

        foreach (var category in document.Categories ?? [])
            existingCategories[category.Guid] = category;

        var existingFilePaths = new HashSet<string>(mediaFilePaths.Count, StringComparer.OrdinalIgnoreCase);

        if (existingFolders.Count > 0)
        {
            foreach (string filePath in mediaFilePaths)
            {
                foreach (string folder in existingFolders)
                {
                    int index = filePath.IndexOf(folder, StringComparison.OrdinalIgnoreCase);

                    if (index > -1)
                    {
                        existingFilePaths.Add(filePath.Substring(index + folder.Length + 1).Replace("/", "\\"));
                        break;
                    }
                }
            }
        }

        return new(existingTracks, existingPlaylists, existingCategories, existingTrackPaths, existingFilePaths);
    }

    private static UpdatingMedia GetUpdatingMedia(TracksScreenshot tracksScreenshot, Dictionary<string, Playlist> playlists,
        CategoriesScreenshot categoriesScreenshot, List<string> folders, ExistingMedia existingMedia)
        => GetUpdatingMediaCore(tracksScreenshot, playlists, categoriesScreenshot, folders, existingMedia);

    private static UpdatingMedia GetUpdatingPlaylistsMedia(TracksScreenshot tracksScreenshot, Dictionary<string, Playlist> playlists,
        CategoriesScreenshot categoriesScreenshot, List<string> folders, ExistingMedia existingMedia)
    {
        var playlistsTracks = playlists.Values.SelectMany(p => p.Tracks.Select(t => t.Guid)).ToHashSet();

        return GetUpdatingMediaCore(tracksScreenshot, playlists, categoriesScreenshot, folders, existingMedia,
            track => !playlistsTracks.Contains(track.Guid));
    }

    private static UpdatingMedia GetUpdatingMediaCore(TracksScreenshot tracksScreenshot, Dictionary<string, Playlist> playlists,
        CategoriesScreenshot categoriesScreenshot, List<string> folders, ExistingMedia existingMedia, Predicate<Track>? trackCondition = null)
    {
        var updatingTracks = new List<List<Track>>(tracksScreenshot.Tracks.Count / _packetSize);
        var updatingTracksPacket = new List<Track>(_packetSize);

        var updatingPlaylists = new List<List<Playlist>>(playlists.Count / _packetSize);
        var updatingPlaylistsPacket = new List<Playlist>(_packetSize);

        var updatingCategories = new List<List<Category>>(categoriesScreenshot.Categories.Count / _packetSize);
        var updatingCategoriesPacket = new List<Category>(_packetSize);

        var trackPaths = new Dictionary<int, string>(tracksScreenshot.Tracks.Count);

        foreach (var track in tracksScreenshot.Tracks)
        {
            if (trackCondition?.Invoke(track) ?? false)
                continue;

            if (existingMedia.Tracks.TryGetValue(track.Guid, out var existingTrack) && track.Updated == existingTrack.Updated)
                continue;

            updatingTracksPacket.Add(track);
            AddTrackRelativePath(track, trackPaths, folders);

            if (updatingTracksPacket.Count == _packetSize)
            {
                updatingTracks.Add(updatingTracksPacket);
                updatingTracksPacket = new List<Track>(_packetSize);
            }
        }

        foreach (var playlist in playlists.Values)
        {
            if (existingMedia.Playlists.TryGetValue(playlist.Guid, out var existingPlaylist) && playlist.Updated == existingPlaylist.Updated)
                continue;

            updatingPlaylistsPacket.Add(playlist);

            if (updatingPlaylistsPacket.Count == _packetSize)
            {
                updatingPlaylists.Add(updatingPlaylistsPacket);
                updatingPlaylistsPacket = new List<Playlist>(_packetSize);
            }
        }

        foreach (var category in categoriesScreenshot.Categories)
        {
            if (existingMedia.Categories.TryGetValue(category.Guid, out var existingCategory) && category.Name == existingCategory.Name)
                continue;

            updatingCategoriesPacket.Add(category);

            if (updatingCategoriesPacket.Count == _packetSize)
            {
                updatingCategories.Add(updatingCategoriesPacket);
                updatingCategoriesPacket = new List<Category>(_packetSize);
            }
        }

        if (updatingTracksPacket.Count > 0)
            updatingTracks.Add(updatingTracksPacket);

        if (updatingPlaylistsPacket.Count > 0)
            updatingPlaylists.Add(updatingPlaylistsPacket);

        if (updatingCategoriesPacket.Count > 0)
            updatingCategories.Add(updatingCategoriesPacket);

        return new(updatingTracks, updatingPlaylists, updatingCategories, trackPaths);
    }

    private static void AddTrackRelativePath(Track track, Dictionary<int, string> trackPaths, List<string> folders)
        => trackPaths.Add(track.Id, GetTrackRelativePath(track.Path, folders));

    private static string GetTrackRelativePath(string filePath, List<string> folders)
    {
        int index;

        foreach (string folder in folders)
        {
            index = filePath.IndexOf(folder, StringComparison.OrdinalIgnoreCase);

            if (index > -1)
                return filePath.Substring(index + folder.Length);
        }

        const string disk = ":\\";
        index = filePath.IndexOf(disk, StringComparison.OrdinalIgnoreCase);
        return filePath.Substring(index + disk.Length);
    }

    private async Task UploadAsync(UpdatingMedia updatingMedia,
        Dictionary<Guid, string> existingTrackPaths, HashSet<string> existingFilePaths,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        foreach (var tracks in updatingMedia.Tracks)
            await UploadTracksWithFilesAsync(tracks, updatingMedia.TrackPaths, existingTrackPaths, existingFilePaths, subscription, token);

        foreach (var playlists in updatingMedia.Playlists)
            await UploadPlaylistsAsync(playlists, subscription, token);

        foreach (var categories in updatingMedia.Categories)
            await UploadCategoriesAsync(categories, subscription, token);
    }

    private static Task UploadTrackFileAsync(Track track, Dictionary<int, string> trackPaths,
        Dictionary<Guid, string> existingTrackPaths, HashSet<string> existingFilePaths,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        string relativePath = trackPaths[track.Id];

        if (existingFilePaths.Contains(relativePath) || (existingTrackPaths.TryGetValue(track.Guid, out string existingRelativePath)
            && relativePath.Equals(existingRelativePath, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.CompletedTask;
        }

        byte[] content = File.ReadAllBytes(track.Path);
        var data = new UploadTrackFileData() { Guid = track.Guid, Path = relativePath, Content = content };
        return subscription.WriteAsync(data, token);
    }

    private async Task UploadTracksAsync(IReadOnlyCollection<Track> tracks, Dictionary<int, string> trackPaths,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        var tracksDataTable = new DataTable() { RowCapacity = tracks.Count, RowCount = tracks.Count };
        tracksDataTable.AddGuidColumn(nameof(Tracks.Guid)).Values.AddRange(tracks.Select(t => t.Guid));
        tracksDataTable.AddStringColumn(nameof(Tracks.Path)).Values.AddRange(tracks.Select(t => trackPaths[t.Id]));
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
        tracksDataTable.AddStringColumn(nameof(Tracks.OriginalText)).Values.AddRange(tracks.Select(t => t.OriginalText));
        tracksDataTable.AddStringColumn(nameof(Tracks.TranslateText)).Values.AddRange(tracks.Select(t => t.TranslateText));
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

    private async Task UploadTracksWithFilesAsync(IReadOnlyCollection<Track> tracks, Dictionary<int, string> trackPaths,
        Dictionary<Guid, string> existingTrackPaths, HashSet<string> existingFilePaths,
        IServerStreamWriter<SubscriptionTicket> subscription, CancellationToken token)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        foreach (var track in tracks)
            await UploadTrackFileAsync(track, trackPaths, existingTrackPaths, existingFilePaths, subscription, token);

        await UploadTracksAsync(tracks, trackPaths, subscription, token);
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
        var playlistColumn = playlistTracksDataTable.AddGuidColumn(nameof(PlaylistTracks.PlaylistId));
        var trackColumn = playlistTracksDataTable.AddGuidColumn(nameof(PlaylistTracks.TrackId));
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

    private record ExistingMedia(Dictionary<Guid, TrackElement> Tracks, Dictionary<Guid, PlaylistElement> Playlists, Dictionary<Guid, CategoryElement> Categories,
        Dictionary<Guid, string> TrackPaths, HashSet<string> FilePaths);

    private record UpdatingMedia(List<List<Track>> Tracks, List<List<Playlist>> Playlists, List<List<Category>> Categories,
        Dictionary<int, string> TrackPaths);
}
