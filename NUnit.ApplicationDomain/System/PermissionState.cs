#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System.Security.Permissions;

/// <summary>
/// A fake PermissionState enum.
/// </summary>
internal enum PermissionState
{
    /// <summary>
    /// The None value.
    /// </summary>
    None = 0,

    /// <summary>
    /// The Unrestricted value.
    /// </summary>
    Unrestricted = 1,
}

#endif