using System.Security.Claims;
using FrameHub.Modules.Shared.API.ActionFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace FrameHub.Tests.Modules.Shared.API.ActionFilters;

public class UserClaimsAttributeTests
{
    [Fact]
    public void OnActionExecuting_Success()
    {
        const string userId = "test-user-id";
        const string email = "user@example.com";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };
        
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),                         
            new ActionDescriptor(),                  
            new ModelStateDictionary()               
        );

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            controller: null);

        var attribute = new UserClaimAttribute();
        
        attribute.OnActionExecuting(context);

        // Assert
        Assert.Null(context.Result);
        Assert.Equal(userId, httpContext.Items["UserId"]);
        Assert.Equal(email, httpContext.Items["Email"]);
    }
    
    [Fact]
    public void OnActionExecuting_Failure()
    {
        const string userId = "";
        const string email = "";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };
        
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),                         
            new ActionDescriptor(),                  
            new ModelStateDictionary()               
        );

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            controller: null);

        var attribute = new UserClaimAttribute();
        
        attribute.OnActionExecuting(context);

        Assert.NotNull(context.Result);
        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal("User claims missing or invalid.", result.Value);
    }
}