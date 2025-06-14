using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Sunrise.Model;

namespace Sunrise.ViewModels;

internal static class TrackIconHelper
{
    public static void SetPicture(Player player, Track track, Action<object?> setTrackIcon)
    {
        if (!track.HasPicture)
            return;

        var icon = track.PictureIcon;

        if (icon is not null)
        {
            setTrackIcon(icon);
            return;
        }

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var picture = await player.LoadPictureAsync(track);

            if (picture is null)
                track.HasPicture = false;
            else
            {
                try
                {
                    using var bitmapStream = new MemoryStream(picture.Data);
                    bitmapStream.Seek(0, SeekOrigin.Begin);
                    var bitmap = new Bitmap(bitmapStream);
                    track.PictureIcon = bitmap; // %%TODO Convert to 48 x 48
                    setTrackIcon(bitmap);
                }
                catch
                {
                    track.HasPicture = false;
                }
            }
        }, DispatcherPriority.Normal);
    }

}
