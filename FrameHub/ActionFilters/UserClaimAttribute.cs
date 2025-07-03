using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FrameHub.ActionFilters;

public class UserClaimAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = user.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            context.Result = new UnauthorizedObjectResult("User claims missing or invalid.");
            return;
        }
        context.HttpContext.Items["UserId"] = userId;
        context.HttpContext.Items["Email"] = email;
    }
}