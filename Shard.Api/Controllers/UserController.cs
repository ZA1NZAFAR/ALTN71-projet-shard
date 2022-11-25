using System.Text.RegularExpressions;
using System.Web.WebPages;
using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Api.Services;
using Shard.Api.Tools;
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
    public ActionResult<User> CreateUser(string userId, [FromBody] User user)
    {
        if (user == null || userId == null || userId != user.Id ||
            userId.Length == 1 && Regex.IsMatch(userId, @"[!@#$'%^&*()_+=\[{\]};:<>|./?,-]")
           )
            return BadRequest();


        user.ResourcesQuantity = new Dictionary<ResourceKind, int>()
        {
            { ResourceKind.Carbon, 20 },
            { ResourceKind.Iron, 10 },
            { ResourceKind.Oxygen, 50 },
            { ResourceKind.Water, 50 },
            { ResourceKind.Aluminium, 0 },
            { ResourceKind.Gold, 0 },
            { ResourceKind.Titanium, 0 }
        };

        _userService.AddUser(user);
        var system = _celestialService.GetRandomSystem();
        _userService.AddUnitUser(
            new Unit(Guid.NewGuid().ToString(), "scout", system.Name, null!), user);
        _userService.AddUnitUser(
            new Unit(Guid.NewGuid().ToString(), "builder", system.Name, null!), user);

        return user;
    }


    [HttpGet("users/{userId}")]
    public ActionResult<User> GetUser(string userId)
    {
        var res = _userService.GetUser(userId);
        if (res == null)
            return new NotFoundResult();

        // update user resources quantity
        SwissKnife.UpdateResources(res, _userService, _celestialService, _clock);
        return res;
    }


    [HttpGet("users/{userId}/units")]
    public List<Unit> GetAllUnits(string userId)
        => _userService.GetUnitsOfUserById(userId) ?? throw new InvalidOperationException();

    [HttpGet("users/{userId}/units/{unitId}")]
    public async Task<ActionResult<Unit>> GetUnit(string userId, string unitId)
    {
        var x = _userService.GetUnitOfUserById(userId, unitId);
        if (x == null)
            return new NotFoundResult();

        // wait if the movement is not finished and arrival time is less than 2 seconds
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
        if (x == null)
            return new NotFoundResult();
        return x;
    }

    [HttpGet("users/{userId}/units/{unitId}/location")]
    public ActionResult<Location> GetUnitLocation(string userId, string unitId)
    {
        var unit = _userService.GetUnitOfUserById(userId, unitId);

        var location = new Location(unit.System, _celestialService.GetPlanetOfSystem(unit.System, unit.Planet));

        // getting builder doesn't return resources
        if (unit.Type.Equals("builder"))
        {
            location.ResourcesQuantity = null;
        }

        return location;
    }

    [HttpPost("users/{userId}/buildings")]
    public ActionResult<Building> CreateBuilding(string userId, [FromBody] Building building)
    {
        // various checks --start
        if (building == null || building.BuilderId == null)
            return BadRequest("Building or builder id is null");

        if (!(building.ResourceCategory == null) && !building.ResourceCategory.Equals("gaseous") &&
            !building.ResourceCategory.Equals("solid") &&
            !building.ResourceCategory.Equals("liquid"))
            return BadRequest("Resource category is not valid");

        if (_userService.GetUser(userId) == null)
            return new NotFoundResult();

        if (building == null || building.Type.IsEmpty() || building.BuilderId.IsEmpty())
            return BadRequest();
        // various checks --end

        try
        {
            var freshlyCreatedBuilding = _userService.CreateBuilding(userId, building, _clock);
            return freshlyCreatedBuilding;
        }
        catch (Exception)
        {
            return BadRequest("Building creation failed");
        }
    }


    [HttpGet("users/{userId}/buildings")]
    public ActionResult<List<Building>> GetAllBuildings(string userId)
    {
        try
        {
            // can return null if no buildings are found
            var buildings = _userService.GetBuildingsOfUserById(userId);
            if (buildings == null)
                throw new Exception("No buildings found");

            return buildings;
        }
        catch (Exception)
        {
            return NotFound();
        }
    }


    [HttpGet("users/{userId}/Buildings/{buildingId}")]
    public ActionResult<Building> GetBuilding(string userId, string buildingId)
    {
        try
        {
            var building = _userService.GetBuildingOfUserById(userId, buildingId);
            if (building == null)
                throw new Exception("No building found");

            // wait if the building is not finished and finish time is less than 2 seconds - start
            if (building.BuildTask != null && building.EstimatedBuildTime != null)
            {
                if (building.EstimatedBuildTime - _clock.Now <= TimeSpan.FromSeconds(2))
                {
                    // keep refreshing building object in case it gets updated/deleted during the wait
                    while (building.EstimatedBuildTime - _clock.Now >= TimeSpan.Zero)
                    {
                        try
                        {
                            building = _userService.GetBuildingOfUserById(userId, buildingId);
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }
                        Thread.Sleep(500);
                    }
                }
            }
            // wait if the building is not finished and finish time is less than 2 seconds - end

            return building;
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}