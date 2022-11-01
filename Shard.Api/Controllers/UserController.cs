﻿using System.Net;
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
    public ActionResult<User> createUser(string userId, [FromBody] User user)
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

        _userService.AddUser(user);
        var system = _celestialService.GetRandomSystem();
        _userService.AddUnitUser(
            new Unit(Guid.NewGuid().ToString(), "scout", system.Name, null), user);
        _userService.AddUnitUser(
            new Unit(Guid.NewGuid().ToString(), "builder", system.Name, null), user);

        return user;
    }


    [HttpGet("users/{userId}")]
    public ActionResult<User> getUser(string userId)
    {
        var res = _userService.GetUser(userId);
        if (res == null)
        {
            return new NotFoundResult();
        }

        return res;
    }

    [HttpGet("users/{userId}/units")]
    public ActionResult<List<Unit>> getAllUnits(string userId)
    {
        var x = _userService.GetUnitsOfUserById(userId);
        return x;
    }

    [HttpGet("users/{userId}/units/{unitId}")]
    public async Task<ActionResult<Unit>> getUnit(string userId, string unitId)
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
    public ActionResult<Unit> updateUnit(string userId, string unitId, [FromBody] Unit unit)
    {
        var x = _userService.UpdateUnitOfUserById(userId, unitId, unit, _clock);
        return x;
    }

    [HttpGet("users/{userId}/units/{unitId}/location")]
    public ActionResult<Location> getUnitLocation(string userId, string unitId)
    {
        Unit temp = _userService.GetUnitOfUserById(userId, unitId);

        Location l = new Location(temp.System, _celestialService.GetPlanetOfSystem(temp.System, temp.Planet));

        if (temp.Type.Equals("builder"))
        {
            l.resourcesQuantity = null;
        }

        return l;
    }

    [HttpPost("users/{userId}/buildings")]
    public ActionResult<Building> createBuilding(string userId, [FromBody] Building building)
    {
        if (_userService.GetUser(userId) == null)
            return new NotFoundResult();

        if (building == null || building.Type.IsEmpty() || building.BuilderId.IsEmpty() ||
            !building.Type.Equals("mine"))
        {
            return BadRequest();
        }

        try
        {
            var x = _userService.CreateBuilding(userId, building);
            return x;
        }
        catch (Exception e)
        {
            return BadRequest();
        }
    }
}