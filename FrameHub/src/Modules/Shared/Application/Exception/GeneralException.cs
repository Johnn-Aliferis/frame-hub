using System.Net;

namespace FrameHub.Modules.Shared.Application.Exception;

public class GeneralException(string message, HttpStatusCode status) : System.Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}
