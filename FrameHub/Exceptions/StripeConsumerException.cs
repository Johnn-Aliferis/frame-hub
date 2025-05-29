using System.Net;

namespace FrameHub.Exceptions;

public class StripeConsumerException(string message, HttpStatusCode status) : GeneralException(message, status)
{
}