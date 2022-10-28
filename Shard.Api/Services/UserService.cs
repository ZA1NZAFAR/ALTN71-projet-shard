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
    object? createBuilding(string userId, Building building);
}

public class UserService : IUserService
{
    private Dictionary<User, List<Vaisseau>> _usersDB;
    private Dictionary<User, List<Building>> _usersBuildingsDB;

    public UserService()
    {
        _usersDB = new Dictionary<User, List<Vaisseau>>();
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
            if (unit != null)
            {
                _usersDB[user].Remove(unit);
                _usersDB[user].Add(vaisseau);
                return vaisseau;
            }
        }

        return null;
    }

    public object? createBuilding(string userId, Building building)
    {
        var user = _usersDB.Keys.First(u => u.id == userId);
        if (user != null)
        {
            var unit = _usersBuildingsDB[user].First(u => u.Id == building.Id);
            if (unit != null)
            {
                _usersBuildingsDB[user].Remove(unit);
                _usersBuildingsDB[user].Add(building);
                return building;
            }
        }

        return null;
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