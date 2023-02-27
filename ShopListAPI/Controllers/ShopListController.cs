using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ShopListAPI.Controllers;

[ApiController]
[Route("api/[controller]/")]
[Authorize]
public class ShopListController : ControllerBase
{
    public ShopListController()
    {
    }

    [HttpPost]
    public ActionResult Post()
    {
        return Ok();
    }

    [HttpPost("post2")]
    public ActionResult Post2()
    {
        return Ok();
    }

}
