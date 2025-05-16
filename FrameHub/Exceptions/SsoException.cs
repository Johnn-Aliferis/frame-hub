using System.Net;

namespace FrameHub.Exceptions;

public class SsoException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}