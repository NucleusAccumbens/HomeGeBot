namespace Bot.Exceptions;

public class UrlException : Exception
{
    public UrlException() : base() { }
    
    public UrlException(string message) : base(message) { }
}
