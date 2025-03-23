using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movies.API.Auth;

public class AdminAuthRequirement : IAuthorizationRequirement, IAuthorizationHandler
{
    private readonly string _apiKey;

    public AdminAuthRequirement(string apiKey)
    {
        _apiKey = apiKey;
    }

    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if(context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
        {
            context.Succeed(this);
            return Task.CompletedTask;
        }

        var httpContext = context.Resource as HttpContext;
        if(httpContext is null)
        {
            return Task.CompletedTask;
        }

        if(!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName,
            out var extractedApiKey))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if(_apiKey != extractedApiKey)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        //Attach userId to token
        var identity = (ClaimsIdentity)httpContext.User.Identity!;
        identity.AddClaim(
            new Claim(AuthConstants.UserIdClaimName,
                    Guid.Parse("ca62ee98-4eb5-4443-b290-21c04a872dd4").ToString()));
        context.Succeed(this);
        return Task.CompletedTask;

    }
}
