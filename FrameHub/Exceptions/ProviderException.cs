using System.Net;

namespace FrameHub.Exceptions;

public class ProviderException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}