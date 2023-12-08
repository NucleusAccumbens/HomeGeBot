using System.Net;

namespace Parser.Exceptions;

public class BadResponseException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ReasonPhrase { get; }
    public HttpRequestMessage RequestMessage { get; }

    public BadResponseException(HttpResponseMessage response)
        : base($"Error occurred while requesting URL. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}")
    {
        StatusCode = response.StatusCode;
        ReasonPhrase = response.ReasonPhrase;
        RequestMessage = response.RequestMessage;
    }
}

