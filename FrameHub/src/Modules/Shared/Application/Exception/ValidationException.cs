using System.Net;

namespace FrameHub.Modules.Shared.Application.Exception;

public class ValidationException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}