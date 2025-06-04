using System.Text;
using IcyRain.Tables;
using RedLight;
using RedLight.SQLite;
using Sunrise.Model.Resources;
using Sunrise.Model.Schemes;

namespace Sunrise.Model;

public sealed class Player
{
    private static readonly HashSet<string> _insertExcludedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Tracks.Picked)
    };

    private static readonly HashSet<string> _updateExcludedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Tracks.Guid), nameof(Tracks.Path), nameof(Tracks.Picked), nameof(Tracks.Rating), nameof(Tracks.Reproduced),
        nameof(Tracks.Added), nameof(Tracks.RootFolder), nameof(Tracks.RelationFolder), nameof(Tracks.OriginalText),
        nameof(Tracks.TranslateText), nameof(Tracks.Language)
    };

    private readonly string _folderPath;
    private readonly DatabaseConnection _connection;

    private List<Track>? _allTracks;
    private Dictionary<int, Track> _allTracksById;
    private Dictionary<string, Track>? _allTracksByPath;
    private readonly object _allTracksSync = new();

    private Dictionary<string, Playlist>? _allPlaylistsByName;
    private readonly object _allPlaylistsSync = new();

    static Player()
        => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private Player(string folderPath, DatabaseConnection connection)
    {
        _folderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Media = new(this);
    }

    public MediaPlayer Media { get; }

    public static async Task<Player> InitAsync(CancellationToken token = default)
    {
        string rootFolder = OperatingSystem.IsAndroid()
            ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        string folderPath = Path.Combine(rootFolder, "Sunrise");
        Directory.CreateDirectory(folderPath);

        string databaseFilePath = Path.Combine(folderPath, "MediaLibrary.db");
        string connectionString = $@"Provider=SQLite;Data Source='{databaseFilePath}'";
        var connection = SQLiteDatabaseConnection.Create(connectionString);

        if (!File.Exists(databaseFilePath))
            await CreateDatabase(connection, token);

        return new Player(folderPath, connection);
    }

    private static async Task CreateDatabase(DatabaseConnection connection, CancellationToken token)
    {
        await connection.Schema.CreateTableWithParseQuery<Devices>().RunAsync(token);

        await connection.Insert.CreateQuery<Devices>() // Текущее устройство
            .AddColumn(Devices.Guid, Guid.NewGuid())
            .AddColumn(Devices.Name, Environment.MachineName)
            .AddColumn(Devices.IsMain, true)
            .RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<Folders>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<Tracks>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<TrackPictures>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<Playlists>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<PlaylistTracks>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<AppNames>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<TrackReproduceds>().RunAsync(token);
    }

    public bool IsAllTracksLoaded()
    {
        lock (_allTracksSync)
            return _allTracks is not null && _allTracksByPath is not null;
    }

    public async Task<TracksScreenshot> GetAllTracks(CancellationToken token = default)
    {
        List<Track> allTracks;
        Dictionary<int, Track> allTracksById;
        Dictionary<string, Track> allTracksByPath;

        lock (_allTracksSync)
        {
            allTracks = _allTracks;
            allTracksById = _allTracksById;
            allTracksByPath = _allTracksByPath;
        }

        if (allTracks is not null && allTracksById is not null && allTracksByPath is not null)
            return new(allTracks, allTracksById, allTracksByPath);

        allTracks = await _connection.Select.CreateWithParseQuery<Track, Tracks>().GetAsync(token);
        allTracksById = new(allTracks.Count);
        allTracksByPath = new(allTracks.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var track in allTracks)
        {
            allTracksById.Add(track.Id, track);
            allTracksByPath[track.Path] = track;
        }

        lock (_allTracksSync)
        {
            _allTracks = allTracks;
            _allTracksById = allTracksById;
            _allTracksByPath = allTracksByPath;
        }

        return new(allTracks, allTracksById, allTracksByPath);
    }

    public async Task<Dictionary<string, Playlist>> GetAllPlaylists(CancellationToken token = default)
    {
        Dictionary<string, Playlist>? allPlaylistsByName;

        lock (_allPlaylistsSync)
            allPlaylistsByName = _allPlaylistsByName;

        if (allPlaylistsByName is not null)
            return allPlaylistsByName;

        var screenshot = await GetAllTracks(token);
        var playlists = await _connection.Select.CreateWithParseQuery<Playlist, Playlists>().GetAsync(token);
        var allPlaylistsById = new Dictionary<int, Playlist>(playlists.Count);
        allPlaylistsByName = new(playlists.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var playlist in playlists)
        {
            playlist.Tracks = [];
            allPlaylistsById.Add(playlist.Id, playlist);
            allPlaylistsByName[playlist.Name] = playlist;
        }

        var playlistTracks = await _connection.Select.CreateWithParseQuery<PlaylistTracks>().OrderBy(PlaylistTracks.Position).GetAsync<DataTable>(token);
        var playlistIdColumn = playlistTracks[nameof(PlaylistTracks.PlaylistId)];
        var trackIdColumn = playlistTracks[nameof(PlaylistTracks.TrackId)];

        for (int row = 0; row < playlistTracks.RowCount; row++)
        {
            int playlistId = playlistIdColumn.GetInt(row);
            int trackId = trackIdColumn.GetInt(row);

            if (allPlaylistsById.TryGetValue(playlistId, out var playlist) && screenshot.AllTracksById.TryGetValue(trackId, out var track))
                playlist.Tracks.Add(track);
        }

        lock (_allPlaylistsSync)
            _allPlaylistsByName = allPlaylistsByName;

        return allPlaylistsByName;
    }

    public async Task<bool> DeletePlaylist(Playlist? playlist, CancellationToken token = default)
    {
        if (playlist is null)
            return false;

        lock (_allPlaylistsSync)
            _allPlaylistsByName?.Remove(playlist.Name);

        bool result = await _connection.Delete.CreateQuery<Playlists>()
            .WithTerm(Playlists.Id, playlist.Id)
            .RunAsync(token) > 0;

        if (!result)
            return false;

        await _connection.Delete.CreateQuery<PlaylistTracks>()
            .WithTerm(PlaylistTracks.PlaylistId, playlist.Id)
            .RunAsync(token);

        return true;
    }

    public void ClearAllTracks()
    {
        lock (_allTracksSync)
        {
            _allTracks = null;
            _allTracksByPath = null;
        }
    }

    public void ClearAllPlaylists()
    {
        lock (_allPlaylistsSync)
            _allPlaylistsByName = null;
    }

    public async ValueTask<bool> AddFolderAsync(string? folderPath, IProgress? progressOwner = null, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return false;

        if (folderPath[^1] == Path.PathSeparator)
            folderPath = folderPath.Substring(0, folderPath.Length - 1);

        var directoryInfo = new DirectoryInfo(folderPath);

        if (!directoryInfo.Exists)
            return false;

        await _connection.Insert.CreateQuery(Tables.Folders)
            .AddColumn(Folders.Path, folderPath)
            .RunAsync(token);

        var files = directoryInfo.GetFiles("*.mp3", SearchOption.AllDirectories);
        await AddAsync(files, progressOwner, token);
        return true;
    }

    public async ValueTask AddAsync(IReadOnlyCollection<FileInfo> files, IProgress? progressOwner = null, CancellationToken token = default)
    {
        if (files is null || files.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadTracks);
        var screenshot = await GetAllTracks(token);
        double progress = 0d;
        double step = 100d / files.Count;
        const int tracksInPacket = 1_000;
        var addTracks = new List<Track>(tracksInPacket);
        var updateTracks = new List<Track>(tracksInPacket);
        var removePictureIds = new List<int>(tracksInPacket);
        var pictures = new List<TrackPicture>(tracksInPacket);
        var now = DateTime.Now;

        foreach (var file in files)
        {
            if (progressOwner is not null)
            {
                progress += step;
                progressOwner.Next(progress, file.Name);
            }

            if (!TrackManager.TryCreate(file, now, out var track))
                continue;

            if (screenshot.AllTracksByPath.TryGetValue(file.FullName, out var existingTrack))
            {
                UpdateIdProperties(track, existingTrack);
                UpdatePostProperties(track, existingTrack);

                if (track.LastWrite == existingTrack.LastWrite)
                    continue;

                updateTracks.Add(track);

                if (existingTrack.HasPicture)
                    removePictureIds.Add(track.Id);
            }
            else
                addTracks.Add(track);

            if (addTracks.Count == tracksInPacket)
                await AddTracks(addTracks, pictures, token);

            if (updateTracks.Count == tracksInPacket)
                await UpdateTracks(updateTracks, pictures, removePictureIds, token);
        }

        if (addTracks.Count > 0)
            await AddTracks(addTracks, pictures, token);

        if (updateTracks.Count > 0)
            await UpdateTracks(updateTracks, pictures, removePictureIds, token);

        ClearAllTracks();
        progressOwner?.Hide();
    }

    public async ValueTask AddAsync(IReadOnlyCollection<Track> tracks, IProgress? progressOwner = null, string? withAppName = null,
        CancellationToken token = default)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadTracks);
        var screenshot = await GetAllTracks(token);
        double progress = 0d;
        double step = 100d / tracks.Count;
        const int tracksInPacket = 1_000;
        var addTracks = new List<Track>(tracksInPacket);
        var updateTracks = new List<Track>(tracksInPacket);
        var removePictureIds = new List<int>(tracksInPacket);
        var pictures = new List<TrackPicture>(tracksInPacket);
        var now = DateTime.Now;

        int? withAppNameId = null;
        var addReproduceds = new List<(Track, int)>(tracksInPacket);
        var updateReproduceds = new List<TrackReproduced>(tracksInPacket);
        var appNames = await _connection.Select.CreateWithParseQuery<AppName, AppNames>().GetAsync(token);
        var trackReproduceds = await _connection.Select.CreateWithParseQuery<TrackReproduced, TrackReproduceds>().GetAsync(token);
        var trackReproducedsMap = new Dictionary<int, Dictionary<int, int>>(screenshot.AllTracks.Count); // Track -> App -> Reproduced

        foreach (var trackReproduced in trackReproduceds)
        {
            if (!trackReproducedsMap.TryGetValue(trackReproduced.TrackId, out var appReproducedsMap))
                trackReproducedsMap.Add(trackReproduced.TrackId, appReproducedsMap = []);

            appReproducedsMap.Add(trackReproduced.AppId, trackReproduced.Reproduced);
        }

        if (!string.IsNullOrWhiteSpace(withAppName))
        {
            withAppNameId = appNames.FirstOrDefault(a => a.Name == withAppName)?.Id;

            if (!withAppNameId.HasValue)
            {
                var appName = new AppName() { Name = withAppName };
                await _connection.Insert.CreateWithParseQuery<AppName, AppNames>(appName).FillAsync(token);
                withAppNameId = appName.Id;
            }
        }

        foreach (var track in tracks)
        {
            if (track is null)
                continue;

            if (progressOwner is not null)
            {
                string fileName = Path.GetFileName(track.Path);
                progress += step;
                progressOwner.Next(progress, fileName);
            }

            if (screenshot.AllTracksByPath.TryGetValue(track.Path, out var existingTrack))
            {
                UpdateIdProperties(track, existingTrack);

                if (!NeedUpdateTrack(track, existingTrack, withAppName, withAppNameId, trackReproducedsMap))
                {
                    UpdatePostProperties(track, existingTrack);
                    continue;
                }

                if (!track.LastPlay.HasValue || (existingTrack.LastPlay.HasValue && track.LastPlay.Value > existingTrack.LastPlay.Value))
                    track.LastPlay = existingTrack.LastPlay;

                track.SelfReproduced = existingTrack.SelfReproduced;
                updateTracks.Add(track);

                if (existingTrack.HasPicture)
                    removePictureIds.Add(track.Id);
            }
            else
                addTracks.Add(track);

            if (track.LastPlay.HasValue && track.LastPlay.Value > now)
                track.LastPlay = now;

            FillReprodused(track, addReproduceds, updateReproduceds, withAppName, withAppNameId, trackReproducedsMap);

            if (addTracks.Count == tracksInPacket || addReproduceds.Count == tracksInPacket)
            {
                await AddTracks(addTracks, pictures, token);

                if (addReproduceds.Count > 0)
                    await AddReproduceds(addReproduceds, withAppNameId.GetValueOrDefault(), token);
            }

            if (updateTracks.Count == tracksInPacket)
                await UpdateTracks(updateTracks, pictures, removePictureIds, token);

            if (updateReproduceds.Count == tracksInPacket)
                await UpdateReproduceds(updateReproduceds, token);
        }

        if (addTracks.Count > 0)
            await AddTracks(addTracks, pictures, token);

        if (updateTracks.Count > 0)
            await UpdateTracks(updateTracks, pictures, removePictureIds, token);

        if (addReproduceds.Count > 0)
            await AddReproduceds(addReproduceds, withAppNameId.GetValueOrDefault(), token);

        if (updateReproduceds.Count > 0)
            await UpdateReproduceds(updateReproduceds, token);

        ClearAllTracks();
        progressOwner?.Hide();
    }

    private static void UpdateIdProperties(Track track, Track existingTrack)
    {
        track.Id = existingTrack.Id;
        track.Guid = existingTrack.Guid;
    }

    private static void UpdatePostProperties(Track track, Track existingTrack)
    {
        track.LastPlay = existingTrack.LastPlay;
        track.Reproduced = existingTrack.Reproduced;
        track.SelfReproduced = existingTrack.SelfReproduced;
    }

    private static bool NeedUpdateTrack(Track track, Track existingTrack,
        string? withAppName, int? withAppNameId, Dictionary<int, Dictionary<int, int>> trackReproducedsMap)
    {
        if (track.LastWrite != existingTrack.LastWrite)
            return true;

        if (track.LastPlay != existingTrack.LastPlay)
            return true;

        if (withAppNameId.HasValue && track[withAppName] is int reproduceds && (!trackReproducedsMap.TryGetValue(track.Id, out var appReproducedsMap)
            || !appReproducedsMap.TryGetValue(withAppNameId.Value, out int existingReproduced) || existingReproduced != reproduceds))
        {
            return true;
        }

        return false;
    }

    private async Task AddTracks(List<Track> tracks, List<TrackPicture> pictures, CancellationToken token)
    {
        await _connection.Insert.CreateWithParseMultiQuery<Track, Tracks>(tracks, excludedColumns: _insertExcludedColumns).FillAsync(token);
        pictures.Clear();

        foreach (var track in tracks)
        {
            var picture = track.Picture;

            if (picture is null)
                continue;

            picture.Id = track.Id;
            pictures.Add(picture);
        }

        if (pictures.Count > 0)
            await _connection.Insert.CreateWithParseMultiQuery<TrackPicture, TrackPictures>(pictures).FillAsync(token);

        tracks.Clear();
    }

    private async Task UpdateTracks(List<Track> tracks, List<TrackPicture> pictures, List<int> removePictureIds, CancellationToken token)
    {
        if (removePictureIds.Count > 0)
        {
            await _connection.Delete.CreateWithParseQuery<int, TrackPictures>(removePictureIds).RunAsync(token);
            removePictureIds.Clear();
        }

        await _connection.Update.CreateWithParseMultiQuery<Track, Tracks>(tracks, _updateExcludedColumns).RunAsync(token);
        pictures.Clear();

        foreach (var track in tracks)
        {
            var picture = track.Picture;

            if (picture is null)
                continue;

            picture.Id = track.Id;
            pictures.Add(picture);
        }

        if (pictures.Count > 0)
            await _connection.Insert.CreateWithParseMultiQuery<TrackPicture, TrackPictures>(pictures).FillAsync(token);

        tracks.Clear();
    }

    private static void FillReprodused(Track track, List<(Track, int)> addReproduceds, List<TrackReproduced> updateReproduceds,
        string? withAppName, int? withAppNameId, Dictionary<int, Dictionary<int, int>> trackReproducedsMap)
    {
        track.Reproduced = track.SelfReproduced;

        if (track.Id > 0)
        {
            if (withAppNameId.HasValue && track[withAppName] is int reproduceds)
            {
                if (!trackReproducedsMap.TryGetValue(track.Id, out var appReproducedsMap))
                    trackReproducedsMap.Add(track.Id, appReproducedsMap = []);

                if (!appReproducedsMap.TryGetValue(withAppNameId.Value, out int existingReproduced))
                {
                    appReproducedsMap.Add(withAppNameId.Value, reproduceds);
                    addReproduceds.Add((track, reproduceds));
                }
                else if (existingReproduced != reproduceds)
                {
                    appReproducedsMap[withAppNameId.Value] = reproduceds;
                    updateReproduceds.Add(new TrackReproduced(withAppNameId.Value, track.Id, reproduceds));
                }

                track.Reproduced += appReproducedsMap.Values.Sum();
            }
            else if (trackReproducedsMap.TryGetValue(track.Id, out var appReproducedsMap))
                track.Reproduced += appReproducedsMap.Values.Sum();
        }
        else if (withAppNameId.HasValue && withAppNameId.HasValue && track[withAppName] is int reproduceds)
        {
            addReproduceds.Add((track, reproduceds));
            track.Reproduced += reproduceds;
        }
    }

    private async Task AddReproduceds(List<(Track, int)> reproduceds, int withAppNameId, CancellationToken token)
    {
        var addReproduceds = new List<TrackReproduced>(reproduceds.Count);

        foreach (var pair in reproduceds)
        {
            int trackId = pair.Item1.Id;

            if (trackId <= 0)
                throw new InvalidOperationException(pair.Item1.Path);

            addReproduceds.Add(new TrackReproduced(withAppNameId, trackId, pair.Item2));
        }

        await _connection.Insert.CreateWithParseMultiQuery<TrackReproduced, TrackReproduceds>(addReproduceds).RunAsync(token);
        reproduceds.Clear();
    }

    private async Task UpdateReproduceds(List<TrackReproduced> reproduceds, CancellationToken token)
    {
        await _connection.Update.CreateWithParseMultiQuery<TrackReproduced, TrackReproduceds>(reproduceds).RunAsync(token);
        reproduceds.Clear();
    }

    public async ValueTask AddAsync(IReadOnlyCollection<Playlist> playlists, IProgress? progressOwner = null, CancellationToken token = default)
    {
        if (playlists is null || playlists.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadPlaylists);
        var existingPlaylists = await GetAllPlaylists(token);
        double progress = 0d;
        double step = 100d / playlists.Count;
        const int playlistsInPacket = 1_000;
        var addPlaylists = new List<Playlist>(playlistsInPacket);
        var updatePlaylists = new List<(Playlist Playlist, Playlist ExistingPlaylist)>(playlistsInPacket);

        var removePlaylistTracks = new DataTable();
        var removePlaylistIdColumn = removePlaylistTracks.AddInt32Column(nameof(PlaylistTracks.PlaylistId));
        var removeTrackIdColumn = removePlaylistTracks.AddInt32Column(nameof(PlaylistTracks.TrackId));

        var addPlaylistTracks = new DataTable();
        var addPlaylistIdColumn = addPlaylistTracks.AddInt32Column(nameof(PlaylistTracks.PlaylistId));
        var addTrackIdColumn = addPlaylistTracks.AddInt32Column(nameof(PlaylistTracks.TrackId));
        var addPositionColumn = addPlaylistTracks.AddInt32Column(nameof(PlaylistTracks.Position));

        foreach (var playlist in playlists)
        {
            if (progressOwner is not null)
            {
                progress += step;
                progressOwner.Next(progress, playlist.Name);
            }

            if (existingPlaylists.TryGetValue(playlist.Name, out var existingPlaylist))
            {
                playlist.Id = existingPlaylist.Id;
                playlist.Guid = existingPlaylist.Guid;
                playlist.Created = existingPlaylist.Created;

                if (NeedUpdatePlaylist(playlist, existingPlaylist))
                    updatePlaylists.Add((playlist, existingPlaylist));
            }
            else
                addPlaylists.Add(playlist);
        }

        if (addPlaylists.Count > 0)
        {
            await _connection.Insert.CreateWithParseMultiQuery<Playlist, Playlists>(addPlaylists).FillAsync(token);

            foreach (var playlist in addPlaylists)
            {
                for (int i = 0; i < playlist.Tracks.Count; i++)
                {
                    int trackId = playlist.Tracks[i].Id;

                    int row = addPlaylistTracks.RowCount++;
                    addPlaylistIdColumn.Set(row, playlist.Id);
                    addTrackIdColumn.Set(row, trackId);
                    addPositionColumn.Set(row, i + 1);
                }
            }
        }

        if (updatePlaylists.Count > 0)
        {
            foreach (var (playlist, existingPlaylist) in updatePlaylists)
            {
                int position = 0;
                var existingTrackIds = new List<int>(existingPlaylist.Tracks.Count);

                foreach (var track in existingPlaylist.Tracks)
                {
                    existingTrackIds.Add(track.Id);

                    int row = removePlaylistTracks.RowCount++;
                    removePlaylistIdColumn.Set(row, playlist.Id);
                    removeTrackIdColumn.Set(row, track.Id);
                }

                for (int i = 0; i < playlist.Tracks.Count; i++)
                {
                    int trackId = playlist.Tracks[i].Id;
                    existingTrackIds.Remove(trackId);
                    position = i + 1;

                    int row = addPlaylistTracks.RowCount++;
                    addPlaylistIdColumn.Set(row, playlist.Id);
                    addTrackIdColumn.Set(row, trackId);
                    addPositionColumn.Set(row, position);
                }

                foreach (int trackId in existingTrackIds)
                {
                    int row = addPlaylistTracks.RowCount++;
                    addPlaylistIdColumn.Set(row, playlist.Id);
                    addTrackIdColumn.Set(row, trackId);
                    addPositionColumn.Set(row, ++position);
                }
            }
        }

        if (removePlaylistTracks.RowCount > 0)
            await _connection.Delete.CreateWithParseMultiQuery<DataTable, PlaylistTracks>(removePlaylistTracks).RunAsync(token);

        if (addPlaylistTracks.RowCount > 0)
            await _connection.Insert.CreateWithParseMultiQuery<DataTable, PlaylistTracks>(addPlaylistTracks, returningIdentity: false).RunAsync(token);

        ClearAllPlaylists();
        progressOwner?.Hide();
    }

    private static bool NeedUpdatePlaylist(Playlist playlist, Playlist existingPlaylist)
    {
        if (playlist.Tracks.Count != existingPlaylist.Tracks.Count)
            return true;

        for (int i = 0; i < playlist.Tracks.Count; i++)
        {
            if (playlist.Tracks[i].Id != existingPlaylist.Tracks[i].Id)
                return true;
        }

        return false;
    }

    public async Task DeleteFolderAsync(string folderPath, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            throw new InvalidOperationException(folderPath);

        await _connection.Delete.CreateQuery(Tables.Folders)
            .With(t => t.WithValueColumnTerm(Folders.Path, Op.Equal, folderPath))
            .RunAsync(token);
    }

    public Task<List<string>> GetFoldersAsync(CancellationToken token = default)
        => _connection.Select.CreateQuery(Tables.Folders)
            .AddColumn(Folders.Path)
            .GetAsync<List<string>>(token);

#pragma warning disable CA1822 // Mark members as static
    /// <summary>Обновить треки из папок</summary>
    public void RefreshTracks(CancellationToken token = default)
#pragma warning restore CA1822 // Mark members as static
    {


        // %%TODO
    }

    /// <summary>Поднять счётчик воспроизведения</summary>
    public async Task OnEndPlayedAsync(Track track, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(track);
        track.SelfReproduced++;
        track.Reproduced++;
        track.LastPlay = DateTime.Now;

        await _connection.Update.CreateQuery<Tracks>()
            .WithTerm(Tracks.Id, track.Id)
            .AddColumn(Tracks.SelfReproduced, track.SelfReproduced)
            .AddColumn(Tracks.Reproduced, track.Reproduced)
            .AddColumn(Tracks.LastPlay, track.LastPlay)
            .RunAsync(token);
    }

    public async Task<TrackPicture?> LoadPictureAsync(Track track, CancellationToken token = default)
        => track.Picture = await _connection.Select.CreateWithParseQuery<TrackPicture, TrackPictures>().GetOneAsync(TrackPictures.Id, track.Id, token);
}
