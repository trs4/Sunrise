using System.Net;
using Grpc.Core;
using IcyRain.Grpc.Client;
using IcyRain.Tables;
using Sunrise.Model.Communication.Data;
using Sunrise.Model.Schemes;

namespace Sunrise.Model.Communication;

public sealed class SyncClient : SyncService.Client, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly string _name;
    private readonly Player _player;
    private readonly Func<CancellationToken, Task>? _reloadCallback;
    private CancellationTokenSource? _subscriptionCTS;

    private SyncClient(GrpcChannel channel, string name, Player player, Func<CancellationToken, Task>? reloadCallback)
        : base(channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
        _player = player ?? throw new ArgumentNullException(nameof(player));
        _reloadCallback = reloadCallback;
    }

    public static SyncClient Create(string name, Player player, IPAddress ipAddress, int port,
        Func<CancellationToken, Task>? reloadCallback = null)
    {
        var channel = GrpcChannel.ForAddress(ipAddress, port, setOptions: options =>
        {
            options.HttpVersion = new Version(2, 0);
            options.MaxReceiveMessageSize = null;
        });

        return new SyncClient(channel, name, player, reloadCallback);
    }

    public void Connect()
    {
        if (_subscriptionCTS is not null)
            throw new InvalidOperationException(nameof(Connect));

        var parameters = new ConnectParameters() { Name = _name };
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
                if (await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
                {
                    var ticket = call.ResponseStream.Current;
                    await ProcessTicket(ticket, token);
                }
            }
        }
        catch { }
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
        else if (ticket is MediaLibraryTicket mediaLibraryTicket)
            await TransferMediaLibraryAsync(mediaLibraryTicket);
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
    }

    private async Task TransferMediaLibraryAsync(MediaLibraryTicket ticket)
    {
        // %%TODO
        var tracksDataTable = new DataTable(); // { RowCapacity = tracks.Count, RowCount = tracks.Count };
                                               //tracksDataTable.AddGuidColumn(nameof(Tracks.Guid)).Values.AddRange(tracks.Select(t => t.Guid));
                                               //tracksDataTable.AddStringColumn(nameof(Tracks.Title)).Values.AddRange(tracks.Select(t => t.Title));
                                               //tracksDataTable.AddNullableInt32Column(nameof(Tracks.Year)).Values.AddRange(tracks.Select(t => t.Year));
                                               //tracksDataTable.AddTimeSpanColumn(nameof(Tracks.Duration)).Values.AddRange(tracks.Select(t => t.Duration));
                                               //tracksDataTable.AddByteColumn(nameof(Tracks.Rating)).Values.AddRange(tracks.Select(t => t.Rating));
                                               //tracksDataTable.AddStringColumn(nameof(Tracks.Artist)).Values.AddRange(tracks.Select(t => t.Artist));
                                               //tracksDataTable.AddStringColumn(nameof(Tracks.Genre)).Values.AddRange(tracks.Select(t => t.Genre));
                                               //tracksDataTable.AddStringColumn(nameof(Tracks.Album)).Values.AddRange(tracks.Select(t => t.Album));
                                               //tracksDataTable.AddDateTimeColumn(nameof(Tracks.Created)).Values.AddRange(tracks.Select(t => t.Created));
                                               //tracksDataTable.AddDateTimeColumn(nameof(Tracks.Added)).Values.AddRange(tracks.Select(t => t.Added));
                                               //tracksDataTable.AddInt32Column(nameof(Tracks.Bitrate)).Values.AddRange(tracks.Select(t => t.Bitrate));
                                               //tracksDataTable.AddInt64Column(nameof(Tracks.Size)).Values.AddRange(tracks.Select(t => t.Size));
                                               //tracksDataTable.AddDateTimeColumn(nameof(Tracks.LastWrite)).Values.AddRange(tracks.Select(t => t.LastWrite));
                                               //tracksDataTable.AddBooleanColumn(nameof(Tracks.HasPicture)).Values.AddRange(tracks.Select(t => t.HasPicture));
                                               //var mimeTypeColumn = tracksDataTable.AddStringColumn(nameof(TrackPictures.MimeType));
                                               //var pictureColumn = tracksDataTable.AddByteArrayColumn(nameof(TrackPictures.Data));
                                               //int row = 0;
                                               //var trackPictures = await Player.LoadPicturesAsync(tracks, token);
                                               //var trackPicturesMap = trackPictures.ToDictionary(t => t.Id);

        //foreach (var track in tracks)
        //{
        //    if (track.HasPicture && trackPicturesMap.TryGetValue(track.Id, out var trackPicture))
        //    {
        //        mimeTypeColumn.Set(row, trackPicture.MimeType);
        //        pictureColumn.Set(row, trackPicture.Data);
        //    }

        //    row++;
        //}

        var playlistsDataTable = new DataTable(); // { RowCapacity = playlists.Count, RowCount = playlists.Count };
        //playlistsDataTable.AddGuidColumn(nameof(Playlists.Guid)).Values.AddRange(playlists.Select(t => t.Guid));
        //playlistsDataTable.AddStringColumn(nameof(Playlists.Name)).Values.AddRange(playlists.Select(t => t.Name));
        //playlistsDataTable.AddDateTimeColumn(nameof(Playlists.Created)).Values.AddRange(playlists.Select(t => t.Created));

        //var playlistTracksCount = playlists.Sum(p => p.Tracks.Count);
        var playlistTracksDataTable = new DataTable(); // { RowCapacity = playlistTracksCount, RowCount = playlistTracksCount };
                                                       //var playlistColumn = playlistsDataTable.AddGuidColumn(nameof(PlaylistTracks.PlaylistId));
                                                       //var trackColumn = playlistsDataTable.AddGuidColumn(nameof(PlaylistTracks.TrackId));
                                                       //int row = 0;

        //foreach (var playlist in playlists)
        //{
        //    foreach (var track in playlist.Tracks)
        //    {
        //        playlistColumn.Set(row, playlist.Guid);
        //        trackColumn.Set(row, track.Guid);

        //        row++;
        //    }
        //}


        var categoriesDataTable = new DataTable(); // { RowCapacity = categories.Count, RowCount = categories.Count };
        //categoriesDataTable.AddGuidColumn(nameof(Categories.Guid)).Values.AddRange(categories.Select(t => t.Guid));
        //categoriesDataTable.AddStringColumn(nameof(Categories.Name)).Values.AddRange(categories.Select(t => t.Name));







        var data = new MediaLibraryData()
        {
            Tracks = tracksDataTable,
            Playlists = playlistsDataTable,
            PlaylistTracks = playlistTracksDataTable,
            Categories = categoriesDataTable,
        };

        await TransferMediaLibrary(data);
    }

    private void UploadTrackFile(UploadTrackFileData data)
    {
        string filePath = Path.Combine(_player.TracksPath, data.Guid.ToString());
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
        var genreColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Genre)];
        var albumColumn = (StringDataColumn)tracksDataTable[nameof(Tracks.Album)];
        var createdColumn = (DateTimeDataColumn)tracksDataTable[nameof(Tracks.Created)];
        var addedColumn = (DateTimeDataColumn)tracksDataTable[nameof(Tracks.Added)];
        var bitrateColumn = (Int32DataColumn)tracksDataTable[nameof(Tracks.Bitrate)];
        var sizeColumn = (Int64DataColumn)tracksDataTable[nameof(Tracks.Size)];
        var lastWriteColumn = (DateTimeDataColumn)tracksDataTable[nameof(Tracks.LastWrite)];
        var hasPictureColumn = (BooleanDataColumn)tracksDataTable[nameof(Tracks.HasPicture)];
        var mimeTypeColumn = (StringDataColumn)tracksDataTable[nameof(TrackPictures.MimeType)];
        var pictureColumn = (ByteArrayDataColumn)tracksDataTable[nameof(TrackPictures.Data)];
        var tracks = new List<Track>(tracksDataTable.RowCount);

        for (int row = 0; row < tracksDataTable.RowCount; row++)
        {
            var guid = guidColumn.Get(row);
            string path = pathColumn.Get(row);
            string extension = Path.GetExtension(path);
            string filePath = Path.Combine(_player.TracksPath, guid.ToString(), extension);
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
                Genre = genreColumn.Get(row),
                Album = albumColumn.Get(row),
                Created = createdColumn.Get(row),
                Added = addedColumn.Get(row),
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

    private Task DeleteTracksAsync(DeleteTracksData data, CancellationToken token)
        => _player.DeleteTracksAsync(data.Tracks, token);

    private async Task UploadPlaylistsAsync(UploadPlaylistsData data, CancellationToken token)
    {
        var playlistsDataTable = data.Playlists;
        var guidColumn = (GuidDataColumn)playlistsDataTable[nameof(Playlists.Guid)];
        var nameColumn = (StringDataColumn)playlistsDataTable[nameof(Playlists.Name)];
        var createdColumn = (DateTimeDataColumn)playlistsDataTable[nameof(Playlists.Created)];
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

    private Task DeletePlaylistsAsync(DeletePlaylistsData data, CancellationToken token)
        => _player.DeletePlaylistsAsync(data.Playlists, token);

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

    private Task DeleteCategoriesAsync(DeleteCategoriesData data, CancellationToken token)
        => _player.DeleteCategoriesAsync(data.Categories, token);

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
