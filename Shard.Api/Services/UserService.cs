using System.Web.WebPages;
using JetBrains.ReSharper.TestRunner.Abstractions.Extensions;
using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface IUserService
{
    public void addUser(User user);
    public void addVaisseauUser(Vaisseau vaisseau, User user);
    User getUser(string userId);
    List<Vaisseau> getUnitsOfUserById(string userId);
    Vaisseau getUnitOfUserById(string userId, string unitId);
    Vaisseau? updateUnitOfUserById(string userId, string unitId, Vaisseau vaisseau, IClock clock);
    ActionResult<Building> createBuilding(string userId, Building building);
}

public class UserService : IUserService
{
    private Dictionary<User, List<Vaisseau>> _usersDB;
    private Dictionary<User, List<Building>> _usersBuildingsDB;

    public UserService()
    {
        _usersDB = new Dictionary<User, List<Vaisseau>>();
        _usersBuildingsDB = new Dictionary<User, List<Building>>();
    }

    public void addUser(User user)
    {
        if (!_usersDB.ContainsKey(user))
        {
            _usersDB.Add(user, new List<Vaisseau>());
        }
    }

    public void addVaisseauUser(Vaisseau vaisseau, User user)
    {
        if (_usersDB.ContainsKey(user))
        {
            _usersDB[user].Add(vaisseau);
        }
    }

    public User getUser(string userId)
        => _usersDB.Keys.FirstOrDefault(u => u.id == userId) ?? null;


    public List<Vaisseau> getUnitsOfUserById(string userId)
    {
        var user = _usersDB.Keys.FirstOrDefault(u => u.id == userId);
        return user != null ? _usersDB[user] : null;
    }

    public Vaisseau getUnitOfUserById(string userId, string unitId)
    {
        var user = _usersDB.Keys.First(u => u.id == userId);
        return user != null ? _usersDB[user].FirstOrDefault(u => u.id == unitId) ?? null : null;
    }

    public Vaisseau? updateUnitOfUserById(string userId, string unitId, Vaisseau vaisseau, IClock clock)
    {
        var user = _usersDB.Keys.First(u => u.id == userId);
        if (user != null)
        {
            var unit = _usersDB[user].First(u => u.id == unitId);
            if (unit == null)
            {
                return null;
            }

            _usersDB[user].Remove(unit);
            unit.destinationSystem = vaisseau.destinationSystem;
            unit.destinationPlanet = vaisseau.destinationPlanet;
            _usersDB[user].Add(unit);

            moveUnitBackgroundTask(unit, user, clock);
            return vaisseau;
        }

        return null;
    }

    //background async unit move
    public async Task moveUnitBackgroundTask(Vaisseau unit, User user, IClock clock)
    {
        await Task.Run(() =>
        {
            var tmp = _usersDB[user].First(u => u.id == unit.id);
            if ((tmp.system.IsNullOrEmpty()) ||
                (!tmp.destinationSystem.IsNullOrEmpty() && !tmp.system.Equals(tmp.destinationPlanet))
               )
            {
                clock.Delay(60000);
                tmp.system = tmp.destinationSystem;
                tmp.destinationSystem = null;
            }

            if (tmp.planet.IsNullOrEmpty() ||
                (!tmp.destinationPlanet.IsNullOrEmpty() && !tmp.planet.Equals(tmp.destinationPlanet)))
            {
                clock.Delay(15000);
                tmp.planet = tmp.destinationPlanet;
                tmp.destinationPlanet = null;
            }
        });
    }

    public ActionResult<Building> createBuilding(string userId, Building building)
    {
        var user = _usersDB.Keys.First(u => u.id == userId);
        if (user != null)
        {
            var unit = _usersDB[user].Find(u => u.id == building.BuilderId);
            if (unit != null)
            {
                if (unit.planet.IsNullOrEmpty() || unit.system.IsNullOrEmpty() || unit.type != "builder")
                {
                    return new BadRequestObjectResult("Unit must be on a planet to build");
                }

                building.System = unit.system;
                building.Planet = unit.planet;
                if (_usersBuildingsDB.ContainsKey(user))
                    _usersBuildingsDB[user].Add(building);
                else
                    _usersBuildingsDB.Add(user, new List<Building> { building });

                return building;
            }
        }

        throw new Exception("User not found");
    }


    // private void updateAllUnitLocations()
    // {
    //     _usersDB.Values.ToList().ForEach(vaisseaux =>
    //     {
    //         vaisseaux.ForEach(vaisseau =>
    //         {
    //             if (!vaisseau.system.Equals(vaisseau.destinationSystem))
    //             {
    //                 if (DateTime.Now - vaisseau.lastUpdate >= TimeSpan.FromMinutes(1))
    //                 {
    //                     vaisseau.system = vaisseau.destinationPlanet;
    //                     vaisseau.destinationPlanet = null;
    //                     vaisseau.lastUpdate = DateTime.Now;
    //                 }
    //             }
    //             else if (!vaisseau.planet.Equals(vaisseau.destinationPlanet))
    //             {
    //                 if (DateTime.Now - vaisseau.lastUpdate >= TimeSpan.FromSeconds(15))
    //                 {
    //                     vaisseau.planet = vaisseau.destinationPlanet;
    //                     vaisseau.destinationPlanet = null;
    //                     vaisseau.lastUpdate = DateTime.Now;
    //                 }
    //             }
    //         });
    //     });
    // }
}