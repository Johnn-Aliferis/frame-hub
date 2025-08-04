using System.Net;
using FrameHub.Modules.Shared.Application.Exception;

namespace FrameHub.Modules.Subscriptions.Application.Exception;

public class StripeConsumerException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}