using System.Security.Cryptography;
using CarRentalExamen.Core.Interfaces;

namespace CarRentalExamen.Infrastructure.Services;

/// <summary>
/// Secure password hasher using PBKDF2 with salt
/// Follows .NET security best practices
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // OWASP recommended minimum

    /// <summary>
    /// Hashes a password using PBKDF2 with a random salt
    /// Format: {iterations}.{salt}.{hash}
    /// </summary>
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    public bool Verify(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        
        // Handle legacy SHA256 hashes (no dots = old format)
        if (parts.Length != 3)
        {
            // Fallback for old SHA256 hashes during migration
            return VerifyLegacySha256(password, hashedPassword);
        }

        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var storedHash = Convert.FromBase64String(parts[2]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            storedHash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }

    /// <summary>
    /// Legacy verification for old SHA256 hashes (migration support)
    /// </summary>
    private static bool VerifyLegacySha256(string password, string hash)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        var computed = Convert.ToBase64String(bytes);
        return computed == hash;
    }

    // Static methods for backward compatibility during migration
    public static string HashStatic(string password) => new PasswordHasher().Hash(password);
    public static bool VerifyStatic(string password, string hash) => new PasswordHasher().Verify(password, hash);
}
