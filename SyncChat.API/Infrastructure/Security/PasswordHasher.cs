using Microsoft.Extensions.Hosting;
using SyncChat.API.Shared.Security.Contracts;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SyncChat.API.Infrastructure.Security;

/// <summary>
/// PasswordHasher is a sealed class to prevent inheritance and ensure that 
/// the hashing logic remains consistent and secure. 
/// 
/// Password hashing is a security-critical operation. Allowing inheritance 
/// could lead to accidental or intentional overrides of core hashing and 
/// verification logic, potentially introducing vulnerabilities.
///
/// If customization is needed, consider implementing a new class 
/// that adheres to a defined interface (e.g., IPasswordHasher) 
/// rather than extending this class.
///
/// This design choice enforces immutability of hashing behavior 
/// and promotes security best practices.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 16 bytes of salt (a random value added to the password before hashing to make hashes unique)

    private const int HashSize = 32; // Final hash will be 32 bytes.

    // number of times the password+salt will be hashed
    // (increases computational cost for attackers - more iterations = safer, but slower)
    private const int Iterations = 500_000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;


    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Why PBKDF2:
        // Because it's a key derivation function specifically designed to be
        // computationally expensive, making brute-force much harder.
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword)) return false;

        string[] parts = hashedPassword.Split('-');

        if (parts.Length != 2) return false;

        try
        {
            byte[] hash = Convert.FromHexString(parts[0]);
            byte[] salt = Convert.FromHexString(parts[1]);

            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

            // Prevents timing attacks
            // (which measure time differences in string comparison to infer data)
            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
        catch
        {
            return false;
        }
    }
}
