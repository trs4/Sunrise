namespace Sunrise.Model.TagLib;

public class CombinedTag : Tag
{
    private readonly List<Tag> _tags;

    public CombinedTag()
        => _tags = [];

    public CombinedTag(params Tag[] tags)
        => _tags = new List<Tag>(tags);

    public virtual Tag[] Tags => _tags.ToArray();

    public void SetTags(params Tag[] tags)
    {
        _tags.Clear();
        _tags.AddRange(tags);
    }

    protected void InsertTag(int index, Tag tag) => _tags.Insert(index, tag);

    protected void AddTag(Tag tag) => _tags.Add(tag);

    protected void RemoveTag(Tag tag) => _tags.Remove(tag);

    protected void ClearTags() => _tags.Clear();

    public override TagTypes TagTypes
    {
        get
        {
            var types = TagTypes.None;

            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    types |= tag.TagTypes;
            }

            return types;
        }
    }

    public override string? Title
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Title;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Title = value;
            }
        }
    }

    public override string? Subtitle
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Subtitle;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Subtitle = value;
            }
        }
    }

    public override string? Description
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Description;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Description = value;
            }
        }
    }

    public override string?[] Performers
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string?[] value = tag.Performers;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Performers = value;
            }
        }
    }

    public override string?[] PerformersSort
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                var value = tag.PerformersSort;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.PerformersSort = value;
            }
        }
    }

    public override string[] PerformersRole
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string[] value = tag.PerformersRole;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.PerformersRole = value;
            }
        }
    }

    public override string?[] AlbumArtistsSort
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                var value = tag.AlbumArtistsSort;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.AlbumArtistsSort = value;
            }
        }
    }

    public override string?[] AlbumArtists
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                var value = tag.AlbumArtists;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.AlbumArtists = value;
            }
        }
    }

    public override string?[] Composers
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                var value = tag.Composers;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Composers = value;
            }
        }
    }

    public override string?[] ComposersSort
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                var value = tag.ComposersSort;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.ComposersSort = value;
            }
        }
    }

    public override string? TitleSort
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.TitleSort;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.TitleSort = value;
            }
        }
    }

    public override string? AlbumSort
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.AlbumSort;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.AlbumSort = value;
            }
        }
    }

    public override string? Album
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Album;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Album = value;
            }
        }
    }

    public override string? Comment
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Comment;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Comment = value;
            }
        }
    }

    public override string?[] Genres
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                var value = tag.Genres;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return [];
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Genres = value;
            }
        }
    }

    public override uint Year
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                uint value = tag.Year;

                if (value != 0)
                    return value;
            }

            return 0;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Year = value;
            }
        }
    }

    public override uint Track
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                uint value = tag.Track;

                if (value != 0)
                    return value;
            }

            return 0;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Track = value;
            }
        }
    }

    public override uint TrackCount
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                uint value = tag.TrackCount;

                if (value != 0)
                    return value;
            }

            return 0;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.TrackCount = value;
            }
        }
    }

    public override uint Disc
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                uint value = tag.Disc;

                if (value != 0)
                    return value;
            }

            return 0;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Disc = value;
            }
        }
    }

    public override uint DiscCount
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                uint value = tag.DiscCount;

                if (value != 0)
                    return value;
            }

            return 0;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.DiscCount = value;
            }
        }
    }

    public override string? Lyrics
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Lyrics;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Lyrics = value;
            }
        }
    }

    public override string? Grouping
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Grouping;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Grouping = value;
            }
        }
    }

    public override uint BeatsPerMinute
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                uint value = tag.BeatsPerMinute;

                if (value != 0)
                    return value;
            }

            return 0;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.BeatsPerMinute = value;
            }
        }
    }

    public override string? Conductor
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Conductor;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Conductor = value;
            }
        }
    }

    public override string? Copyright
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Copyright;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Copyright = value;
            }
        }
    }

    public override DateTime? DateTagged
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                DateTime? value = tag.DateTagged;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.DateTagged = value;
            }
        }
    }

    public override string? MusicBrainzArtistId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzArtistId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzArtistId = value;
            }
        }
    }

    public override string? MusicBrainzReleaseGroupId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzReleaseGroupId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzReleaseGroupId = value;
            }
        }
    }

    public override string? MusicBrainzReleaseId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzReleaseId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzReleaseId = value;
            }
        }
    }

    public override string? MusicBrainzReleaseArtistId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzReleaseArtistId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzReleaseArtistId = value;
            }
        }
    }

    public override string? MusicBrainzTrackId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzTrackId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzTrackId = value;
            }
        }
    }

    public override string? MusicBrainzDiscId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzDiscId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzDiscId = value;
            }
        }
    }

    public override string? MusicIpId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicIpId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicIpId = value;
            }
        }
    }

    public override string? AmazonId
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.AmazonId;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.AmazonId = value;
            }
        }
    }

    public override string? MusicBrainzReleaseStatus
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzReleaseStatus;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzReleaseStatus = value;
            }
        }
    }

    public override string? MusicBrainzReleaseType
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzReleaseType;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzReleaseType = value;
            }
        }
    }

    public override string? MusicBrainzReleaseCountry
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.MusicBrainzReleaseCountry;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.MusicBrainzReleaseCountry = value;
            }
        }
    }

    public override IPicture[] Pictures
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                IPicture[] value = tag.Pictures;

                if (value is not null && value.Length > 0)
                    return value;
            }

            return base.Pictures;
        }

        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Pictures = value;
            }
        }
    }

    public override double ReplayGainTrackGain
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                double value = tag.ReplayGainTrackGain;

                if (!double.IsNaN(value))
                    return value;
            }

            return double.NaN;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.ReplayGainTrackGain = value;
            }
        }
    }

    public override double ReplayGainTrackPeak
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                double value = tag.ReplayGainTrackPeak;

                if (!double.IsNaN(value))
                    return value;
            }

            return double.NaN;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.ReplayGainTrackPeak = value;
            }
        }
    }

    public override double ReplayGainAlbumGain
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                double value = tag.ReplayGainAlbumGain;

                if (!double.IsNaN(value))
                    return value;
            }

            return double.NaN;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.ReplayGainAlbumGain = value;
            }
        }
    }

    public override double ReplayGainAlbumPeak
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                double value = tag.ReplayGainAlbumPeak;

                if (!double.IsNaN(value))
                    return value;
            }

            return double.NaN;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.ReplayGainAlbumPeak = value;
            }
        }
    }

    public override string? InitialKey
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.InitialKey;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.InitialKey = value;
            }
        }
    }

    public override string? RemixedBy
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.RemixedBy;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.RemixedBy = value;
            }
        }
    }

    public override string? Publisher
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Publisher;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Publisher = value;
            }
        }
    }

    public override string? ISRC
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.ISRC;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.ISRC = value;
            }
        }
    }

    public override string? Length
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag is null)
                    continue;

                string value = tag.Length;

                if (value is not null)
                    return value;
            }

            return null;
        }
        set
        {
            foreach (Tag tag in _tags)
            {
                if (tag is not null)
                    tag.Length = value;
            }
        }
    }

    public override bool IsEmpty
    {
        get
        {
            foreach (Tag tag in _tags)
            {
                if (tag.IsEmpty)
                    return true;
            }

            return false;
        }
    }

    public override void Clear()
    {
        foreach (Tag tag in _tags)
            tag.Clear();
    }

}
