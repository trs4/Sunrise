namespace Sunrise.Model.TagLib;

public abstract class Tag
{
    public abstract TagTypes TagTypes { get; }

    public virtual string? Title
    {
        get => null;
        set { }
    }

    public virtual string? TitleSort
    {
        get => null;
        set { }
    }

    public virtual string? Subtitle
    {
        get => null;
        set { }
    }

    public virtual string? Description
    {
        get => null;
        set { }
    }

    public virtual string?[] Performers
    {
        get => [];
        set { }
    }

    public virtual string?[] PerformersSort
    {
        get => [];
        set { }
    }

    public virtual string[] PerformersRole
    {
        get => [];
        set { }
    }

    public virtual string?[] AlbumArtists
    {
        get => [];
        set { }
    }

    public virtual string?[] AlbumArtistsSort
    {
        get => [];
        set { }
    }

    public virtual string?[] Composers
    {
        get => [];
        set { }
    }

    public virtual string?[] ComposersSort
    {
        get => [];
        set { }
    }

    public virtual string? Album
    {
        get => null;
        set { }
    }

    public virtual string? AlbumSort
    {
        get => null;
        set { }
    }

    public virtual string? Comment
    {
        get => null;
        set { }
    }

    public virtual string?[] Genres
    {
        get => [];
        set { }
    }

    public virtual uint Year
    {
        get => 0;
        set { }
    }

    public virtual uint Track
    {
        get => 0;
        set { }
    }

    public virtual uint TrackCount
    {
        get => 0;
        set { }
    }

    public virtual uint Disc
    {
        get => 0;
        set { }
    }

    public virtual uint DiscCount
    {
        get => 0;
        set { }
    }

    public virtual string? Lyrics
    {
        get => null;
        set { }
    }

    public virtual string? Grouping
    {
        get => null;
        set { }
    }

    public virtual uint BeatsPerMinute
    {
        get => 0;
        set { }
    }

    public virtual string? Conductor
    {
        get => null;
        set { }
    }

    public virtual string? Copyright
    {
        get => null;
        set { }
    }

    public virtual DateTime? DateTagged
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzArtistId
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzReleaseGroupId
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzReleaseId
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzReleaseArtistId
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzTrackId
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzDiscId
    {
        get => null;
        set { }
    }

    public virtual string? MusicIpId
    {
        get => null;
        set { }
    }

    public virtual string? AmazonId
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzReleaseStatus
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzReleaseType
    {
        get => null;
        set { }
    }

    public virtual string? MusicBrainzReleaseCountry
    {
        get => null;
        set { }
    }

    public virtual double ReplayGainTrackGain
    {
        get => double.NaN;
        set { }
    }

    public virtual double ReplayGainTrackPeak
    {
        get => double.NaN;
        set { }
    }

    public virtual double ReplayGainAlbumGain
    {
        get => double.NaN;
        set { }
    }

    public virtual double ReplayGainAlbumPeak
    {
        get => double.NaN;
        set { }
    }

    public virtual string? InitialKey
    {
        get => null;
        set { }
    }

    public virtual string? RemixedBy
    {
        get => null;
        set { }
    }

    public virtual string? Publisher
    {
        get => null;
        set { }
    }

    public virtual string? ISRC
    {
        get => null;
        set { }
    }

    public virtual string? Length
    {
        get => null;
        set { }
    }

    public virtual IPicture[] Pictures
    {
        get => [];
        set { }
    }

    public string? FirstAlbumArtist => FirstInGroup(AlbumArtists);

    public string? FirstAlbumArtistSort => FirstInGroup(AlbumArtistsSort);

    public string? FirstPerformer => FirstInGroup(Performers);

    public string? FirstPerformerSort => FirstInGroup(PerformersSort);

    public string? FirstComposerSort => FirstInGroup(ComposersSort);

    public string? FirstComposer => FirstInGroup(Composers);

    public string? FirstGenre => FirstInGroup(Genres);

    public string? JoinedAlbumArtists => JoinGroup(AlbumArtists);

    public string? JoinedPerformers => JoinGroup(Performers);

