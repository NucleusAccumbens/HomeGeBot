namespace Parser.Exceptions;

public class NoResponseException : Exception
{
    public NoResponseException()
        : base("No response received from the server.")
    {
    }

    public NoResponseException(string message)
        : base(message)
    {
    }

    public NoResponseException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

