using System;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Sunrise.Model;
using Track = Sunrise.Model.Track;

namespace Sunrise.ViewModels;

public class TrackViewModel : ObservableObject
{
    private bool? _isPlaying;
    private bool _picked;
    private object? _icon;
    private string? _title;
    private int? _year;
    private TimeSpan _duration;
    private byte _rating;
    private string? _artist;
    private string? _genre;
    private DateTime? _lastPlay;
    private int _reproduced;
    private string? _album;
    private DateTime _created;
    private DateTime _added;
    private DateTime _updated;
    private double _bitrate;
    private long _size;
    private string? _extension;

    public TrackViewModel(Track track, Player player)
    {
        Track = track ?? throw new ArgumentNullException(nameof(track));
        Player = player ?? throw new ArgumentNullException(nameof(player));
        _picked = track.Picked;
        _title = track.Title;
        _year = track.Year;
        _duration = track.Duration;
        _rating = track.Rating;
        _artist = track.Artist;
        _genre = track.Genre;
        _reproduced = track.Reproduced;
        _album = track.Album;
        _created = track.Created;
        _added = track.Added;
        _updated = track.Updated;
        _bitrate = track.Bitrate;
        _size = track.Size;
    }

    public Track Track { get; }

    public Player Player { get; }

    /// <summary>Воспроизводится</summary>
    public bool? IsPlaying
    {
        get => _isPlaying;
        set => SetProperty(ref _isPlaying, value);
    }

    /// <summary>Выбран</summary>
    public bool Picked
    {
        get => _picked;
        set => SetProperty(ref _picked, value);
    }

    /// <summary>Иконка</summary>
    public object? Icon
    {
        get
        {
            if (_icon is null && Track.HasPicture)
                TrackIconHelper.SetPicture(Player, Track, icon => Icon = icon);

            return _icon;
        }
        set => SetProperty(ref _icon, value);
    }

    /// <summary>Название</summary>
    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>Год</summary>
    public int? Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    /// <summary>Длительность</summary>
    public TimeSpan Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    /// <summary>Рейтинг</summary>
    public byte Rating
    {
        get => _rating;
        set => SetProperty(ref _rating, value);
    }

    /// <summary>Артист</summary>
    public string? Artist
    {
        get => _artist;
        set => SetProperty(ref _artist, value);
    }

    /// <summary>Жанр</summary>
    public string? Genre
    {
        get => _genre;
        set => SetProperty(ref _genre, value);
    }

    /// <summary>Последнее воспроизведение</summary>
    public DateTime? LastPlay
    {
        get => _lastPlay;
        set => SetProperty(ref _lastPlay, value);
    }

    /// <summary>Воспроизведено</summary>
    public int Reproduced
    {
        get => _reproduced;
        set => SetProperty(ref _reproduced, value);
    }

    /// <summary>Альбом</summary>
    public string? Album
    {
        get => _album;
        set => SetProperty(ref _album, value);
    }

    /// <summary>Создано</summary>
    public DateTime Created
    {
        get => _created;
        set => SetProperty(ref _created, value);
    }

    /// <summary>Добавлено</summary>
    public DateTime Added
    {
        get => _added;
        set => SetProperty(ref _added, value);
    }

    /// <summary>Обновлено</summary>
    public DateTime Updated
    {
        get => _updated;
        set => SetProperty(ref _updated, value);
    }

    /// <summary>Битрейт</summary>
    public double Bitrate
    {
        get => _bitrate;
        set => SetProperty(ref _bitrate, value);
    }

    /// <summary>Размер</summary>
    public long Size
    {
        get => _size;
        set => SetProperty(ref _size, value);
    }

    /// <summary>Расширение</summary>
    public string Extension
    {
        get => _extension ??= Path.GetExtension(Track.Path).Substring(1);
        set => SetProperty(ref _extension, value);
    }

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Picked))
            await Player.ChangePickedAsync(Track, _picked);
        else if (e.PropertyName == nameof(Rating))
            await Player.ChangeRatingAsync(Track, _rating);
    }

    public override string ToString() => $"{_artist} - {_title}";
}
