using Sunrise.Model.Common;
using Sunrise.Model.Communication;
using Sunrise.Model.Model.Exchange;

namespace Sunrise.Model.Model;

public static class MediaExporter
{
    public static async Task ExportAsync(Player player, Stream stream, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(stream);

        var devices = await player.GetDevicesAsync(token);
        var folders = await player.GetFoldersAsync(token);
        folders.Add(player.TracksPath);

        var tracksScreenshot = await player.GetTracksAsync(token);
        var playlists = await player.GetPlaylistsAsync(token);
        var categoriesScreenshot = await player.GetCategoriesAsync(token);
        var document = BuildDocument(devices, folders, tracksScreenshot, playlists, categoriesScreenshot);

        JsonSerialization.Serialize(stream, document);
    }

    internal static MediaDocument? Deserialize(Stream stream)
        => JsonSerialization.Deserialize<MediaDocument>(stream);

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
                Updated = track.Updated,
                Bitrate = track.Bitrate,
                Size = track.Size,
                LastWrite = track.LastWrite,
                HasPicture = track.HasPicture,
                Lyrics = track.Lyrics ?? string.Empty,
                Translate = track.Translate ?? string.Empty,
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
                Updated = playlist.Updated,
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
