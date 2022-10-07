using System.Net;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Api.Services;

namespace Shard.Api.Controllers;

public class UserController : Controller
{
    private readonly ICelestialService _celestialService;
    private readonly IUserService _userService;

    public UserController(ICelestialService celestialService, IUserService userService)
    {
        _celestialService = celestialService;
        _userService = userService;
    }

    [HttpPut("users/{userId}")]
    [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(User), (int)HttpStatusCode.NotFound)]
    public ActionResult<User> createUser(string userId, [FromBody] User user)
    {
        if (user == null 
            ||
            userId == null
            ||
            userId != user.id
            ||
            userId.Length == 1 && Regex.IsMatch(userId, @"[!@#$'%^&*()_+=\[{\]};:<>|./?,-]")
           )
        {
            return BadRequest();
        }

        _userService.addUser(user);
        _userService.addVaisseauUser(
            new Vaisseau(Guid.NewGuid().ToString(), "scout", _celestialService.getRandomSystem()), user);

        return user;
    }


    [HttpGet("users/{userId}")]
    public ActionResult<User> getUser(string userId)
    {
        var res = _userService.getUser(userId);
        if (res == null)
        {
            return new NotFoundResult();
        }

        return res;
    }

    [HttpGet("users/{userId}/units")]
    public ActionResult<List<Vaisseau>> getAllUnits(string userId)
    {
        var x = _userService.getUnitsOfUserById(userId);
        Console.WriteLine("User units : " + x);
        Console.WriteLine("User units : " + x[0].toString());
        return Json(x);
    }

    [HttpGet("users/{userId}/units/{unitId}")]
    public ActionResult<Vaisseau> getUnit(string userId, string unitId)
    {
        var x = _userService.getUnitOfUserById(userId, unitId);
        if (x == null)
        {
            return new NotFoundResult();
        }
        return Json(x);
    }
    
    [HttpPut("users/{userId}/units/{unitId}")]
    public ActionResult<Vaisseau> updateUnit(string userId, string unitId, [FromBody] Vaisseau vaisseau)
    {
        var x = _userService.updateUnitOfUserById(userId, unitId, vaisseau);
        return Json(x);
    }
    
    [HttpDelete("users/{userId}/units/{unitId}/location")]
    public ActionResult<Vaisseau> deleteUnitLocation(string userId, string unitId)
    {
        return BadRequest("Not implemented");
    }
    
}