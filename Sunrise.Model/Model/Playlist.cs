using System.Text.Json;
using Sunrise.Model.Common;

namespace Sunrise.Model;

/// <summary>Список воспроизведения</summary>
public class Playlist
{
    private PlaylistCalculatedData? _calculatedData;
    private bool _tracksInitialized;

    /// <summary>Идентификатор</summary>
    public int Id { get; set; }

    /// <summary>Уникальный идентификатор</summary>
    public Guid Guid { get; set; }

    /// <summary>Наименование</summary>
    public string Name { get; set; }

    /// <summary>Создано</summary>
    public DateTime Created { get; set; }

    /// <summary>Обновлено</summary>
    public DateTime Updated { get; set; }

    /// <summary>Данные вычисляемого списка</summary>
    public byte[] CalculatedData { get; set; }

    /// <summary>Треки</summary>
    public List<Track> Tracks { get; set; }

    /// <summary>Категории</summary>
    public List<Category> Categories { get; set; }

    public void SetCalculatedData(PlaylistCalculatedData calculatedData)
    {
        _calculatedData = calculatedData ?? throw new ArgumentNullException(nameof(calculatedData));
        CalculatedData = JsonSerialization.Serialize(calculatedData);
    }

    public async Task<List<Track>> GetTracksAsync(Player player, CancellationToken token = default)
    {
        if (CalculatedData is not null)
        {
            _calculatedData ??= Deserialize();

            if (!_tracksInitialized)
            {
                _tracksInitialized = true;
                Tracks = await player.GetTracksAsync(_calculatedData, token);
            }
        }

        return Tracks;
    }

    private PlaylistCalculatedData? Deserialize()
    {
        var calculatedData = JsonSerialization.Deserialize<PlaylistCalculatedData>(CalculatedData);

        if (calculatedData is null)
            return null;

        if (calculatedData.TermRules is not null)
        {
            foreach (var rule in calculatedData.TermRules)
            {
                if (rule.Value is JsonElement jsonElement)
                    rule.Value = jsonElement.GetString();
            }
        }

        return calculatedData;
    }

    public void ReloadTracks()
        => _tracksInitialized = false;

    public override string ToString() => Name;
}
