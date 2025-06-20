using System.Net;

namespace FrameHub.Exceptions;

public class MediaException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}