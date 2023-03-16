using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopListAPI.Models;
using ShopListAPI.Services;

namespace ShopListAPI.Controllers;

[ApiController]
[Route("api/[controller]/")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService, IConfiguration configuration)
    {
        _accountService = accountService;
    }

    [AllowAnonymous]
    [HttpPost("createAccount")]
    public async Task<ActionResult> CreateAccount([FromBody] User newUser)
    {
        await _accountService.CreateUserAsync(newUser);
        return Ok(newUser);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<Token>> Login([FromBody] User user)
    {
        var existingUser = await _accountService.CheckCredentials(user);
        if (existingUser is null)
            return Unauthorized("Invalid credentials");
        var token = await _accountService.CreateTokenAsync(existingUser.Id);

        Response.Headers.Add("Set-Cookie", "userid=" + existingUser.Id + ";");
        return Ok(token);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpGet("refreshToken")]
    public async Task<ActionResult<Token>> RefreshToken([FromQuery] string refreshToken)
    {
        var token = await _accountService.RefreshTokenAsync(refreshToken);
        return token;
    }
}
