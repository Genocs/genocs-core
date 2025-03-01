using Genocs.Auth.Configurations;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Genocs.Auth;

/// <summary>
/// The access token validator middleware.
/// </summary>
/// <remarks>
/// The AccessTokenValidatorMiddleware constructor.
/// </remarks>
/// <param name="accessTokenService">The access token service.</param>
/// <param name="options">The options.</param>
public class AccessTokenValidatorMiddleware(IAccessTokenService accessTokenService, JwtOptions options) : IMiddleware
{
    private readonly IAccessTokenService _accessTokenService = accessTokenService;
    private readonly IEnumerable<string> _allowAnonymousEndpoints = options.AllowAnonymousEndpoints ?? [];

    /// <summary>
    /// The InvokeAsync method.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <param name="next">The request delegate.</param>
    /// <returns>The task.</returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;

        // Skip check on AnonymousEndpoints
        if (_allowAnonymousEndpoints.Contains(path))
        {
            await next(context);
            return;
        }

        if (_accessTokenService.IsCurrentActiveToken())
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    }
}