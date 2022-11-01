using Microsoft.AspNetCore.Mvc;
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
    private Dictionary<User, List<Unit>> _usersUnitsDB;
    private Dictionary<User, List<Building>> _usersBuildingsDB;

    public UserService()
    {
        _usersUnitsDB = new Dictionary<User, List<Unit>>();
        _usersBuildingsDB = new Dictionary<User, List<Building>>();
    }

    public void AddUser(User user)
    {
        if (!_usersUnitsDB.ContainsKey(user))
        {
            _usersUnitsDB.Add(user, new List<Unit>());
        }
    }

    public void AddUnitUser(Unit unit, User user)
    {
        if (_usersUnitsDB.ContainsKey(user))
        {
            _usersUnitsDB[user].Add(unit);
        }
    }

    public User GetUser(string userId)
        => _usersUnitsDB.Keys.FirstOrDefault(u => u.Id == userId) ?? null;


    public List<Unit> GetUnitsOfUserById(string userId)
    {
        var user = _usersUnitsDB.Keys.FirstOrDefault(u => u.Id == userId);
        return user != null ? _usersUnitsDB[user] : null;
    }

    public Unit GetUnitOfUserById(string userId, string unitId)
    {
        var user = _usersUnitsDB.Keys.First(u => u.Id == userId);
        return user != null ? _usersUnitsDB[user].FirstOrDefault(u => u.Id == unitId) ?? null : null;
    }

    public Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock)
    {
        var user = _usersUnitsDB.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDB[user].First(u => u.Id == unitId);
            if (unit == null)
            {
                return null;
            }

            _usersUnitsDB[user].Remove(unit);
            unit.DestinationSystem = unitUpdated.DestinationSystem;
            unit.DestinationPlanet = unitUpdated.DestinationPlanet;
            _usersUnitsDB[user].Add(unit);


            unit.MoveTask = moveUnitBackgroundTask(unit, user, clock);
            unit.LastUpdate = clock.Now;
            return unitUpdated;
        }
        return null;
    }

    private async Task moveUnitBackgroundTask(Unit unit, User user, IClock clock)
    {
        await Task.Run(async () =>
        {
            var tmp = _usersUnitsDB[user].First(u => u.Id == unit.Id);
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
        var user = _usersUnitsDB.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDB[user].Find(u => u.Id == building.BuilderId);
            if (unit != null)
            {
                if (unit.Planet == null || unit.System == null || unit.Type != "builder")
                {
                    return new BadRequestObjectResult("Unit must be on a planet to build");
                }

                building.System = unit.System;
                building.Planet = unit.Planet;
                if (_usersBuildingsDB.ContainsKey(user))
                    _usersBuildingsDB[user].Add(building);
                else
                    _usersBuildingsDB.Add(user, new List<Building> { building });

                return building;
            }
        }
        throw new Exception("User not found");
    }
}