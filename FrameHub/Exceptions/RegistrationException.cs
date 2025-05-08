using System.Net;

namespace FrameHub.Exceptions;

public class RegistrationException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}