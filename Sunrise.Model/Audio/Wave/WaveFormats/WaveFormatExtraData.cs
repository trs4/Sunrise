using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Wave;

/// <summary>This class used for marshalling from unmanaged code</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
public class WaveFormatExtraData : WaveFormat
{
    // try with 100 bytes for now, increase if necessary
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    private readonly byte[] _extraData = new byte[100];

    /// <summary>Allows the extra data to be read</summary>
    public byte[] ExtraData => _extraData;

    /// <summary>Parameterless constructor for marshalling</summary>
    internal WaveFormatExtraData() { }

    /// <summary>Reads this structure from a BinaryReader</summary>
    public WaveFormatExtraData(BinaryReader reader)
        : base(reader)
        => ReadExtraData(reader);

    internal void ReadExtraData(BinaryReader reader)
    {
        if (_extraSize > 0)
            reader.Read(_extraData, 0, _extraSize);
    }

    /// <summary>Writes this structure to a BinaryWriter</summary>
    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);

        if (_extraSize > 0)
            writer.Write(_extraData, 0, _extraSize);
    }

}