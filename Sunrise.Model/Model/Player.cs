using System.Text;
using IcyRain.Tables;
using RedLight;
using RedLight.SQLite;
using Sunrise.Model.Resources;
using Sunrise.Model.Schemes;
using Sunrise.Model.TagLib;

namespace Sunrise.Model;

public sealed class Player
{
    private static readonly HashSet<string> _insertExcludedColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Tracks.Picked)
    };

    private static readonly HashSet<string> _updateTracksExcludedColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Tracks.Guid), nameof(Tracks.Path), nameof(Tracks.Picked), nameof(Tracks.Rating), nameof(Tracks.Reproduced),
        nameof(Tracks.Added), nameof(Tracks.RootFolder), nameof(Tracks.RelationFolder), nameof(Tracks.OriginalText),
        nameof(Tracks.TranslateText), nameof(Tracks.Language)
    };

    private static readonly HashSet<string> _updatePlaylistsExcludedColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Playlists.Guid)
    };

    private static readonly HashSet<string> _updateCategoriesExcludedColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Categories.Guid)
    };

    private readonly DatabaseConnection _connection;

    static Player()
        => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private Player(string folderPath, string tracksPath, DatabaseConnection connection)
    {
        FolderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
        TracksPath = tracksPath ?? throw new ArgumentNullException(nameof(tracksPath));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Media = new(this);
    }

    public MediaPlayer Media { get; }

    public string FolderPath { get; }

    public string TracksPath { get; }

    public static async Task<Player> InitAsync(string? rootFolder = null, CancellationToken token = default)
    {
        bool isDevice = OperatingSystem.IsAndroid();
        rootFolder ??= Environment.GetFolderPath(isDevice ? Environment.SpecialFolder.LocalApplicationData : Environment.SpecialFolder.MyMusic);

        string folderPath = Path.Combine(rootFolder, "Sunrise");
        Directory.CreateDirectory(folderPath);

        string tracksPath = Path.Combine(folderPath, "Tracks");

        if (isDevice)
            Directory.CreateDirectory(tracksPath);

        string databaseFilePath = Path.Combine(folderPath, "MediaLibrary.db");
        string connectionString = $@"Provider=SQLite;Data Source='{databaseFilePath}'";
        var connection = SQLiteDatabaseConnection.Create(connectionString);

        if (!System.IO.File.Exists(databaseFilePath))
            await CreateDatabaseAsync(connection, token);

        return new Player(folderPath, tracksPath, connection);
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
        await connection.Schema.CreateTableWithParseQuery<AppNames>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<Tracks>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<TrackPictures>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<Categories>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<Playlists>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<PlaylistTracks>().RunAsync(token);
        await connection.Schema.CreateTableWithParseQuery<PlaylistCategories>().RunAsync(token);

        await connection.Schema.CreateTableWithParseQuery<TrackReproduceds>().RunAsync(token);
    }

    #region Tracks

    private TracksScreenshot? _tracks;
    private readonly object _tracksSync = new();

    public bool IsTracksLoaded()
    {
        lock (_tracksSync)
            return _tracks is not null;
    }

    public async Task<TracksScreenshot> GetTracksAsync(CancellationToken token = default)
    {
        TracksScreenshot? screenshot;

        lock (_tracksSync)
            screenshot = _tracks;

        if (screenshot is not null)
            return screenshot;

        var tracks = await _connection.Select.CreateWithParseQuery<Track, Tracks>().GetAsync(token);
        screenshot = new TracksScreenshot(tracks);

        lock (_tracksSync)
            _tracks = screenshot;

        return screenshot;
    }

    public void ClearTracks()
    {
        lock (_tracksSync)
            _tracks = null;
    }

    #endregion
    #region Categories

    private CategoriesScreenshot? _categories;
    private readonly object _categoriesSync = new();

    public async Task<CategoriesScreenshot> GetCategoriesAsync(CancellationToken token = default)
    {
        CategoriesScreenshot? screenshot;

        lock (_categoriesSync)
            screenshot = _categories;

        if (screenshot is not null)
            return screenshot;

        var categories = await _connection.Select.CreateWithParseQuery<Category, Categories>().GetAsync(token);
        screenshot = new CategoriesScreenshot(categories);

        lock (_categoriesSync)
            _categories = screenshot;

        return screenshot;
    }

    public async Task<Category> AddCategoryAsync(CancellationToken token = default)
    {
        var categoriesScreenshot = await GetCategoriesAsync(token);
        string name = FindNewCategoryName(categoriesScreenshot);

        var category = new Category()
        {
            Guid = Guid.NewGuid(),
            Name = name,
        };

        await _connection.Insert.CreateWithParseQuery<Category, Categories>(category).FillAsync(token);
        categoriesScreenshot.Add(category);
        return category;
    }

    public async ValueTask AddAsync(IReadOnlyCollection<Category> categories,
        bool sync = false, IProgress? progressOwner = null, CancellationToken token = default)
    {
        if (categories is null || categories.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadCategories);
        var existingCategories = await GetCategoriesAsync(token);
        double progress = 0d;
        double step = 100d / categories.Count;
        const int categoriesInPacket = 1_000;
        var addCategories = new List<Category>(categoriesInPacket);
        var updateCategories = new List<Category>(categoriesInPacket);

        foreach (var category in categories)
        {
            if (progressOwner is not null)
            {
                progress += step;
                progressOwner.Next(progress, category.Name);
            }

            if (sync)
            {
                if (existingCategories.CategoriesByGuid.TryGetValue(category.Guid, out var existingCategory))
                {
                    if (category.Name != existingCategory.Name)
                    {
                        category.Name = existingCategory.Name;
                        updateCategories.Add(category);
                    }
                }
                else
                    addCategories.Add(category);
            }
            else
            {
                if (existingCategories.CategoriesByName.TryGetValue(category.Name, out var existingCategory))
                {
                    category.Id = existingCategory.Id;
                    category.Guid = existingCategory.Guid;
                    updateCategories.Add(category);
                }
                else
                    addCategories.Add(category);
            }
        }

        if (addCategories.Count > 0)
            await _connection.Insert.CreateWithParseMultiQuery<Category, Categories>(addCategories).FillAsync(token);

        if (updateCategories.Count > 0)
            await _connection.Update.CreateWithParseMultiQuery<Category, Categories>(updateCategories, _updateCategoriesExcludedColumns).RunAsync(token);

        ClearCategories();
        progressOwner?.Hide();
    }

    private static string FindNewCategoryName(CategoriesScreenshot screenshot)
    {
        string name = Texts.Category;

        if (screenshot.CategoriesByName.ContainsKey(name))
        {
            int number = 2;

            while (true)
            {
                name = $"{Texts.Category} {number++}";

                if (!screenshot.CategoriesByName.ContainsKey(name))
                    break;
            }
        }

        return name;
    }

    public async Task<bool> ChangeCategoryNameAsync(Category? category, string name, CancellationToken token = default)
    {
        if (category is null || name is null)
            return false;

        name = name.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty).Trim();

        if (string.IsNullOrEmpty(name) || category.Name == name)
            return false;

        var categoriesScreenshot = await GetCategoriesAsync(token);

        if (categoriesScreenshot.CategoriesByName.ContainsKey(name))
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

        lock (_categoriesSync)
            _categories?.Remove(category);

        return await _connection.Delete.CreateQuery<Categories>()
            .WithTerm(Categories.Id, category.Id)
            .RunAsync(token) > 0;
    }

    public async Task DeleteCategoriesAsync(IReadOnlyList<Guid>? categories, CancellationToken token = default)
    {
        if (categories is null || categories.Count == 0)
            return;

        var categoryIds = await _connection.Select.CreateQuery<Categories>()
            .WithValuesTerm(Categories.Guid, categories)
            .GetAsync<HashSet<int>>(token);

        lock (_categoriesSync)
            _categories?.Remove(categoryIds);

        await _connection.Delete.CreateQuery<Categories>()
            .WithValuesTerm(Categories.Id, categoryIds)
            .RunAsync(token);
    }

    public void ClearCategories()
    {
        lock (_categoriesSync)
            _categories = null;
    }

    #endregion
    #region Playlists

    private Dictionary<string, Playlist>? _playlistsByName;
    private readonly object _playlistsSync = new();

    public async Task<Dictionary<string, Playlist>> GetPlaylistsAsync(CancellationToken token = default)
    {
        Dictionary<string, Playlist>? playlistsByName;

        lock (_playlistsSync)
            playlistsByName = _playlistsByName;

        if (playlistsByName is not null)
            return playlistsByName;

        var tracksScreenshot = await GetTracksAsync(token);
        var categoriesScreenshot = await GetCategoriesAsync(token);
        var playlists = await _connection.Select.CreateWithParseQuery<Playlist, Playlists>().GetAsync(token);
        var playlistsById = new Dictionary<int, Playlist>(playlists.Count);
        playlistsByName = new(playlists.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var playlist in playlists)
        {
            playlist.Tracks = [];
            playlist.Categories = [];
            playlistsById.Add(playlist.Id, playlist);
            playlistsByName[playlist.Name] = playlist;
        }

        await FillTracksAsync(playlistsById, tracksScreenshot, token);
        await FillCategoriesAsync(playlistsById, categoriesScreenshot, token);

        lock (_playlistsSync)
            _playlistsByName = playlistsByName;

        return playlistsByName;
    }

    private async Task FillTracksAsync(Dictionary<int, Playlist> playlistsById, TracksScreenshot tracks, CancellationToken token)
    {
        var playlistTracks = await _connection.Select.CreateWithParseQuery<PlaylistTracks>()
            .OrderBy(PlaylistTracks.Position).GetAsync<DataTable>(token);

        var playlistIdColumn = playlistTracks[nameof(PlaylistTracks.PlaylistId)];
        var trackIdColumn = playlistTracks[nameof(PlaylistTracks.TrackId)];

        for (int row = 0; row < playlistTracks.RowCount; row++)
        {
            int playlistId = playlistIdColumn.GetInt(row);
            int trackId = trackIdColumn.GetInt(row);

            if (playlistsById.TryGetValue(playlistId, out var playlist)
                && tracks.TracksById.TryGetValue(trackId, out var track))
            {
                playlist.Tracks.Add(track);
            }
        }
    }

    private async Task FillCategoriesAsync(Dictionary<int, Playlist> playlistsById, CategoriesScreenshot categories,
        CancellationToken token)
    {
        var playlistCategories = await _connection.Select.CreateWithParseQuery<PlaylistCategories>().GetAsync<DataTable>(token);

        var playlistIdColumn = playlistCategories[nameof(PlaylistCategories.PlaylistId)];
        var categoryIdColumn = playlistCategories[nameof(PlaylistCategories.CategoryId)];

        for (int row = 0; row < playlistCategories.RowCount; row++)
        {
            int playlistId = playlistIdColumn.GetInt(row);
            int categoryId = categoryIdColumn.GetInt(row);

            if (playlistsById.TryGetValue(playlistId, out var playlist)
                && categories.CategoriesById.TryGetValue(categoryId, out var category))
            {
                playlist.Categories.Add(category);
            }
        }
    }

    public async Task<Playlist> AddPlaylistAsync(CancellationToken token = default)
    {
        var playlists = await GetPlaylistsAsync(token);
        string name = FindNewPlaylistName(playlists);

        var playlist = new Playlist()
        {
            Guid = Guid.NewGuid(),
            Name = name,
            Created = DateTime.Now,
            Tracks = [],
            Categories = [],
        };

        await _connection.Insert.CreateWithParseQuery<Playlist, Playlists>(playlist).FillAsync(token);
        playlists.Add(name, playlist);
        return playlist;
    }

    public async Task AddTrackInPlaylistAsync(Playlist? playlist, Track? track, CancellationToken token = default)
    {
        if (playlist is null || track is null)
            return;

        await _connection.Insert.CreateQuery<PlaylistTracks>()
            .AddColumn(nameof(PlaylistTracks.PlaylistId), playlist.Id)
            .AddColumn(nameof(PlaylistTracks.TrackId), track.Id)
            .AddColumn(nameof(PlaylistTracks.Position), playlist.Tracks.Count + 1)
            .RunAsync(token);

        playlist.Tracks.Add(track);
    }

    public async Task AddCategoryInPlaylistAsync(Playlist? playlist, Category? category, CancellationToken token = default)
    {
        if (playlist is null || category is null)
            return;

        await _connection.Insert.CreateQuery<PlaylistCategories>()
            .AddColumn(nameof(PlaylistCategories.PlaylistId), playlist.Id)
            .AddColumn(nameof(PlaylistCategories.CategoryId), category.Id)
            .RunAsync(token);

        playlist.Categories.Add(category);
    }

    public async Task DeleteCategoryInPlaylistAsync(Playlist? playlist, Category? category, CancellationToken token = default)
    {
        if (playlist is null || category is null)
            return;

        await _connection.Delete.CreateQuery<PlaylistCategories>()
            .WithTerm(nameof(PlaylistCategories.PlaylistId), playlist.Id)
            .WithTerm(nameof(PlaylistCategories.CategoryId), category.Id)
            .RunAsync(token);

        playlist.Categories.Remove(category);
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
        if (playlist is null || name is null)
            return false;

        name = name.Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty).Trim();

        if (string.IsNullOrEmpty(name) || playlist.Name == name)
            return false;

        var playlists = await GetPlaylistsAsync(token);

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

    public async Task<bool> DeleteTrackAsync(Track? track, CancellationToken token = default)
    {
        if (track is null)
            return false;

        int trackId = track.Id;

        lock (_tracksSync)
            _tracks?.Remove(track);

        lock (_playlistsSync)
        {
            var playlistsByName = _playlistsByName;

            if (playlistsByName is not null)
            {
                foreach (var playlist in playlistsByName.Values)
                {
                    var playlistTracks = playlist.Tracks;

                    for (int i = playlistTracks.Count - 1; i >= 0; i--)
                    {
                        if (playlistTracks[i].Id == trackId)
                            playlistTracks.RemoveAt(i);
                    }
                }
            }
        }

        bool result = await _connection.Delete.CreateQuery<Tracks>()
            .WithTerm(Tracks.Id, trackId)
            .RunAsync(token) > 0;

        if (!result)
            return false;

        await _connection.Delete.CreateQuery<PlaylistTracks>()
            .WithTerm(PlaylistTracks.TrackId, trackId)
            .RunAsync(token);

        return true;
    }

    public async Task DeleteTracksAsync(IReadOnlyList<Guid>? tracks, CancellationToken token = default)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        var trackIds = await _connection.Select.CreateQuery<Tracks>()
            .WithValuesTerm(Tracks.Guid, tracks)
            .GetAsync<HashSet<int>>(token);

        lock (_tracksSync)
            _tracks?.Remove(trackIds);

        lock (_playlistsSync)
        {
            var playlistsByName = _playlistsByName;

            if (playlistsByName is not null)
            {
                foreach (var playlist in playlistsByName.Values)
                {
                    var playlistTracks = playlist.Tracks;

                    for (int i = playlistTracks.Count - 1; i >= 0; i--)
                    {
                        if (trackIds.Contains(playlistTracks[i].Id))
                            playlistTracks.RemoveAt(i);
                    }
                }
            }
        }

        bool result = await _connection.Delete.CreateQuery<Tracks>()
            .WithValuesTerm(Tracks.Id, trackIds)
            .RunAsync(token) > 0;

        if (!result)
            return;

        await _connection.Delete.CreateQuery<PlaylistTracks>()
            .WithValuesTerm(PlaylistTracks.TrackId, trackIds)
            .RunAsync(token);
    }

    public async Task<bool> DeletePlaylistAsync(Playlist? playlist, CancellationToken token = default)
    {
        if (playlist is null)
            return false;

        lock (_playlistsSync)
            _playlistsByName?.Remove(playlist.Name);

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

    public async Task DeletePlaylistsAsync(IReadOnlyList<Guid>? playlists, CancellationToken token = default)
    {
        if (playlists is null || playlists.Count == 0)
            return;

        var playlistIds = await _connection.Select.CreateQuery<Playlists>()
            .WithValuesTerm(Playlists.Guid, playlists)
            .GetAsync<HashSet<int>>(token);

        lock (_playlistsSync)
        {
            var playlistsByName = _playlistsByName;

            if (playlistsByName is not null)
            {
                var names = playlistsByName.Values.Where(p => playlistIds.Contains(p.Id)).Select(p => p.Name).ToList();

                foreach (string name in names)
                    playlistsByName.Remove(name);
            }
        }

        bool result = await _connection.Delete.CreateQuery<Playlists>()
            .WithValuesTerm(Playlists.Id, playlistIds)
            .RunAsync(token) > 0;

        if (!result)
            return;

        await _connection.Delete.CreateQuery<PlaylistTracks>()
            .WithValuesTerm(PlaylistTracks.PlaylistId, playlistIds)
            .RunAsync(token);
    }

    public void ClearPlaylists()
    {
        lock (_playlistsSync)
            _playlistsByName = null;
    }

    #endregion
    #region Search

    public async ValueTask<SearchResults> SearchAsync(string text, int maxCountForRubric = 10, CancellationToken token = default)
    {
        var tracks = new List<Track>();
        var artists = new List<(string Name, Dictionary<string, List<Track>> TracksByAlbums)>();
        var albums = new List<(string Name, string Artist, List<Track> Tracks)>();
        var genres = new List<(string Name, List<Track> Tracks)>();
        text = text?.Trim();

        if (string.IsNullOrEmpty(text))
            return new(tracks, artists, albums, genres);

        if (maxCountForRubric < 1)
            maxCountForRubric = 1;

        var tracksScreenshot = await GetTracksAsync(token);
        SearchTracks(text, maxCountForRubric, tracksScreenshot, tracks);
        SearchArtists(text, maxCountForRubric, tracksScreenshot, artists);
        SearchAlbums(text, maxCountForRubric, tracksScreenshot, albums);
        SearchGenres(text, maxCountForRubric, tracksScreenshot, genres);
        return new(tracks, artists, albums, genres);
    }

    private static void SearchTracks(string text, int maxCountForRubric, TracksScreenshot screenshot,
        List<Track> tracks)
    {
        foreach (var track in screenshot.Tracks)
        {
            if (track.Title?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                tracks.Add(track);

                if (tracks.Count == maxCountForRubric)
                    return;
            }
        }
    }

    private static void SearchArtists(string text, int maxCountForRubric, TracksScreenshot screenshot,
        List<(string Name, Dictionary<string, List<Track>> TracksByAlbums)> artists)
    {
        foreach (var artistPair in screenshot.TracksByArtist)
        {
            if (artistPair.Key.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                artists.Add((artistPair.Key, artistPair.Value));

                if (artists.Count == maxCountForRubric)
                    return;
            }
        }
    }

    private static void SearchAlbums(string text, int maxCountForRubric, TracksScreenshot screenshot,
        List<(string Name, string Artist, List<Track> Tracks)> albums)
    {
        foreach (var artistPair in screenshot.TracksByArtist)
        {
            foreach (var albumPair in artistPair.Value)
            {
                if (albumPair.Key.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    albums.Add((albumPair.Key, artistPair.Key, albumPair.Value));

                    if (albums.Count == maxCountForRubric)
                        return;
                }
            }
        }
    }

    private static void SearchGenres(string text, int maxCountForRubric, TracksScreenshot screenshot,
        List<(string Name, List<Track> Tracks)> genres)
    {
        foreach (var genrePair in screenshot.TracksByGenre)
        {
            if (genrePair.Key.Contains(text, StringComparison.OrdinalIgnoreCase))
            {
                genres.Add((genrePair.Key, genrePair.Value));

                if (genres.Count == maxCountForRubric)
                    return;
            }
        }
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

        string searchPattern = String.Join("|", SupportedMimeType.AllExtensions.Select(e => $"*.{e}"));
        var files = directoryInfo.GetFiles("*.mp3", SearchOption.AllDirectories);
        await AddAsync(files, progressOwner, token);
        return true;
    }

    public async ValueTask AddAsync(IReadOnlyCollection<FileInfo> files, IProgress? progressOwner = null, CancellationToken token = default)
    {
        if (files is null || files.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadTracks);
        var tracksScreenshot = await GetTracksAsync(token);
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

            if (tracksScreenshot.TracksByPath.TryGetValue(file.FullName, out var existingTrack))
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

        ClearTracks();
        progressOwner?.Hide();
    }

    public async ValueTask AddAsync(IReadOnlyCollection<Track> tracks,
        bool sync = false, IProgress? progressOwner = null, string? withAppName = null, CancellationToken token = default)
    {
        if (tracks is null || tracks.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadTracks);
        var tracksScreenshot = await GetTracksAsync(token);
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
        var trackReproducedsMap = new Dictionary<int, Dictionary<int, int>>(tracksScreenshot.Tracks.Count); // Track -> App -> Reproduced

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

            if (sync ? tracksScreenshot.TracksByGuid.TryGetValue(track.Guid, out Track existingTrack)
                : tracksScreenshot.TracksByPath.TryGetValue(track.Path, out existingTrack))
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

            FillReproduced(track, addReproduceds, updateReproduceds, withAppName, withAppNameId, trackReproducedsMap);

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

        ClearTracks();
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

        await _connection.Update.CreateWithParseMultiQuery<Track, Tracks>(tracks, _updateTracksExcludedColumns).RunAsync(token);
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

    private static void FillReproduced(Track track, List<(Track, int)> addReproduceds, List<TrackReproduced> updateReproduceds,
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

    public async ValueTask AddAsync(IReadOnlyCollection<Playlist> playlists,
        bool sync = false, IProgress? progressOwner = null, CancellationToken token = default)
    {
        if (playlists is null || playlists.Count == 0)
            return;

        progressOwner?.Show(Texts.LoadPlaylists);
        var existingPlaylists = await GetPlaylistsAsync(token);
        var existingPlaylistsByGuid = sync ? existingPlaylists.Values.ToDictionary(p => p.Guid) : null;
        double progress = 0d;
        double step = 100d / playlists.Count;
        const int playlistsInPacket = 1_000;
        var addPlaylists = new List<Playlist>(playlistsInPacket);
        var updatePlaylists = new List<Playlist>(playlistsInPacket);
        var updatePlaylistTracks = new List<(Playlist Playlist, Playlist ExistingPlaylist)>(playlistsInPacket);

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

            if (sync ? existingPlaylistsByGuid!.TryGetValue(playlist.Guid, out var existingPlaylist)
                : existingPlaylists.TryGetValue(playlist.Name, out existingPlaylist))
            {
                updatePlaylists.Add(playlist);

                if (sync)
                    playlist.Name = existingPlaylist.Name;
                else
                {
                    playlist.Id = existingPlaylist.Id;
                    playlist.Guid = existingPlaylist.Guid;
                }

                playlist.Created = existingPlaylist.Created;

                if (NeedUpdatePlaylist(playlist, existingPlaylist))
                    updatePlaylistTracks.Add((playlist, existingPlaylist));
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
            await _connection.Update.CreateWithParseMultiQuery<Playlist, Playlists>(updatePlaylists, _updatePlaylistsExcludedColumns).RunAsync(token);

        if (updatePlaylistTracks.Count > 0)
        {
            foreach (var (playlist, existingPlaylist) in updatePlaylistTracks)
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

        ClearPlaylists();
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

    public async Task ChangeRatingAsync(Track track, byte value, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(track);

        if (track.Rating == value)
            return;

        track.Rating = value;

        await _connection.Update.CreateQuery<Tracks>()
            .WithTerm(Tracks.Id, track.Id)
            .AddColumn(Tracks.Rating, value)
            .RunAsync(token);
    }

    public async Task<TrackPicture?> LoadPictureAsync(Track track, CancellationToken token = default)
        => track.Picture = await _connection.Select.CreateWithParseQuery<TrackPicture, TrackPictures>().GetOneAsync(TrackPictures.Id, track.Id, token);

    public Task<List<TrackPicture>> LoadPicturesAsync(IReadOnlyCollection<Track> tracks, CancellationToken token = default)
        => _connection.Select.CreateWithParseQuery<TrackPicture, TrackPictures>()
            .WithValuesTerm(TrackPictures.Id, GetIds(tracks)).GetAsync(token);

    private static int[] GetIds(IReadOnlyCollection<Track> tracks)
    {
        var ids = new int[tracks.Count];
        int i = 0;

        foreach (var track in tracks)
            ids[i++] = track.Id;

        return ids;
    }

    public async Task DeleteAllMediaAsync(CancellationToken token = default)
    {
        await _connection.Delete.CreateQuery<Tracks>().RunAsync(token);
        await _connection.Delete.CreateQuery<TrackPictures>().RunAsync(token);

        await _connection.Delete.CreateQuery<Categories>().RunAsync(token);

        await _connection.Delete.CreateQuery<Playlists>().RunAsync(token);
        await _connection.Delete.CreateQuery<PlaylistTracks>().RunAsync(token);
        await _connection.Delete.CreateQuery<PlaylistCategories>().RunAsync(token);

        await _connection.Delete.CreateQuery<TrackReproduceds>().RunAsync(token);
    }

}
