using System.Net;
using System.Text.RegularExpressions;
using System.Web.WebPages;
using Microsoft.AspNetCore.Mvc;
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
    public ActionResult<User> CreateUser(string userId, [FromBody] User user)
    {
        if (user == null
            ||
            userId == null
            ||
            userId != user.Id
            ||
            userId.Length == 1 && Regex.IsMatch(userId, @"[!@#$'%^&*()_+=\[{\]};:<>|./?,-]")
           )
        {
            return BadRequest();
        }

        user.ResourcesQuantity = new Dictionary<ResourceKind, int>()
        {
            { ResourceKind.Carbon, 20 },
            { ResourceKind.Iron, 10 },
            { ResourceKind.Oxygen, 50 },
            { ResourceKind.Water, 50 }
        };

        _userService.AddUser(user);
        var system = _celestialService.GetRandomSystem();
        _userService.AddUnitUser(
            new Unit(Guid.NewGuid().ToString(), "scout", system.Name, null), user);
        _userService.AddUnitUser(
            new Unit(Guid.NewGuid().ToString(), "builder", system.Name, null), user);

        return user;
    }


    [HttpGet("users/{userId}")]
    public ActionResult<User> GetUser(string userId)
    {
        var res = _userService.GetUser(userId);
        if (res == null)
        {
            return new NotFoundResult();
        }

        return res;
    }

    [HttpGet("users/{userId}/units")]
    public ActionResult<List<Unit>> GetAllUnits(string userId)
    {
        var x = _userService.GetUnitsOfUserById(userId);
        return x;
    }

    [HttpGet("users/{userId}/units/{unitId}")]
    public async Task<ActionResult<Unit>> GetUnit(string userId, string unitId)
    {
        var x = _userService.GetUnitOfUserById(userId, unitId);
        if (x == null)
        {
            return new NotFoundResult();
        }


        if (x.MoveTask != null && x.ETA != null)
        {
            if (x.ETA - (_clock.Now.Second * 1000) <= 2000)
            {
                await x.MoveTask;
            }
        }

        return x;
    }

    [HttpPut("users/{userId}/units/{unitId}")]
    public ActionResult<Unit> UpdateUnit(string userId, string unitId, [FromBody] Unit unit)
    {
        var x = _userService.UpdateUnitOfUserById(userId, unitId, unit, _clock);
        return x;
    }

    [HttpGet("users/{userId}/units/{unitId}/location")]
    public ActionResult<Location> GetUnitLocation(string userId, string unitId)
    {
        var temp = _userService.GetUnitOfUserById(userId, unitId);

        var l = new Location(temp.System, _celestialService.GetPlanetOfSystem(temp.System, temp.Planet));

        if (temp.Type.Equals("builder"))
        {
            l.resourcesQuantity = null;
        }

        return l;
    }

    [HttpPost("users/{userId}/buildings")]
    public ActionResult<Building> CreateBuilding(string userId, [FromBody] Building building)
    {
        if (building == null || building.BuilderId == null )
        {
            return BadRequest();
        }

        if (!(building.ResourceCategory ==null) && !building.ResourceCategory.Equals("gaseous") && !building.ResourceCategory.Equals("solid") &&
            !building.ResourceCategory.Equals("liquid"))
        {
            return BadRequest();
        }

        if (_userService.GetUser(userId) == null)
            return new NotFoundResult();

        if (building == null || building.Type.IsEmpty() || building.BuilderId.IsEmpty() ||
            !building.Type.Equals("mine"))
        {
            return BadRequest();
        }

        try
        {
            var x = _userService.CreateBuilding(userId, building, _clock);
            return x;
        }
        catch (Exception ignored)
        {
            return BadRequest();
        }
    }


    [HttpGet("users/{userId}/buildings")]
    public ActionResult<List<Building>> GetAllBuildings(string userId)
    {
        try
        {
            var buildings = _userService.GetBuildingsOfUserById(userId);
            if (buildings == null)
                throw new Exception("No buildings found");

            return buildings;
        }
        catch (Exception e)
        {
            return NotFound();
        }
    }


    [HttpGet("users/{userId}/Buildings/{buildingId}")]
    public ActionResult<Building> GetBuilding(string userId, string buildingId)
    {
        try
        {
            var building =  _userService.GetBuildingOfUserById(userId, buildingId);
            if (building == null)
                throw new Exception("No building found");
            return building;
        }
        catch (Exception e)
        {
            return NotFound();
        }
    }
}