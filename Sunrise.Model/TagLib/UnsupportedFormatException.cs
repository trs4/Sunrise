namespace Sunrise.Model.TagLib;

[Serializable]
public class UnsupportedFormatException : Exception
{
    public UnsupportedFormatException(string message) : base(message) { }

    public UnsupportedFormatException() { }

    public UnsupportedFormatException(string message, Exception innerException) : base(message, innerException) { }
}
