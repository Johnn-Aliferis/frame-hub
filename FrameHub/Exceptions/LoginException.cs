using System.Net;

namespace FrameHub.Exceptions;

public class LoginException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}