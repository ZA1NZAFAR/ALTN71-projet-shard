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
}

public class UserService : IUserService
{
    private Dictionary<User, Vaisseau> _usersDB;

    public UserService()
    {
        _usersDB = new Dictionary<User, Vaisseau>();
    }

    public void addUser(User user)
    {
        if (!_usersDB.ContainsKey(user))
        {
            _usersDB.Add(user, null);
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
            _usersDB[user] = vaisseau;
        }
    }

    public void addVaisseauUser(Vaisseau vaisseau, string id)
    {
        var user = _usersDB.Keys.FirstOrDefault(u => u.id == id);
        if (user != null)
        {
            _usersDB[user] = vaisseau;
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
}