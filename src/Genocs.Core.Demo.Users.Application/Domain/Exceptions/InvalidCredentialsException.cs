namespace Genocs.Core.Demo.Users.Application.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public string Email { get; }

    public InvalidCredentialsException(string email) : base("Invalid credentials.")
    {
        Email = email;
    }
}