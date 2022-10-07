using Shard.Api.Models;

namespace Shard.Api.Services;

public interface IUserService
{
    public void addUser(User user);
    public void deleteUser(User user);
    public void deleteUser(string id);
    public void addVaisseauUser(Vaisseau vaisseau, User user);
    public void addVaisseauUser(Vaisseau vaisseau, string id);
    List<User> getAllUsers();
    User getUser(string userId);
    List<Vaisseau> getUnitsOfUserById(string userId);
    object getUnitOfUserById(string userId, string unitId);
    object? updateUnitOfUserById(string userId, string unitId, Vaisseau vaisseau);
}

public class UserService : IUserService
{
    private Dictionary<User, List<Vaisseau>> _usersDB;

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

    public void deleteUser(User user)
    {
        if (_usersDB.ContainsKey(user))
        {
            _usersDB.Remove(user);
        }
    }

    public void deleteUser(string id)
    {
        var user = _usersDB.Keys.FirstOrDefault(u => u.id == id);
        if (user != null)
        {
            _usersDB.Remove(user);
        }
    }

    public void addVaisseauUser(Vaisseau vaisseau, User user)
    {
        if (_usersDB.ContainsKey(user))
        {
            _usersDB[user].Add(vaisseau);
        }
    }

    public void addVaisseauUser(Vaisseau vaisseau, string id)
    {
        var user = _usersDB.Keys.FirstOrDefault(u => u.id == id);
        if (user != null)
        {
            _usersDB[user].Add(vaisseau);
        }
    }

    public List<User> getAllUsers()
    {
        return _usersDB.Keys.ToList();
    }

    public User getUser(string userId)
    {
        //find and return user or return null
        return _usersDB.Keys.FirstOrDefault(u => u.id == userId) ?? null;
    }
    
    public List<Vaisseau> getUnitsOfUserById(string userId)
    {
        var user = _usersDB.Keys.FirstOrDefault(u => u.id == userId);
        if (user != null)
        {
            return _usersDB[user];
        }
        return null;
    }
    
    public object getUnitOfUserById(string userId, string unitId)
    {
        var user = _usersDB.Keys.First(u => u.id == userId);
        if (user != null)
        {
            return _usersDB[user].FirstOrDefault(u => u.id == unitId) ?? null;
        }
        return null;
    }
    
    public object? updateUnitOfUserById(string userId, string unitId, Vaisseau vaisseau)
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
}