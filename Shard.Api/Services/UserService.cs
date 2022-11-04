﻿using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface IUserService
{
    public void AddUser(User user);
    public void AddUnitUser(Unit unit, User user);
    User GetUser(string userId);
    List<Unit> GetUnitsOfUserById(string userId);
    Unit GetUnitOfUserById(string userId, string unitId);
    Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock);
    ActionResult<Building> CreateBuilding(string userId, Building building);
}

public class UserService : IUserService
{
    private readonly Dictionary<User, List<Unit>> _usersUnitsDb;
    private readonly Dictionary<User, List<Building>> _usersBuildingsDb;

    public UserService()
    {
        _usersUnitsDb = new Dictionary<User, List<Unit>>();
        _usersBuildingsDb = new Dictionary<User, List<Building>>();
    }

    public void AddUser(User user)
    {
        if (!_usersUnitsDb.ContainsKey(user))
        {
            _usersUnitsDb.Add(user, new List<Unit>());
        }
    }

    public void AddUnitUser(Unit unit, User user)
    {
        if (_usersUnitsDb.ContainsKey(user))
        {
            _usersUnitsDb[user].Add(unit);
        }
    }

    public User GetUser(string userId)
        => _usersUnitsDb.Keys.FirstOrDefault(u => u.Id == userId) ?? null;


    public List<Unit> GetUnitsOfUserById(string userId)
    {
        var user = _usersUnitsDb.Keys.FirstOrDefault(u => u.Id == userId);
        return user != null ? _usersUnitsDb[user] : null;
    }

    public Unit GetUnitOfUserById(string userId, string unitId)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        return user != null ? _usersUnitsDb[user].FirstOrDefault(u => u.Id == unitId) ?? null : null;
    }

    public Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDb[user].First(u => u.Id == unitId);
            if (unit == null)
            {
                return null;
            }

            _usersUnitsDb[user].Remove(unit);
            unit.DestinationSystem = unitUpdated.DestinationSystem;
            unit.DestinationPlanet = unitUpdated.DestinationPlanet;
            _usersUnitsDb[user].Add(unit);


            unit.MoveTask = MoveUnitBackgroundTask(unit, user, clock);
            unit.LastUpdate = clock.Now;
            return unitUpdated;
        }
        return null;
    }

    private async Task MoveUnitBackgroundTask(Unit unit, User user, IClock clock)
    {
        await Task.Run(async () =>
        {
            var tmp = _usersUnitsDb[user].First(u => u.Id == unit.Id);
            if ((tmp.System == null) ||
                (tmp.DestinationSystem != null && !tmp.System.Equals(tmp.DestinationSystem))
               )
            {
                tmp.ETA += 60000;
                await clock.Delay(60000);
                tmp.System = tmp.DestinationSystem;
                tmp.DestinationSystem = null;
                tmp.LastUpdate = clock.Now;
            }

            if (tmp.Planet==null ||
                (tmp.DestinationPlanet != null && !tmp.Planet.Equals(tmp.DestinationPlanet)))
            {
                tmp.ETA += 15000;
                await clock.Delay(15000);
                tmp.Planet = tmp.DestinationPlanet;
                tmp.DestinationPlanet = null;
                tmp.LastUpdate = clock.Now;
            }
        });
    }

    public ActionResult<Building> CreateBuilding(string userId, Building building)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDb[user].Find(u => u.Id == building.BuilderId);
            if (unit != null)
            {
                if (unit.Planet == null || unit.System == null || unit.Type != "builder")
                {
                    return new BadRequestObjectResult("Unit must be on a planet to build");
                }

                building.System = unit.System;
                building.Planet = unit.Planet;
                if (_usersBuildingsDb.ContainsKey(user))
                    _usersBuildingsDb[user].Add(building);
                else
                    _usersBuildingsDb.Add(user, new List<Building> { building });

                return building;
            }
        }
        throw new Exception("User not found");
    }
}