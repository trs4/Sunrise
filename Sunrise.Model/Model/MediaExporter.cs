using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Sunrise.Model.Communication;
using Sunrise.Model.Model.Exchange;

namespace Sunrise.Model.Model;

public static class MediaExporter
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };

    public static async Task ExportAsync(Player player, Stream stream, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(stream);
        var devices = await player.GetDevicesAsync(token);
        var folders = await player.GetFoldersAsync(token);
        var tracksScreenshot = await player.GetTracksAsync(token);
        var playlists = await player.GetPlaylistsAsync(token);
        var categoriesScreenshot = await player.GetCategoriesAsync(token);
        var document = BuildDocument(devices, folders, tracksScreenshot, playlists, categoriesScreenshot);

        JsonSerializer.Serialize(stream, document, _options);
        stream.Flush();
    }

    private static MediaDocument BuildDocument(List<Device> devices, List<string> folders,
        TracksScreenshot tracksScreenshot, Dictionary<string, Playlist> playlists, CategoriesScreenshot categoriesScreenshot)
    {
        var mainDevice = devices.First(d => d.IsMain);

        return new MediaDocument()
        {
            Name = mainDevice.Name,
            Guid = mainDevice.Guid,
            Version = SyncServiceManager.Version,
            Date = DateTime.Now,
            Folders = folders.Count == 0 ? null : folders,
            Tracks = BuildTracks(tracksScreenshot),
            Playlists = BuildPlaylists(playlists),
            Categories = BuildCategories(categoriesScreenshot),
        };
    }

    private static List<TrackElement>? BuildTracks(TracksScreenshot tracksScreenshot)
    {
        var trackElements = new List<TrackElement>(tracksScreenshot.Tracks.Count);

        foreach (var track in tracksScreenshot.Tracks)
        {
            var trackElement = new TrackElement()
            {
                Title = track.Title ?? string.Empty,
                Guid = track.Guid,
                Path = track.Path,
                Year = track.Year,
                Duration = track.Duration,
                Rating = track.Rating,
                Artist = track.Artist ?? string.Empty,
                Genre = track.Genre ?? string.Empty,
                LastPlay = track.LastPlay,
                Reproduced = track.Reproduced,
                Album = track.Album ?? string.Empty,
                Created = track.Created,
                Added = track.Added,
                Bitrate = track.Bitrate,
                Size = track.Size,
                LastWrite = track.LastWrite,
                HasPicture = track.HasPicture,
            };

            trackElements.Add(trackElement);
        }

        return trackElements.Count == 0 ? null : trackElements;
    }

    private static List<PlaylistElement>? BuildPlaylists(Dictionary<string, Playlist> playlists)
    {
        var playlistElements = new List<PlaylistElement>(playlists.Count);

        foreach (var playlist in playlists.Values)
        {
            var playlistElement = new PlaylistElement()
            {
                Name = playlist.Name,
                Guid = playlist.Guid,
                Created = playlist.Created,
                Tracks = [.. playlist.Tracks.Select(t => t.Guid)],
                Categories = [.. playlist.Categories.Select(c => c.Guid)],
            };

            if (playlistElement.Tracks.Count == 0)
                playlistElement.Tracks = null;

            if (playlistElement.Categories.Count == 0)
                playlistElement.Categories = null;

            playlistElements.Add(playlistElement);
        }

        return playlistElements.Count == 0 ? null : playlistElements;
    }

    private static List<CategoryElement>? BuildCategories(CategoriesScreenshot categoriesScreenshot)
    {
        var categoryElements = new List<CategoryElement>(categoriesScreenshot.Categories.Count);

        foreach (var category in categoriesScreenshot.Categories)
        {
            var categoryElement = new CategoryElement()
            {
                Name = category.Name,
                Guid = category.Guid,
            };

            categoryElements.Add(categoryElement);
        }

        return categoryElements.Count == 0 ? null : categoryElements;
    }

}
