using SyncChat.API.Shared.Security.Contracts;

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

}
