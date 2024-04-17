namespace Sunrise.Model.TagLib;

[Serializable]
public class CorruptFileException : Exception
{
    public CorruptFileException() { }

    public CorruptFileException(string message) : base(message) { }

    public CorruptFileException(string message, Exception innerException) : base(message, innerException) { }
}
