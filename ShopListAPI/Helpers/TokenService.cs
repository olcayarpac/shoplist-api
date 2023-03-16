using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ShopListAPI.Models;

namespace ShopListAPI.Helpers;

public class TokenHelper
{
    private readonly IConfiguration _configuration;
    public TokenHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Token CreateAccessToken(string userId)
    {
        var tokenModel = new Token();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecurityKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("Id", userId) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        tokenModel.AccessToken = tokenHandler.WriteToken(token);
        tokenModel.ExpireDate = token.ValidTo;
        tokenModel.RefreshToken = CreateRefreshToken();
        return tokenModel;
    }

    public string CreateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
}