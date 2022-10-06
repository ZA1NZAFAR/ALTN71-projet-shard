using System.Net;
using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Api.Services;

namespace Shard.Api.Controllers;

public class UserController
{
    private readonly ICelestialService _celestialService;
    private readonly IUserService _userService;

    public UserController(ICelestialService celestialService, IUserService userService)
    {
        _celestialService = celestialService;
        _userService = userService;
    }

    [HttpPut("users/{userId}")]
    [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(User), (int)HttpStatusCode.NotFound)]
    public ActionResult<User> createUser(string userId, [FromBody] User user)
    {
        _userService.addUser(user);
        _userService.addVaisseauUser(new Vaisseau("Scout", _celestialService.getRandomSystem()), user);

        Console.WriteLine("User created : " + user);
        Console.WriteLine("User created : " + _userService.getAllUsers());

        return user;
    }


    [HttpGet("users/{userId}")]
    public ActionResult<User> getUser(string userId)
    {
        var res = _userService.getUser(userId);
        if (res == null)
        {
            return new NotFoundResult();
        }
        return res;
    }
}