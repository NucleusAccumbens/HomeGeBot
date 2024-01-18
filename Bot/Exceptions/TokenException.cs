namespace Bot.Exceptions;

public class TokenException : Exception
{
    public TokenException() : base() { }

    public TokenException(string message) : base(message) { }
}
