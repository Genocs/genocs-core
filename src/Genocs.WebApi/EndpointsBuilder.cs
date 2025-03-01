using Genocs.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Genocs.WebApi;

public class EndpointsBuilder(IEndpointRouteBuilder routeBuilder, WebApiEndpointDefinitions definitions) : IEndpointsBuilder
{
    private readonly WebApiEndpointDefinitions _definitions = definitions;
    private readonly IEndpointRouteBuilder _routeBuilder = routeBuilder;

    public IEndpointsBuilder Get(
                                    string path,
                                    Func<HttpContext, Task>? context = null,
                                    Action<IEndpointConventionBuilder>? endpoint = null,
                                    bool auth = false,
                                    string? roles = null,
                                    params string[] policies)
    {
        var builder = _routeBuilder.MapGet(path, ctx => context?.Invoke(ctx));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition(HttpMethods.Get, path);

        return this;
    }

    public IEndpointsBuilder Get<T>(
                                    string path,
                                    Func<T, HttpContext, Task>? context = null,
                                    Action<IEndpointConventionBuilder> endpoint = null,
                                    bool auth = false,
                                    string? roles = null,
                                    params string[] policies)
        where T : class
    {
        var builder = _routeBuilder.MapGet(path, ctx => BuildQueryContext(ctx, context));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition<T>(HttpMethods.Get, path);

        return this;
    }

    public IEndpointsBuilder Get<TRequest, TResult>(string path, Func<TRequest, HttpContext, Task> context = null,
        Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string? roles = null,
        params string[] policies)
        where TRequest : class
    {
        var builder = _routeBuilder.MapGet(path, ctx => BuildQueryContext(ctx, context));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition<TRequest, TResult>(HttpMethods.Get, path);

        return this;
    }

    public IEndpointsBuilder Post(string path, Func<HttpContext, Task> context = null,
        Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string? roles = null,
        params string[] policies)
    {
        var builder = _routeBuilder.MapPost(path, ctx => context?.Invoke(ctx));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition(HttpMethods.Post, path);

        return this;
    }

    public IEndpointsBuilder Post<T>(string path, Func<T, HttpContext, Task> context = null,
        Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string? roles = null,
        params string[] policies)
        where T : class
    {
        var builder = _routeBuilder.MapPost(path, ctx => BuildRequestContext(ctx, context));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition<T>(HttpMethods.Post, path);

        return this;
    }

    public IEndpointsBuilder Put(string path, Func<HttpContext, Task> context = null,
        Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string? roles = null,
        params string[] policies)
    {
        var builder = _routeBuilder.MapPut(path, ctx => context?.Invoke(ctx));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition(HttpMethods.Put, path);

        return this;
    }

    public IEndpointsBuilder Put<T>(string path, Func<T, HttpContext, Task> context = null,
        Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string? roles = null,
        params string[] policies)
        where T : class
    {
        var builder = _routeBuilder.MapPut(path, ctx => BuildRequestContext(ctx, context));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition<T>(HttpMethods.Put, path);

        return this;
    }

    public IEndpointsBuilder Delete(string path, Func<HttpContext, Task>? context = null,
        Action<IEndpointConventionBuilder> endpoint = null, bool auth = false, string? roles = null,
        params string[] policies)
    {
        var builder = _routeBuilder.MapDelete(path, ctx => context?.Invoke(ctx));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition(HttpMethods.Delete, path);

        return this;
    }

    public IEndpointsBuilder Delete<T>(string path, Func<T, HttpContext, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
        where T : class
    {
        var builder = _routeBuilder.MapDelete(path, ctx => BuildQueryContext(ctx, context));
        endpoint?.Invoke(builder);
        ApplyAuthRolesAndPolicies(builder, auth, roles, policies);
        AddEndpointDefinition<T>(HttpMethods.Delete, path);

        return this;
    }

    private static void ApplyAuthRolesAndPolicies(IEndpointConventionBuilder builder, bool auth, string? roles, params string[] policies)
    {
        if (policies?.Any() == true)
        {
            builder.RequireAuthorization(policies);
            return;
        }

        bool hasRoles = !string.IsNullOrWhiteSpace(roles);
        if (hasRoles)
        {
            var authorize = new AuthorizeAttribute();
            authorize.Roles = roles;
            builder.RequireAuthorization(authorize);
            return;
        }

        if (auth)
        {
            builder.RequireAuthorization();
            return;
        }

        // I don't like this, but it is the only way to allow anonymous access
        builder.AllowAnonymous();
    }

    private static async Task BuildRequestContext<T>(HttpContext httpContext, Func<T, HttpContext, Task>? context = null)
        where T : class
    {
        var request = await httpContext.ReadJsonAsync<T>();
        if (request is null || context is null)
        {
            return;
        }

        await context.Invoke(request, httpContext);
    }

    private static async Task BuildQueryContext<T>(HttpContext httpContext, Func<T, HttpContext, Task>? context = null)
        where T : class
    {
        var request = httpContext.ReadQuery<T>();
        if (request is null || context is null)
        {
            return;
        }

        await context.Invoke(request, httpContext);
    }

    private void AddEndpointDefinition(string method, string path)
    {
        _definitions.Add(new WebApiEndpointDefinition
        {
            Method = method,
            Path = path,
            Responses = new List<WebApiEndpointResponse>
            {
                new()
                {
                    StatusCode = 200
                }
            }
        });
    }

    private void AddEndpointDefinition<T>(string method, string path)
        => AddEndpointDefinition(method, path, typeof(T), null);

    private void AddEndpointDefinition<Ta, Tu>(string method, string path)
        => AddEndpointDefinition(method, path, typeof(Ta), typeof(Tu));

    private void AddEndpointDefinition(string method, string path, Type input, Type? output)
    {
        if (_definitions.Exists(d => d.Path == path && d.Method == method))
        {
            return;
        }

        _definitions.Add(new WebApiEndpointDefinition
        {
            Method = method,
            Path = path,
            Parameters = new List<WebApiEndpointParameter>
            {
                new()
                {
                    In = method == HttpMethods.Get ? "query" : "body",
                    Name = input?.Name,
                    Type = input,
                    Example = input?.GetDefaultInstance()
                }
            },
            Responses = new List<WebApiEndpointResponse>
            {
                new()
                {
                    StatusCode = method == HttpMethods.Get ? 200 : 202,
                    Type = output,
                    Example = output?.GetDefaultInstance()
                }
            }
        });
    }
}