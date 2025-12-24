using Sunrise.Model.SoundFlow.Metadata.Abstracts;
using Sunrise.Model.SoundFlow.Metadata.Models;
using Sunrise.Model.SoundFlow.Metadata.Readers.Tags;
using Sunrise.Model.SoundFlow.Metadata.Writers.Tags;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Metadata.Writers.Format;

internal class Mp3Writer : ISoundFormatWriter
{
    public async Task<Result> RemoveTagsAsync(string sourcePath, string destinationPath)
    {
        try
        {
            await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            var audioOffsetResult = await GetAudioDataOffsetAsync(sourceStream);
            if (audioOffsetResult.IsFailure) return audioOffsetResult;
            var audioDataOffset = audioOffsetResult.Value;
            
            await using var destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            sourceStream.Position = audioDataOffset;
            await sourceStream.CopyToAsync(destStream);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return new Error("An unexpected error occurred while removing MP3 tags.", ex);
        }
    }

    public async Task<Result> WriteTagsAsync(string sourcePath, string destinationPath, SoundTags tags)
    {
        try
        {
            await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            var audioOffsetResult = await GetAudioDataOffsetAsync(sourceStream);
            if (audioOffsetResult.IsFailure) return audioOffsetResult;
            var audioDataOffset = audioOffsetResult.Value;
            
            var newTagData = Id3V2Builder.Build(tags);

            await using var destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            await destStream.WriteAsync(newTagData);
            
            sourceStream.Position = audioDataOffset;
            await sourceStream.CopyToAsync(destStream);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return new Error("An unexpected error occurred while writing MP3 tags.", ex);
        }
    }

    /// <summary>
    /// Finds the start of the audio data in an MP3 file by skipping over any ID3v2 tags.
    /// </summary>
    private static async Task<Result<long>> GetAudioDataOffsetAsync(Stream stream)
    {
        stream.Position = 0;

        var options = new ReadOptions { ReadTags = true, ReadAlbumArt = false };

        var readResult = await Id3V2Reader.ReadAsync(stream, options);

        if (readResult.IsFailure)
            return Result<long>.Fail(readResult.Error!);
        
        var (_, tagSize) = readResult.Value;
        return tagSize;
    }
}