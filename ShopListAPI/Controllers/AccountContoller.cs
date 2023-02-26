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
    private readonly IConfiguration _configuration;

    public AccountController(AccountService accountService, IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser([FromBody] User newUser){
        await _accountService.CreateUserAsync(newUser);
        return Ok();
    }

    private string GenerateJwt()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //If you've had the login module, you can also use the real user information here
        var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, "user_name"),
        new Claim(JwtRegisteredClaimNames.Email, "user_email"),
        new Claim("DateOfJoing", "2022-09-12"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
            _configuration["Jwt:Issuer"],
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
