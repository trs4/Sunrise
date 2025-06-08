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
            await CreateDatabaseAsync(connection, token);

        return new Player(folderPath, connection);
    }

    private static async Task CreateDatabaseAsync(DatabaseConnection connection, CancellationToken token)
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

        await connection.Schema.CreateTableWithParseQuery<Categories>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<Playlists>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<PlaylistTracks>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<AppNames>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<TrackReproduceds>().RunAsync(token);
    }

    #region Tracks

    private TracksScreenshot? _tracksScreenshot;
    private readonly object _tracksScreenshotSync = new();

    public bool IsAllTracksLoaded()
    {
        lock (_tracksScreenshotSync)
            return _tracksScreenshot is not null;
    }

    public async Task<TracksScreenshot> GetAllTracksAsync(CancellationToken token = default)
    {
        TracksScreenshot? tracksScreenshot;

        lock (_tracksScreenshotSync)
            tracksScreenshot = _tracksScreenshot;

        if (tracksScreenshot is not null)
            return tracksScreenshot;

        var allTracks = await _connection.Select.CreateWithParseQuery<Track, Tracks>().GetAsync(token);
        var allTracksById = new Dictionary<int, Track>(allTracks.Count);

        foreach (var track in allTracks)
            allTracksById.Add(track.Id, track);

        tracksScreenshot = new(allTracks, allTracksById);

        lock (_tracksScreenshotSync)
            _tracksScreenshot = tracksScreenshot;

        return tracksScreenshot;
    }

    public void ClearAllTracks()
    {
        lock (_tracksScreenshotSync)
            _tracksScreenshot = null;
    }

    #endregion
    #region Categories

    private Dictionary<string, Category>? _allCategoriesByName;
    private readonly object _allCategoriesSync = new();

    public async Task<Dictionary<string, Category>> GetAllCategoriesAsync(CancellationToken token = default)
    {
        Dictionary<string, Category>? allCategoriesByName;

        lock (_allPlaylistsSync)
            allCategoriesByName = _allCategoriesByName;

        if (allCategoriesByName is not null)
            return allCategoriesByName;

        var categories = await _connection.Select.CreateWithParseQuery<Category, Categories>().GetAsync(token);
        var allCategoriesById = new Dictionary<int, Category>(categories.Count);
        allCategoriesByName = new(categories.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var category in categories)
        {
            allCategoriesById.Add(category.Id, category);
            allCategoriesByName[category.Name] = category;
        }

        lock (_allCategoriesSync)
            _allCategoriesByName = allCategoriesByName;

        return allCategoriesByName;
    }

    public async Task<Category> AddCategoryAsync(CancellationToken token = default)
    {
        var categories = await GetAllCategoriesAsync(token);
        string name = FindNewCategoryName(categories);

        var category = new Category()
        {
            Guid = Guid.NewGuid(),
            Name = name,
        };

        await _connection.Insert.CreateWithParseQuery<Category, Categories>(category).FillAsync(token);
        categories.Add(name, category);
        return category;
    }

    private static string FindNewCategoryName(Dictionary<string, Category> categories)
    {
        string name = Texts.Category;

        if (categories.ContainsKey(name))
        {
            int number = 2;

            while (true)
            {
                name = $"{Texts.Category} {number++}";

                if (!categories.ContainsKey(name))
                    break;
            }
        }

        return name;
    }

    public async Task<bool> ChangeCategoryNameAsync(Category? category, string name, CancellationToken token = default)
    {
        if (category is null || string.IsNullOrWhiteSpace(name))
            return false;

        name = name.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);

        if (category.Name == name)
            return false;

        var categories = await GetAllCategoriesAsync(token);

        if (categories.ContainsKey(name))
            return false;

        bool result = await _connection.Update.CreateQuery<Categories>()
            .WithTerm(Categories.Id, category.Id)
            .AddColumn(Categories.Name, name)
            .RunAsync(token) > 0;

        if (!result)
            return true;

        category.Name = name;
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(Category? category, CancellationToken token = default)
    {
        if (category is null)
            return false;

        lock (_allCategoriesSync)
            _allCategoriesByName?.Remove(category.Name);

        return await _connection.Delete.CreateQuery<Categories>()
            .WithTerm(Categories.Id, category.Id)
            .RunAsync(token) > 0;
    }

    public void ClearAllCategories()
    {
        lock (_allCategoriesSync)
            _allCategoriesByName = null;
    }

    #endregion
    #region Playlists

    private Dictionary<string, Playlist>? _allPlaylistsByName;
    private readonly object _allPlaylistsSync = new();

    public async Task<Dictionary<string, Playlist>> GetAllPlaylistsAsync(CancellationToken token = default)
    {
        Dictionary<string, Playlist>? allPlaylistsByName;

        lock (_allPlaylistsSync)
            allPlaylistsByName = _allPlaylistsByName;

        if (allPlaylistsByName is not null)
            return allPlaylistsByName;

        var screenshot = await GetAllTracksAsync(token);
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

    public async Task<Playlist> AddPlaylistAsync(CancellationToken token = default)
    {
        var playlists = await GetAllPlaylistsAsync(token);
        string name = FindNewPlaylistName(playlists);

        var playlist = new Playlist()
        {
            Guid = Guid.NewGuid(),
            Name = name,
            Created = DateTime.Now,
            Tracks = [],
        };

        await _connection.Insert.CreateWithParseQuery<Playlist, Playlists>(playlist).FillAsync(token);
        playlists.Add(name, playlist);
        return playlist;
    }

    private static string FindNewPlaylistName(Dictionary<string, Playlist> playlists)
    {
        string name = Texts.Playlist;

        if (playlists.ContainsKey(name))
        {
            int number = 2;

            while (true)
            {
                name = $"{Texts.Playlist} {number++}";

                if (!playlists.ContainsKey(name))
                    break;
            }
        }

        return name;
    }

    public async Task<bool> ChangePlaylistNameAsync(Playlist? playlist, string name, CancellationToken token = default)
    {
        if (playlist is null || string.IsNullOrWhiteSpace(name))
            return false;

        name = name.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);

        if (playlist.Name == name)
            return false;

        var playlists = await GetAllPlaylistsAsync(token);

        if (playlists.ContainsKey(name))
            return false;

        bool result = await _connection.Update.CreateQuery<Playlists>()
            .WithTerm(Playlists.Id, playlist.Id)
            .AddColumn(Playlists.Name, name)
            .RunAsync(token) > 0;

        if (!result)
            return true;

        playlist.Name = name;
        return true;
    }

    public async Task<bool> DeletePlaylistAsync(Playlist? playlist, CancellationToken token = default)
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

    public void ClearAllPlaylists()
    {
        lock (_allPlaylistsSync)
            _allPlaylistsByName = null;
    }

    #endregion

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
        var screenshot = await GetAllTracksAsync(token);
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
                await AddTracksAsync(addTracks, pictures, token);

            if (updateTracks.Count == tracksInPacket)
                await UpdateTracksAsync(updateTracks, pictures, removePictureIds, token);
        }

        if (addTracks.Count > 0)
            await AddTracksAsync(addTracks, pictures, token);

        if (updateTracks.Count > 0)
            await UpdateTracksAsync(updateTracks, pictures, removePictureIds, token);

        ClearAllTracks();
        progressOwner?.Hide();
    }

    public async ValueTask AddAsync(IReadOnlyCollection<Track> tracks, IProgress? progressOwner = null, string? withAppName = null,
        CancellationToken token = default)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadTracks);
        var screenshot = await GetAllTracksAsync(token);
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
                await AddTracksAsync(addTracks, pictures, token);

                if (addReproduceds.Count > 0)
                    await AddReproducedsAsync(addReproduceds, withAppNameId.GetValueOrDefault(), token);
            }

            if (updateTracks.Count == tracksInPacket)
                await UpdateTracksAsync(updateTracks, pictures, removePictureIds, token);

            if (updateReproduceds.Count == tracksInPacket)
                await UpdateReproducedsAsync(updateReproduceds, token);
        }

        if (addTracks.Count > 0)
            await AddTracksAsync(addTracks, pictures, token);

        if (updateTracks.Count > 0)
            await UpdateTracksAsync(updateTracks, pictures, removePictureIds, token);

        if (addReproduceds.Count > 0)
            await AddReproducedsAsync(addReproduceds, withAppNameId.GetValueOrDefault(), token);

        if (updateReproduceds.Count > 0)
            await UpdateReproducedsAsync(updateReproduceds, token);

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

    private async Task AddTracksAsync(List<Track> tracks, List<TrackPicture> pictures, CancellationToken token)
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

    private async Task UpdateTracksAsync(List<Track> tracks, List<TrackPicture> pictures, List<int> removePictureIds, CancellationToken token)
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

    private async Task AddReproducedsAsync(List<(Track, int)> reproduceds, int withAppNameId, CancellationToken token)
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

    private async Task UpdateReproducedsAsync(List<TrackReproduced> reproduceds, CancellationToken token)
    {
        await _connection.Update.CreateWithParseMultiQuery<TrackReproduced, TrackReproduceds>(reproduceds).RunAsync(token);
        reproduceds.Clear();
    }

    public async ValueTask AddAsync(IReadOnlyCollection<Playlist> playlists, IProgress? progressOwner = null, CancellationToken token = default)
    {
        if (playlists is null || playlists.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadPlaylists);
        var existingPlaylists = await GetAllPlaylistsAsync(token);
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

    //#pragma warning disable CA1822 // Mark members as static
    //    /// <summary>Обновить треки из папок</summary>
    //    public void RefreshTracks(CancellationToken token = default)
    //#pragma warning restore CA1822 // Mark members as static
    //    {


    //        // %%TODO
    //    }

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

    public async Task ChangePickedAsync(Track track, bool value, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(track);

        if (track.Picked == value)
            return;

        track.Picked = value;

        await _connection.Update.CreateQuery<Tracks>()
            .WithTerm(Tracks.Id, track.Id)
            .AddColumn(Tracks.Picked, value)
            .RunAsync(token);
    }

    public async Task<TrackPicture?> LoadPictureAsync(Track track, CancellationToken token = default)
        => track.Picture = await _connection.Select.CreateWithParseQuery<TrackPicture, TrackPictures>().GetOneAsync(TrackPictures.Id, track.Id, token);
}
