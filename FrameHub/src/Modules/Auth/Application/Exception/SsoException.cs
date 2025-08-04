using System.Net;
using FrameHub.Modules.Shared.Application.Exception;

namespace FrameHub.Modules.Auth.Application.Exception;

public class SsoException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}