using System.Net;

namespace FrameHub.Exceptions;

public class GeneralException(string message, HttpStatusCode status) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}
