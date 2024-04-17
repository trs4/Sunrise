using System.Text;

namespace Sunrise.Model.TagLib.Id3v2;

public class UserUrlLinkFrame : UrlLinkFrame
{
    public UserUrlLinkFrame(string? description, StringType encoding)
        : base(FrameType.WXXX)
        => base.Text = [description];

    public UserUrlLinkFrame(string description)
        : base(FrameType.WXXX)
        => base.Text = new[] { description };

    public UserUrlLinkFrame(ByteVector data, byte version) : base(data, version) { }

    protected internal UserUrlLinkFrame(ByteVector data, int offset, FrameHeader header, byte version) : base(data, offset, header, version) { }

    public string? Description
    {
        get
        {
            string?[] text = base.Text;
            return text.Length > 0 ? text[0] : null;
        }
        set
        {
            string?[] text = base.Text;

            if (text.Length > 0)
                text[0] = value;
            else
                text = [value];

            base.Text = text;
        }
    }

    public override string?[] Text
    {
        get
        {
            string?[] text = base.Text;

            if (text.Length < 2)
                return [];

            string?[] new_text = new string?[text.Length - 1];

            for (int i = 0; i < new_text.Length; i++)
                new_text[i] = text[i + 1];

            return new_text;
        }
        set
        {
            string?[] new_value = new string?[value?.Length + 1 ?? 1];
            new_value[0] = Description;

            if (value is not null)
            {
                for (int i = 1; i < new_value.Length; i++)
                    new_value[i] = value[i - 1];
            }

            base.Text = new_value;
        }
    }

    public override string ToString() => new StringBuilder().Append('[').Append(Description).Append("] ").Append(base.ToString()).ToString();

    public static UserUrlLinkFrame? Get(Tag tag, string description, StringType type, bool create)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(description);

        if (description.Length == 0)
            throw new ArgumentException("Description must not be empty", nameof(description));

        foreach (var frame in tag.GetFrames<UserUrlLinkFrame>(FrameType.WXXX))
        {
            if (description.Equals(frame.Description))
                return frame;
        }

        if (!create)
            return null;

        var new_frame = new UserUrlLinkFrame(description, type);
        tag.AddFrame(new_frame);
        return new_frame;
    }

    public static UserUrlLinkFrame? Get(Tag tag, string description, bool create) => Get(tag, description, Tag.DefaultEncoding, create);
}
