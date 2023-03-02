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
[Authorize]
public class ShopListController : ControllerBase
{
    private readonly ShopListService _shopListService;
    public ShopListController(ShopListService shopListService)
    {
        _shopListService = shopListService;
    }

    [HttpGet]
    public async Task<ActionResult> GetShopLists([FromQuery] string userId)
    {
        var shopList = await _shopListService.GetShopListsByUserId(userId);
        return Ok(shopList);
    }

    [HttpPost("createShopList")]
    public async Task<ActionResult> CreateShopList([FromBody] ShopList newShopList)
    {
        await _shopListService.CreateShopList(newShopList);
        return Ok();
    }

}
