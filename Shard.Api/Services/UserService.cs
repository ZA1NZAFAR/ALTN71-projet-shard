using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Api.Models.Exceptions;
using Shard.Api.Tools;
using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface IUserService
{
    public void AddUser(User user);
    public void AddUnitUser(Unit unit, User user);
    User? GetUser(string userId);
    List<Unit>? GetUnitsOfUserById(string userId);
    Unit? GetUnitOfUserById(string userId, string unitId);
    Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock,bool isAuthenticated);
    Building CreateBuilding(string userId, Building building, IClock clock);
    List<Building> GetBuildingsOfUserById(string userId);
    Building GetBuildingOfUserById(string userId, string buildingId);
    bool ifExistThenUpdateUser(User user);
    Unit AddToQueue(string userId, string starportId, UnitType unit, IClock clock);
}

public class UserService : IUserService
{
    // Units storage
    private readonly Dictionary<User, List<Unit>> _usersUnitsDb;

    // Buildings storage
    private readonly Dictionary<User, List<Building>> _usersBuildingsDb;

    public UserService()
    {
        _usersUnitsDb = new Dictionary<User, List<Unit>>();
        _usersBuildingsDb = new Dictionary<User, List<Building>>();
    }

    public void AddUser(User user)
    {
        if (!_usersUnitsDb.ContainsKey(user))
            _usersUnitsDb.Add(user, new List<Unit>());
    }

    public void AddUnitUser(Unit unit, User user)
    {
        if (_usersUnitsDb.ContainsKey(user))
            _usersUnitsDb[user].Add(unit);
    }

    public User? GetUser(string userId)
        => _usersUnitsDb.Keys.FirstOrDefault(u => u.Id == userId) ?? null;

    public List<Unit>? GetUnitsOfUserById(string userId)
    {
        var user = _usersUnitsDb.Keys.FirstOrDefault(u => u.Id == userId);
        return user != null ? _usersUnitsDb[user] : null;
    }

    public Unit? GetUnitOfUserById(string userId, string unitId)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        return user != null ? _usersUnitsDb[user].FirstOrDefault(u => u.Id == unitId) ?? null : null;
    }

    public Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock, bool isAuthenticated)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDb[user].FirstOrDefault(u => u.Id == unitId);
            if (unit == null)
            {
                if (isAuthenticated)
                {
                    _usersUnitsDb[user].Add(unitUpdated);
                    return unitUpdated;
                }

                throw new IsUnauthorizedException();
            }

            unit.DestinationSystem = unitUpdated.DestinationSystem;
            unit.DestinationPlanet = unitUpdated.DestinationPlanet;

            // start the travel in the background
            unit.MoveTask = BackGroundTasks.MoveUnitBackgroundTask(unit, user, clock, _usersUnitsDb, _usersBuildingsDb);
            return unitUpdated;
        }

        throw new Exception("User not found");
    }

    public Building CreateBuilding(string userId, Building building, IClock clock)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDb[user].Find(u => u.Id == building.BuilderId);
            if (unit != null)
            {
                // only a "builder" situated on a "planet" in a "system" can build a building
                if (unit.Type != "builder" || unit.Planet == null || unit.System == null)
                    throw new Exception("Unit is not a builder");

                building.System = unit.System;
                building.Planet = unit.Planet;

                // building id could be null
                if (building.Id == null)
                {
                    building.Id = Guid.NewGuid().ToString();
                }

                building.EstimatedBuildTime = clock.Now.AddMinutes(5);

                if (_usersBuildingsDb.ContainsKey(user))
                    _usersBuildingsDb[user].Add(building);
                else
                    _usersBuildingsDb.Add(user, new List<Building> { building });

                // start building task
                building.BuildTask =
                    BackGroundTasks.BuildBuildingBackgroundTask(building, user, clock, _usersBuildingsDb);

                return building;
            }
        }

        throw new Exception("User not found");
    }

    public List<Building> GetBuildingsOfUserById(string userId)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (_usersBuildingsDb.ContainsKey(user))
            return _usersBuildingsDb[user];
        throw new Exception("No buildings found");
    }

    public Building GetBuildingOfUserById(string userId, string buildingId)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user == null)
            throw new Exception("User not found");
        if (_usersBuildingsDb.ContainsKey(user))
            return _usersBuildingsDb[user].FirstOrDefault(u => u.Id == buildingId) ??
                   throw new InvalidOperationException();
        throw new Exception("No buildings found");
    }

    public bool ifExistThenUpdateUser(User user)
    {
        var userFound = _usersUnitsDb.Keys.FirstOrDefault(u => u.Id == user.Id);
        if (userFound != null)
        {
            userFound.ResourcesQuantity = user.ResourcesQuantity;
            return true;
        }

        return false;
    }

    public Unit AddToQueue(string userId, string starportId, UnitType unit, IClock clock)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user == null)
            throw new Exception("User not found");
        if (_usersBuildingsDb.ContainsKey(user))
        {
            var starport = _usersBuildingsDb[user].FirstOrDefault(u => u.Id == starportId);
            if (starport == null)
                throw new Exception("Starport not found");

            if (starport.Type != "starport")
                throw new Exception("Building is not a starport");

            if (starport.System == null || starport.Planet == null)
                throw new Exception("Starport is not situated on a planet in a system");

            if (!starport.IsBuilt)
                throw new Exception("Starport is not built yet");

            // could be useful later
            // if (starport.Queue == null)
            //     starport.Queue = new List<Unit>();
            // if (starport.Queue.Count >= 5)
            //     throw new Exception("Starport queue is full");
            // if (starport.Queue.Any(u => u.Type == unit.Type))
            //     throw new Exception("Unit already in queue");
            // if (starport.EstimatedBuildTime != null)
            //     throw new Exception("User is already building a unit");
            
            if (user.ResourcesQuantity == null)
                throw new Exception("Starport has no resources");

            if (SwissKnife.GetUnitCost(unit.Type)
                .Any(resource => user.ResourcesQuantity[resource.Key] < resource.Value))
            {
                throw new Exception("Starport has not enough resources");
            }


            //starport.Queue.Add(unit);

            foreach (var resource in SwissKnife.GetUnitCost(unit.Type))
            {
                user.ResourcesQuantity[resource.Key] -= resource.Value;
            }

            var unitToAdd = new Unit(Guid.NewGuid().ToString(), unit.Type, starport.System, starport.Planet);
            
            _usersUnitsDb[user].Add(unitToAdd);

            return unitToAdd;
        }

        throw new Exception("No buildings found");
    }
}