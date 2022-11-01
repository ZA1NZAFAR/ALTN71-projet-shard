﻿using System.Text.Json.Serialization;

namespace Shard.Api.Models;

public class User
{
    public string Id { get; set; }
    public string Pseudo { get; set; }
    public DateTime DateOfCreation { get; set; }

    [JsonConstructor]
    public User(string id, string pseudo)
    {
        this.Id = id;
        this.Pseudo = pseudo;
        this.DateOfCreation = DateTime.Now;
    }
}