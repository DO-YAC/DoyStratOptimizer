using System.Net;

namespace DoyStratOptimizer_Common.Models.Exceptions;

public class DoyStratOptimizerException : Exception
{
    public DoyStratOptimizerException(
        string message,
        HttpStatusCode statusCode) 
        : base(message)
    {
        StatusCode = statusCode;
    }
    public HttpStatusCode StatusCode { get; }
}
