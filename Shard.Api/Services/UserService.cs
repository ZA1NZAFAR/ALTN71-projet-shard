﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Api.Tools;
using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface IUserService
{
    public void SetCelestialService(ICelestialService celestialService);
    public void AddUser(User user);
    public void AddUnitUser(Unit unit, User user);
    User GetUser(string userId);
    List<Unit> GetUnitsOfUserById(string userId);
    Unit GetUnitOfUserById(string userId, string unitId);
    Unit? UpdateUnitOfUserById(string userId, string unitId, Unit unitUpdated, IClock clock);
    ActionResult<Building> CreateBuilding(string userId, Building building, IClock clock);
    List<Building> GetBuildingsOfUserById(string userId);
    Building GetBuildingOfUserById(string userId, string buildingId);
    bool unitExists(string buildingBuilderId);
}

public class UserService : IUserService
{
    private readonly Dictionary<User, List<Unit>> _usersUnitsDb;
    private readonly Dictionary<User, List<Building>> _usersBuildingsDb;
    private ICelestialService _celestialService;

    public void SetCelestialService(ICelestialService celestialService)
    {
        _celestialService = celestialService;
    }

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

    public void checkAndRemoveOngoingBuildingsIfChangePlanet(string userId, string unitId)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        var unit = _usersUnitsDb[user].FirstOrDefault(u => u.Id == unitId);
        if (_usersBuildingsDb.ContainsKey(user))
        {
            var buildingsToRemove = _usersBuildingsDb[user].Where(b => b.BuilderId == unitId).ToList();
            foreach (var building in buildingsToRemove)
            {
                if ((unit.Planet != unit.DestinationPlanet && unit.Planet == building.Planet &&
                     !building.IsBuilt) || (unit.System != unit.DestinationSystem && unit.System == building.System &&
                                            !building.IsBuilt))
                {
                    _usersBuildingsDb[user].Remove(building);
                }
            }
        }
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
            checkAndRemoveOngoingBuildingsIfChangePlanet(user.Id, unit.Id);
            var tmp = _usersUnitsDb[user].First(u => u.Id == unit.Id);
            if ((tmp.System == null && tmp.DestinationSystem != null) ||
                (tmp.DestinationSystem != null && !tmp.System.Equals(tmp.DestinationSystem))
               )
            {
                tmp.ETA += 60000;
                await clock.Delay(60000);
                tmp.System = tmp.DestinationSystem;
                tmp.DestinationSystem = null;
                tmp.LastUpdate = clock.Now;
            }

            if ((tmp.Planet == null && tmp.DestinationPlanet != null) ||
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

    public ActionResult<Building> CreateBuilding(string userId, Building building, IClock clock)
    {
        var user = _usersUnitsDb.Keys.First(u => u.Id == userId);
        if (user != null)
        {
            var unit = _usersUnitsDb[user].Find(u => u.Id == building.BuilderId);
            if (unit != null)
            {
                if (unit.Type != "builder" || unit.Planet == null || unit.System == null)
                {
                    throw new Exception("Unit is not a builder");
                }

                building.System = unit.System;
                building.Planet = unit.Planet;
                if (building.Id == null)
                {
                    building.Id = Guid.NewGuid().ToString();
                }

                building.EstimatedBuildTime = clock.Now.AddMinutes(5);

                if (_usersBuildingsDb.ContainsKey(user))
                    _usersBuildingsDb[user].Add(building);
                else
                    _usersBuildingsDb.Add(user, new List<Building> { building });
                building.BuildTask = BuildBuildingBackgroundTask(building, user, clock);


                return building;
            }
        }

        throw new Exception("User not found");
    }

    private async Task BuildBuildingBackgroundTask(Building building, User user, IClock clock)
    {
        await Task.Run(async () =>
        {
            var tmp = _usersBuildingsDb[user].First(u => u.Id == building.Id);
            if (tmp != null)
            {
                await clock.Delay(TimeSpan.FromMinutes(5));
                building.IsBuilt = true;
                building.EstimatedBuildTime = null;
                building.BuildTask = null;
                building.creationTime = clock.Now;
                // building.MiningTask = MineBuildingBackgroundTask(building, user, clock);
            }
        });
    }

    // private async Task MineBuildingBackgroundTask(Building building, User user, IClock clock)
    // {
    //     await Task.Run(async () =>
    //     {
    //         var planet = _celestialService.GetPlanetOfSystem(building.System, building.Planet);
    //         var resourceKind = SwissKnife.getResourceKindFromString(building.ResourceCategory);
    //         while (true)
    //         {
    //             if (clock.Now - building.lastExtraction >= TimeSpan.FromMinutes(1))
    //             {
    //                 if (planet.ResourceQuantity.ContainsKey(resourceKind))
    //                 {
    //                     if (planet.ResourceQuantity[resourceKind] > 0)
    //                     {
    //                         user.ResourcesQuantity[resourceKind] += 1;
    //                     }
    //                 }
    //
    //                 building.lastExtraction = clock.Now;
    //             }
    //             
    //             await clock.Delay(TimeSpan.FromMinutes(1));
    //         }
    //     });
    // }

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
            return _usersBuildingsDb[user].FirstOrDefault(u => u.Id == buildingId);
        throw new Exception();
    }

    public bool unitExists(string buildingBuilderId)
    {
        return _usersUnitsDb.Values.Any(u => u.Any(u => u.Id == buildingBuilderId));
    }
}