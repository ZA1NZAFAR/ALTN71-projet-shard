﻿using System.Text;
using System.Text.RegularExpressions;
using System.Web.WebPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shard.Api.Models;
using Shard.Api.Models.Enums;
using Shard.Api.Models.Exceptions;
using Shard.Api.Services;
using Shard.Api.Tools;
using Shard.Shared.Core;

namespace Shard.Api.Controllers;

public class UserController : Controller
{
    private readonly ICelestialService _celestialService;
    private readonly IUserService _userService;
    private readonly IClock _clock;
    private bool _isAuthenticated;


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


        if (_isAuthenticated)
        {
            if (_userService.IfExistThenUpdateUser(user))
            {
                return Ok(user);
            }
        }


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
            new Unit(Guid.NewGuid().ToString(), UnitTypes.scout.ToString(), system.Name, null!), user, _clock);
        _userService.AddUnitUser(
            new Unit(Guid.NewGuid().ToString(), UnitTypes.builder.ToString(), system.Name, null!), user, _clock);

        return user;
    }


    [HttpGet("users/{userId}")]
    public ActionResult<User> GetUser(string userId)
    {
        var res = _userService.GetUser(userId);
        if (res == null)
            return new NotFoundResult();
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
        try
        {
            unit.Owner = userId;
            unit.EquipWeapons(_clock);
            var x = _userService.UpdateUnitOfUserById(userId, unitId, unit, _clock, _isAuthenticated);
            if (x == null)
                return new NotFoundResult();
            return x;
        }
        catch (IsUnauthorizedException e)
        {
            return new UnauthorizedResult();
        }
    }

    [HttpGet("users/{userId}/units/{unitId}/location")]
    public ActionResult<Location> GetUnitLocation(string userId, string unitId)
    {
        var unit = _userService.GetUnitOfUserById(userId, unitId);

        var location = new Location(unit.System, _celestialService.GetPlanetOfSystem(unit.System, unit.Planet));

        // getting builder doesn't return resources
        if (unit.Type.Equals(UnitTypes.builder.ToString()))
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

        if (!(building.ResourceCategory == null) &&
            !building.ResourceCategory.Equals(ResourceCategories.gaseous.ToString()) &&
            !building.ResourceCategory.Equals(ResourceCategories.solid.ToString()) &&
            !building.ResourceCategory.Equals(ResourceCategories.liquid.ToString()))
            return BadRequest("Resource category is not valid");

        if (_userService.GetUser(userId) == null)
            return new NotFoundResult();

        if (building == null || building.Type.IsEmpty() || building.BuilderId.IsEmpty())
            return BadRequest();

        if (!building.Type.Equals(BuildingTypes.mine.ToString()) &&
            !building.Type.Equals(BuildingTypes.starport.ToString()))
        {
            return BadRequest();
        }
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

                        _clock.Sleep(500);
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

    [HttpPost("/users/{userId}/Buildings/{starportId}/queue")]
    public ActionResult<Unit> AddToQueue(string userId, string starportId, [FromBody] UnitType unit)
    {
        try
        {
            var starport = _userService.GetBuildingOfUserById(userId, starportId);
            if (starport == null)
                throw new Exception("No starport found");

            if (!starport.Type.Equals(BuildingTypes.starport.ToString()))
                throw new Exception("Building is not a starport");

            if (unit == null || unit.Type.IsEmpty())
                throw new Exception("Building or building type is null");

            var res = _userService.AddToQueue(userId, starportId, unit, _clock);
            return res;
        }
        catch (Exception e)
        {
            switch (e.Message)
            {
                case "No starport found":
                    return NotFound();
                case "Building is not a starport":
                    return BadRequest();
                case "Building or building type is null":
                    return BadRequest();
                case "Starport is not built yet":
                    return BadRequest();
                case "User has no resources":
                    return BadRequest();
                case "User has not enough resources":
                    return BadRequest();
                default:
                    return NotFound();
            }
        }
    }

    // override OnActionExecuting to check if the user is authenticated
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string authHeader = Request.Headers["Authorization"];

        if (authHeader != null && authHeader.StartsWith("Basic"))
        {
            var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

            var separatorIndex = usernamePassword.IndexOf(':');

            var username = usernamePassword.Substring(0, separatorIndex);
            var password = usernamePassword.Substring(separatorIndex + 1);

            // check credentials
            if (username.Equals("admin") && password.Equals("password"))
            {
                _isAuthenticated = true;
            }
        }

        // Every request triggers resource update for all users
        foreach (var user in _userService.GetAllUsers())
            // update user resources quantity
            SwissKnife.UpdateResources(user, _userService, _celestialService, _clock);

        // BackGroundTasks.Fight(_userService, _celestialService, _clock);

        base.OnActionExecuting(context);
    }
}