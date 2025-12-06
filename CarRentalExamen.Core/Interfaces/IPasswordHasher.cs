namespace CarRentalExamen.Core.Interfaces;

/// <summary>
/// Interface for password hashing operations - follows Dependency Inversion Principle
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password with a random salt
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    bool Verify(string password, string hash);
}
