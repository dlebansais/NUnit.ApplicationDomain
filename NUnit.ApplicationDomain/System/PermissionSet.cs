#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System.Security;

using NUnit.ApplicationDomain.System.Security.Permissions;

/// <summary>
/// A fake PermissionSet class.
/// </summary>
internal class PermissionSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionSet"/> class.
    /// </summary>
    /// <param name="state">The permission state.</param>
    public PermissionSet(PermissionState state)
    {
    }
}

#endif