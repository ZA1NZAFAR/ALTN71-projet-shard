namespace Shard.Api.Models.Exceptions;

public class IsUnauthorizedException : Exception
{
    public IsUnauthorizedException()
    {
    }
    
    public IsUnauthorizedException(string message) : base(message)
    {
    }
}

