using Sunrise.Model.SoundFlow.Metadata.Models;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Metadata.Abstracts;

/// <summary>Base class for format readers to reduce sync/async code duplication</summary>
internal abstract class SoundFormatReader
{
    public abstract Task<Result<SoundFormatInfo>> ReadAsync(Stream stream, ReadOptions options);
}