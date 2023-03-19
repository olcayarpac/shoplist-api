namespace WebApi.Middlewares;

using Microsoft.Extensions.Options;
using ShopListAPI.Helpers;
using ShopListAPI.Models;
using ShopListAPI.Services;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AccountService _accountService;
    //private readonly TokenHelper _tokenHelper;

    public AuthMiddleware(RequestDelegate next, AccountService accountService)
    {
        _next = next;
        _accountService = accountService;
    }

    public async Task Invoke(HttpContext context, TokenHelper _tokenHelper)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var userId = _tokenHelper.ValidateToken(token!);
        if (userId != null)
        {
            // attach user to context on successful jwt validation
            context.Items["User"] = await _accountService.GetAsync(userId);
        }

        await _next(context);
    }
}
public static class AuthMiddlewareExtension
{
    public static IApplicationBuilder UseAuthMiddlewareExtension(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthMiddleware>();
    }
}