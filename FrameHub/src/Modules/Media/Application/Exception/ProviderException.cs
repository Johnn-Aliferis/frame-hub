using System.Net;
using FrameHub.Modules.Shared.Application.Exception;

namespace FrameHub.Modules.Media.Application.Exception;

public class ProviderException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}