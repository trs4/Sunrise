namespace Sunrise.Model.TagLib.Id3v2;

public class RelativeVolumeFrame : Frame
{
    private readonly ChannelData[] _channels = new ChannelData[9];

    public RelativeVolumeFrame(string identification)
        : base(FrameType.RVA2, 4)
        => Identification = identification;

    public RelativeVolumeFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal RelativeVolumeFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public string Identification { get; private set; }

    public ChannelType[] Channels
    {
        get
        {
            var types = new List<ChannelType>();

            for (int i = 0; i < 9; i++)
            {
                if (_channels[i].IsSet)
                    types.Add((ChannelType)i);
            }

            return [.. types];
        }
    }

    public override string ToString() => Identification;

    public short GetVolumeAdjustmentIndex(ChannelType type) => _channels[(int)type].VolumeAdjustmentIndex;

    public void SetVolumeAdjustmentIndex(ChannelType type, short index) => _channels[(int)type].VolumeAdjustmentIndex = index;

    public float GetVolumeAdjustment(ChannelType type) => _channels[(int)type].VolumeAdjustment;

    public void SetVolumeAdjustment(ChannelType type, float adjustment) => _channels[(int)type].VolumeAdjustment = adjustment;

    public ulong GetPeakVolumeIndex(ChannelType type) => _channels[(int)type].PeakVolumeIndex;

    public void SetPeakVolumeIndex(ChannelType type, ulong index) => _channels[(int)type].PeakVolumeIndex = index;

    public double GetPeakVolume(ChannelType type) => _channels[(int)type].PeakVolume;

    public void SetPeakVolume(ChannelType type, double peak) => _channels[(int)type].PeakVolume = peak;

    public static RelativeVolumeFrame? Get(Tag tag, string identification, bool create)
    {
        RelativeVolumeFrame rva2;

        foreach (Frame frame in tag.GetFrames(FrameType.RVA2))
        {
            rva2 = frame as RelativeVolumeFrame;

            if (rva2 is null)
                continue;

            if (rva2.Identification != identification)
                continue;

            return rva2;
        }

        if (!create)
            return null;

        rva2 = new RelativeVolumeFrame(identification);
        tag.AddFrame(rva2);
        return rva2;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        int pos = data.Find(ByteVector.TextDelimiter(StringType.Latin1));

        if (pos < 0)
            return;

        Identification = data.ToString(StringType.Latin1, 0, pos++);

        while (pos <= data.Count - 4)
        {
            int type = data[pos++];

            unchecked
            {
                _channels[type].VolumeAdjustmentIndex = (short)data.Mid(pos, 2).ToUShort();
            }

            pos += 2;
            int bytes = BitsToBytes(data[pos++]);

            if (data.Count < pos + bytes)
                break;

            _channels[type].PeakVolumeIndex = data.Mid(pos, bytes).ToULong();
            pos += bytes;
        }
    }

    protected override ByteVector RenderFields(byte version)
    {
        var data = new ByteVector
        {
            ByteVector.FromString(Identification, StringType.Latin1),
            ByteVector.TextDelimiter(StringType.Latin1),
        };

        for (byte i = 0; i < 9; i++)
        {
            if (!_channels[i].IsSet)
                continue;

            data.Add(i);

            unchecked
            {
                data.Add(ByteVector.FromUShort((ushort)_channels[i].VolumeAdjustmentIndex));
            }

            byte bits = 0;

            for (byte j = 0; j < 64; j++)
            {
                if ((_channels[i].PeakVolumeIndex & (1UL << j)) != 0)
                    bits = (byte)(j + 1);
            }

            data.Add(bits);

            if (bits > 0)
                data.Add(ByteVector.FromULong(_channels[i].PeakVolumeIndex).Mid(8 - BitsToBytes(bits)));
        }

        return data;
    }

    public override Frame Clone()
    {
        var frame = new RelativeVolumeFrame(Identification);

        for (int i = 0; i < 9; i++)
            frame._channels[i] = _channels[i];

        return frame;
    }

    private static int BitsToBytes(int i) => i % 8 == 0 ? i / 8 : (i - i % 8) / 8 + 1;

    private struct ChannelData
    {
        public short VolumeAdjustmentIndex;
        public ulong PeakVolumeIndex;

        public readonly bool IsSet => VolumeAdjustmentIndex != 0 || PeakVolumeIndex != 0;

        public float VolumeAdjustment
        {
            readonly get => VolumeAdjustmentIndex / 512f;
            set => VolumeAdjustmentIndex = (short)(value * 512f);
        }

        public double PeakVolume
        {
            readonly get => PeakVolumeIndex / 512.0;
            set => PeakVolumeIndex = (ulong)(value * 512.0);
        }
    }

}
