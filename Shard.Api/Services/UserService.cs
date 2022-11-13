using Shard.Api.Models;
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
    Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock);
    Building CreateBuilding(string userId, Building building, IClock clock);
    List<Building> GetBuildingsOfUserById(string userId);
    Building GetBuildingOfUserById(string userId, string buildingId);
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

    public Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDb[user].First(u => u.Id == unitId);
            if (unit == null)
                throw new Exception("Unit not found");

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
}