using System.IdentityModel.Tokens.Jwt;
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

    public Token CreateAccessToken()
    {
        Token token = new Token();
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:SecurityKey"]));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        token.ExpireDate = DateTime.Now.AddHours(3);
        JwtSecurityToken securityToken = new JwtSecurityToken(
            issuer: _configuration["Token:Issuer"],
            audience: _configuration["Token:Audience"],
            notBefore: DateTime.Now,
            expires: token.ExpireDate,
            signingCredentials:credentials 
        );

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        token.AccessToken = tokenHandler.WriteToken(securityToken);
        token.RefreshToken = CreateRefreshToken();
        return token;
    }

    public string CreateRefreshToken(){
        return Guid.NewGuid().ToString();
    }
}