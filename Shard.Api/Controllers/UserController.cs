using System.Net;
using System.Text.RegularExpressions;
using System.Web.Helpers;
using System.Web.WebPages;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Shard.Api.Helpers;
using Shard.Api.Models;
using Shard.Api.Services;
using Shard.Shared.Core;

namespace Shard.Api.Controllers;

public class UserController : Controller
{
    private readonly ICelestialService _celestialService;
    private readonly IUserService _userService;
    private readonly IClock _clock;


    public UserController(ICelestialService celestialService, IUserService userService, IClock clock)
    {
        _celestialService = celestialService;
        _userService = userService;
        _clock = clock;
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
        var system = _celestialService.getRandomSystem();
        _userService.addVaisseauUser(
            new Vaisseau(Guid.NewGuid().ToString(), "scout", system.Name, null), user);
        _userService.addVaisseauUser(
            new Vaisseau(Guid.NewGuid().ToString(), "builder", system.Name, null), user);

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
    public async Task<ActionResult<Vaisseau>> getUnit(string userId, string unitId)
    {
        var x = _userService.getUnitOfUserById(userId, unitId);
        if (x == null)
        {
            return new NotFoundResult();
        }


        if (x.moveTask != null && x.moveTaskTime != null)
        {
            await x.moveTask;
        }

        return x;
    }

    [HttpPut("users/{userId}/units/{unitId}")]
    public ActionResult<Vaisseau> updateUnit(string userId, string unitId, [FromBody] Vaisseau vaisseau)
    {
        var x = _userService.updateUnitOfUserById(userId, unitId, vaisseau, _clock);
        return x;
    }

    [HttpGet("users/{userId}/units/{unitId}/location")]
    public ActionResult<Location> getUnitLocation(string userId, string unitId)
    {
        Vaisseau temp = _userService.getUnitOfUserById(userId, unitId);

        Location l = new Location(temp.system, _celestialService.getPlanetOfSystem(temp.system, temp.planet));

        if (temp.type.Equals("builder"))
        {
            l.resourcesQuantity = null;
        }

        return l;
    }

    [HttpPost("users/{userId}/buildings")]
    public ActionResult<Building> createBuilding(string userId, [FromBody] Building building)
    {
        if (_userService.getUser(userId) == null)
            return new NotFoundResult();

        if (building == null || building.Type.IsEmpty() || building.BuilderId.IsEmpty() ||
            !building.Type.Equals("mine"))
        {
            return BadRequest();
        }

        try
        {
            var x = _userService.createBuilding(userId, building);
            return x;
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }
}