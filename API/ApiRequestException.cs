using System.Net;

namespace SharpAutomation.API;

public sealed class ApiRequestException : Exception
{
    public ApiRequestException(
        string message,
        HttpStatusCode statusCode,
        string? responseBody,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string? ResponseBody { get; }
}
