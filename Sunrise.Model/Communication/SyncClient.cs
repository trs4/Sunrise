using System.Net;
using Grpc.Core;
using IcyRain.Grpc.Client;
using IcyRain.Tables;
using Sunrise.Model.Common;
using Sunrise.Model.Communication.Data;
using Sunrise.Model.Model;
using Sunrise.Model.Schemes;

namespace Sunrise.Model.Communication;

public sealed class SyncClient : SyncService.Client, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly string _name;
    private readonly Player _player;
    private readonly Func<CancellationToken, Task>? _reloadCallback;
    private readonly Func<Exception, Task>? _onExceptionCallback;
    private CancellationTokenSource? _subscriptionCTS;

    private SyncClient(GrpcChannel channel, string name, Player player,
        Func<CancellationToken, Task>? reloadCallback, Func<Exception, Task>? onExceptionCallback)
        : base(channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _reloadCallback = reloadCallback;
        _onExceptionCallback = onExceptionCallback;
    }

    public static SyncClient Create(string name, Player player, IPAddress ipAddress, int port,
        Func<CancellationToken, Task>? reloadCallback = null, Func<Exception, Task>? onExceptionCallback = null)
    {
        var channel = GrpcChannel.ForAddress(ipAddress, port, setOptions: options =>
        {
            options.HttpVersion = new Version(2, 0);
            options.MaxReceiveMessageSize = null;
        });

        return new SyncClient(channel, name, player, reloadCallback, onExceptionCallback);
    }

    public void Connect()
    {
        if (_subscriptionCTS is not null)
            throw new InvalidOperationException(nameof(Connect));

        var parameters = new ConnectParameters() { Name = _name, IPAddress = Network.GetMachineIPAddress().GetAddressBytes() };
        var cts = _subscriptionCTS = new CancellationTokenSource();
        var call = Subscription(parameters);
        _ = SubscriptionWait(call, cts.Token);
    }

    private async Task SubscriptionWait(AsyncServerStreamingCall<SubscriptionTicket> call, CancellationToken token)
    {
        try
        {
            while (true)
            {
                if (await call.ResponseStream.MoveNext(token))
                {
                    var ticket = call.ResponseStream.Current;
                    await ProcessTicket(ticket, token);
                }
            }
        }
        catch (Exception e)
        {
            if (_onExceptionCallback is not null)
                await _onExceptionCallback(e);
        }
        finally
        {
            await call.ResponseHeadersAsync;
            call.Dispose();
        }
    }

    private async Task ProcessTicket(SubscriptionTicket ticket, CancellationToken token)
    {
        if (ticket is DisconnectTicket)
            Disconnect();
        else if (ticket is MediaLibraryTicket)
            await TransferMediaLibraryAsync(token);
        else if (ticket is MediaFilesTicket)
            await TransferMediaFilesAsync(token);
        else if (ticket is UploadTrackFileData uploadTrackFileData)
            UploadTrackFile(uploadTrackFileData);
        else if (ticket is UploadTracksData uploadTracksData)
            await UploadTracksAsync(uploadTracksData, token);
        else if (ticket is UploadTrackReproducedsData uploadTrackReproducedsData)
            UploadTrackReproduceds(uploadTrackReproducedsData);
        else if (ticket is DeleteTracksData deleteTracksData)
            await DeleteTracksAsync(deleteTracksData, token);
        else if (ticket is UploadPlaylistsData uploadPlaylistsData)
            await UploadPlaylistsAsync(uploadPlaylistsData, token);
        else if (ticket is DeletePlaylistsData deletePlaylistsData)
            await DeletePlaylistsAsync(deletePlaylistsData, token);
        else if (ticket is UploadCategoriesData uploadCategoriesData)
            await UploadCategoriesAsync(uploadCategoriesData, token);
        else if (ticket is DeleteCategoriesData deleteCategoriesData)
            await DeleteCategoriesAsync(deleteCategoriesData, token);
        else if (ticket is DeleteData)
            await DeleteDataAsync(token);
    }

    private async Task TransferMediaLibraryAsync(CancellationToken token)
    {
        var stream = new MemoryStream();
        await MediaExporter.ExportAsync(_player, stream, token);
        stream.Seek(0, SeekOrigin.Begin);

        var data = new MediaLibraryData() { Data = stream.ToArray() };
        await TransferMediaLibrary(data);
    }

    private async Task TransferMediaFilesAsync(CancellationToken token)
    {
        var folders = await _player.GetFoldersAsync(token);
        folders.Add(_player.TracksPath);

        var filePaths = new List<string>();

        foreach (string folder in folders)
        {
            var directoryInfo = new DirectoryInfo(folder);

            if (directoryInfo.Exists)
                filePaths.AddRange(directoryInfo.GetFiles("*", SearchOption.AllDirectories).Select(f => f.FullName));
        }

        var data = new MediaFilesData() { FilePaths = filePaths };
        await TransferMediaFiles(data);
    }

    private string GetFilePath(bool isDevice, string filePath)
    {
        filePath = Path.Combine(_player.TracksPath, filePath);

        if (isDevice)
            filePath = filePath.Replace("\\", "/");

        return filePath;
    }

    private void UploadTrackFile(UploadTrackFileData data)
    {
        bool isDevice = OperatingSystem.IsAndroid();
        string filePath = GetFilePath(isDevice, data.Path);
        string directoryPath = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(directoryPath);
        File.WriteAllBytes(filePath, data.Content);
    }

    private async Task UploadTracksAsync(UploadTracksData data, CancellationToken token)
    {
        var tracksDataTable = data.Tracks;
        var guidColumn = (GuidDataColumn)tracksDataTable[nameof(Tracks.Guid)];
        var pathColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Path)];
        var titleColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Title)];
        var yearColumn = (NullableInt32DataColumn)tracksDataTable[nameof(Tracks.Year)];
        var durationColumn = (TimeSpanDataColumn)tracksDataTable[nameof(Tracks.Duration)];
        var ratingColumn = (ByteDataColumn)tracksDataTable[nameof(Tracks.Rating)];
        var artistColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Artist)];
        var artistsColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Artists)];
        var genreColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Genre)];
        var albumColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Album)];
        var createdColumn = (DateTimeDataColumn)tracksDataTable[nameof(Tracks.Created)];
        var addedColumn = (DateTimeDataColumn)tracksDataTable[nameof(Tracks.Added)];
        var updatedColumn = (DateTimeDataColumn)tracksDataTable[nameof(Tracks.Updated)];
        var bitrateColumn = (Int32DataColumn)tracksDataTable[nameof(Tracks.Bitrate)];
        var sizeColumn = (Int64DataColumn)tracksDataTable[nameof(Tracks.Size)];
        var lastWriteColumn = (DateTimeDataColumn)tracksDataTable[nameof(Tracks.LastWrite)];
        var hasPictureColumn = (BooleanDataColumn)tracksDataTable[nameof(Tracks.HasPicture)];
        var mimeTypeColumn = (StringDataColumn)tracksDataTable[nameof(TrackPictures.MimeType)];
        var pictureColumn = (ByteArrayDataColumn)tracksDataTable[nameof(TrackPictures.Data)];
        var tracks = new List<Track>(tracksDataTable.RowCount);
        bool isDevice = OperatingSystem.IsAndroid();

        for (int row = 0; row < tracksDataTable.RowCount; row++)
        {
            var guid = guidColumn.Get(row);
            string path = pathColumn.Get(row);
            string filePath = GetFilePath(isDevice, path);
            bool hasPicture = hasPictureColumn.Get(row);

            var track = new Track()
            {
                Guid = guid,
                Path = filePath,
                Title = titleColumn.Get(row),
                Year = yearColumn.Get(row),
                Duration = durationColumn.Get(row),
                Rating = ratingColumn.Get(row),
                Artist = artistColumn.Get(row),
                Artists = artistsColumn.Get(row),
                Genre = genreColumn.Get(row),
                Album = albumColumn.Get(row),
                Created = createdColumn.Get(row),
                Added = addedColumn.Get(row),
                Updated = updatedColumn.Get(row),
                Bitrate = bitrateColumn.Get(row),
                Size = sizeColumn.Get(row),
                LastWrite = lastWriteColumn.Get(row),
                HasPicture = hasPicture,
            };

            if (hasPicture)
            {
                track.Picture = new TrackPicture()
                {
                    MimeType = mimeTypeColumn.Get(row),
                    Data = pictureColumn.Get(row),
                };
            }

            tracks.Add(track);
        }

        await _player.AddAsync(tracks, sync: true, token: token);

        if (_reloadCallback is not null)
            await _reloadCallback(token);
    }

    private static void UploadTrackReproduceds(UploadTrackReproducedsData data)
    {
        // %%TODO
    }

    private async Task DeleteTracksAsync(DeleteTracksData data, CancellationToken token)
    {
        await _player.DeleteTracksAsync(data.Tracks, token);

        if (_reloadCallback is not null)
            await _reloadCallback(token);
    }

    private async Task UploadPlaylistsAsync(UploadPlaylistsData data, CancellationToken token)
    {
        var playlistsDataTable = data.Playlists;
        var guidColumn = (GuidDataColumn)playlistsDataTable[nameof(Playlists.Guid)];
        var nameColumn = (StringDataColumn)playlistsDataTable[nameof(Playlists.Name)];
        var createdColumn = (DateTimeDataColumn)playlistsDataTable[nameof(Playlists.Created)];
        var updatedColumn = (DateTimeDataColumn)playlistsDataTable[nameof(Playlists.Updated)];
        var playlists = new List<Playlist>(playlistsDataTable.RowCount);
        var playlistsByGuid = new Dictionary<Guid, Playlist>(playlistsDataTable.RowCount);

        for (int row = 0; row < playlistsDataTable.RowCount; row++)
        {
            var guid = guidColumn.Get(row);

            var playlist = new Playlist()
            {
                Guid = guid,
                Name = nameColumn.Get(row),
                Created = createdColumn.Get(row),
                Updated = updatedColumn.Get(row),
                Tracks = [],
                Categories = [],
            };

            playlists.Add(playlist);
            playlistsByGuid[guid] = playlist;
        }

        var playlistTracksDataTable = data.PlaylistTracks;
        var playlistColumn = (GuidDataColumn)playlistTracksDataTable[nameof(PlaylistTracks.PlaylistId)];
        var trackColumn = (GuidDataColumn)playlistTracksDataTable[nameof(PlaylistTracks.TrackId)];
        var tracksScreenshot = await _player.GetTracksAsync(token);

        for (int row = 0; row < playlistsDataTable.RowCount; row++)
        {
            var playlistGuid = playlistColumn.Get(row);
            var trackGuid = trackColumn.Get(row);

            if (playlistsByGuid.TryGetValue(playlistGuid, out var playlist)
                && tracksScreenshot.TracksByGuid.TryGetValue(trackGuid, out var track))
            {
                playlist.Tracks.Add(track);
            }
        }

        await _player.AddAsync(playlists, sync: true, token: token);

        if (_reloadCallback is not null)
            await _reloadCallback(token);
    }

    private async Task DeletePlaylistsAsync(DeletePlaylistsData data, CancellationToken token)
    {
        await _player.DeletePlaylistsAsync(data.Playlists, token);

        if (_reloadCallback is not null)
            await _reloadCallback(token);
    }

    private async Task UploadCategoriesAsync(UploadCategoriesData data, CancellationToken token)
    {
        var categoriesDataTable = data.Categories;
        var guidColumn = (GuidDataColumn)categoriesDataTable[nameof(Categories.Guid)];
        var nameColumn = (StringDataColumn)categoriesDataTable[nameof(Categories.Name)];
        var categories = new List<Category>(categoriesDataTable.RowCount);

        for (int row = 0; row < categoriesDataTable.RowCount; row++)
        {
            var category = new Category()
            {
                Guid = guidColumn.Get(row),
                Name = nameColumn.Get(row),
            };

            categories.Add(category);
        }

        await _player.AddAsync(categories, sync: true, token: token);

        if (_reloadCallback is not null)
            await _reloadCallback(token);
    }

    private async Task DeleteCategoriesAsync(DeleteCategoriesData data, CancellationToken token)
    {
        await _player.DeleteCategoriesAsync(data.Categories, token);

        if (_reloadCallback is not null)
            await _reloadCallback(token);
    }

    private async Task DeleteDataAsync(CancellationToken token)
    {
        await _player.DeleteDataAsync(token);

        if (_reloadCallback is not null)
            await _reloadCallback(token);
    }

    public void Disconnect()
    {
        var subscriptionCTS = _subscriptionCTS;

        if (subscriptionCTS is not null)
        {
            subscriptionCTS.Cancel();
            subscriptionCTS.Dispose();
            _subscriptionCTS = null;
        }
    }

    public void Dispose()
    {
        Disconnect();
        _channel.Dispose();
        GC.SuppressFinalize(this);
    }

}
