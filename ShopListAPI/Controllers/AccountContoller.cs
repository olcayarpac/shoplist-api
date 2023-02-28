using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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

    [HttpPost("createAccount")]
    public async Task<ActionResult> CreateAccount([FromBody] User newUser)
    {
        await _accountService.CreateUserAsync(newUser);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<Token>> Login([FromBody] User user)
    {
        var token = await _accountService.CreateTokenAsync(user);
        return Ok(token);
    }

    [HttpGet("refreshToken")]
    public async Task<ActionResult<Token>> RefreshToken([FromQuery] string refreshToken)
    {
        var token = await _accountService.RefreshTokenAsync(refreshToken);
        return token;
    }
}
