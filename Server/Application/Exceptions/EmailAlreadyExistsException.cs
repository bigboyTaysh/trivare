namespace Trivare.Application.Exceptions;

/// <summary>
/// Exception thrown when attempting to register with an email that already exists
/// </summary>
public class EmailAlreadyExistsException : Exception
{
    /// <summary>
    /// The email address that already exists
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Initializes a new instance of the EmailAlreadyExistsException
    /// </summary>
    /// <param name="email">The email address that already exists</param>
    public EmailAlreadyExistsException(string email) 
        : base($"An account with email '{email}' already exists")
    {
        Email = email;
    }
}
