using System.Net;

namespace FrameHub.Exceptions;

public class ValidationException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}