    public string? JoinedPerformersSort => JoinGroup(PerformersSort);

    public string? JoinedComposers => JoinGroup(Composers);

    public string? JoinedGenres => JoinGroup(Genres);

    private static string? FirstInGroup(string?[] group) => group is null || group.Length == 0 ? null : group[0];

    private static string? JoinGroup(string?[] group) => group is null || group.Length == 0 ? null : string.Join("; ", group);

    public virtual bool IsEmpty
        => string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(Grouping) && IsNullOrLikeEmpty(AlbumArtists) && IsNullOrLikeEmpty(Performers)
        && IsNullOrLikeEmpty(Composers) && string.IsNullOrWhiteSpace(Conductor) && string.IsNullOrWhiteSpace(Copyright) && string.IsNullOrWhiteSpace(Album)
        && string.IsNullOrWhiteSpace(Comment) && IsNullOrLikeEmpty(Genres) && Year == 0 && BeatsPerMinute == 0 && Track == 0 && TrackCount == 0
        && Disc == 0 && DiscCount == 0;

    public abstract void Clear();

    public void SetInfoTag()
        => DateTagged = DateTime.Now;

    public virtual void CopyTo(Tag target, bool overwrite)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (overwrite || string.IsNullOrWhiteSpace(target.Title))
            target.Title = Title;

        if (overwrite || string.IsNullOrWhiteSpace(target.Subtitle))
            target.Subtitle = Subtitle;

        if (overwrite || string.IsNullOrWhiteSpace(target.Description))
            target.Description = Description;

        if (overwrite || IsNullOrLikeEmpty(target.AlbumArtists))
            target.AlbumArtists = AlbumArtists;

        if (overwrite || IsNullOrLikeEmpty(target.Performers))
            target.Performers = Performers;

        if (overwrite || IsNullOrLikeEmpty(target.PerformersRole))
            target.PerformersRole = PerformersRole;

        if (overwrite || IsNullOrLikeEmpty(target.Composers))
            target.Composers = Composers;

        if (overwrite || string.IsNullOrWhiteSpace(target.Album))
            target.Album = Album;

        if (overwrite || string.IsNullOrWhiteSpace(target.Comment))
            target.Comment = Comment;

        if (overwrite || IsNullOrLikeEmpty(target.Genres))
            target.Genres = Genres;

        if (overwrite || target.Year == 0)
            target.Year = Year;

        if (overwrite || target.Track == 0)
            target.Track = Track;

        if (overwrite || target.TrackCount == 0)
            target.TrackCount = TrackCount;

        if (overwrite || target.Disc == 0)
            target.Disc = Disc;

        if (overwrite || target.DiscCount == 0)
            target.DiscCount = DiscCount;

        if (overwrite || target.BeatsPerMinute == 0)
            target.BeatsPerMinute = BeatsPerMinute;

        if (overwrite || string.IsNullOrWhiteSpace(target.InitialKey))
            target.InitialKey = InitialKey;

        if (overwrite || string.IsNullOrWhiteSpace(target.Publisher))
            target.Publisher = Publisher;

        if (overwrite || string.IsNullOrWhiteSpace(target.ISRC))
            target.ISRC = ISRC;

        if (overwrite || string.IsNullOrWhiteSpace(target.RemixedBy))
            target.RemixedBy = RemixedBy;

        if (overwrite || string.IsNullOrWhiteSpace(target.Grouping))
            target.Grouping = Grouping;

        if (overwrite || string.IsNullOrWhiteSpace(target.Conductor))
            target.Conductor = Conductor;

        if (overwrite || string.IsNullOrWhiteSpace(target.Copyright))
            target.Copyright = Copyright;

        if (overwrite || target.DateTagged is null)
            target.DateTagged = DateTagged;

        if (overwrite || target.Pictures is null || target.Pictures.Length == 0)
            target.Pictures = Pictures;
    }

    private static bool IsNullOrLikeEmpty(string?[] value)
    {
        if (value is null)
            return true;

        foreach (string s in value)
        {
            if (!string.IsNullOrWhiteSpace(s))
                return false;
        }

        return true;
    }

}
