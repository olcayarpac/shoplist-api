using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShopListAPI.Models;
using ShopListAPI.Services;

namespace ShopListAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]/")]
public class ShopListController : ControllerBase
{
    private readonly ShopListService _shopListService;
    public ShopListController(ShopListService shopListService)
    {
        _shopListService = shopListService;
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetShopListById([FromRoute] string listId)
    {
        var shopList = await _shopListService.GetShopListsByUserId(listId);
        return Ok(shopList);
    }

    [Authorize(Roles = "HRManager,Finance")]
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
        return Ok(newShopList);
    }

    [HttpDelete("{listId}/deleteShopList")]
    public async Task<ActionResult> DeleteListItem([FromRoute] string listId)
    {
        await _shopListService.DeleteShopList(listId);
        return Ok();
    }

    [HttpPost("{listId}/createListItem")]
    public async Task<ActionResult> CreateListItem([FromRoute] string listId, [FromBody] ListItem newItem)
    {
        await _shopListService.InsertListItem(listId, newItem);
        return Ok(newItem);
    }

    [HttpDelete("{listId}/deleteListItem")]
    public async Task<ActionResult> DeleteListItem([FromRoute] string listId, [FromQuery] string itemId)
    {
        await _shopListService.DeleteListItem(listId, itemId);
        return Ok();
    }

    [HttpPut("{listId}/{itemId}/updateItemStatus")]
    public async Task<ActionResult> MarkListItemAsDone([FromRoute] string listId, [FromRoute] string itemId, [FromQuery] bool isDone)
    {
        var resultList = await _shopListService.UpdateItemDoneStatus(listId, itemId, isDone);
        if (resultList.IsDone == true) return Ok("List updated as done");
        return Ok();
    }
}
