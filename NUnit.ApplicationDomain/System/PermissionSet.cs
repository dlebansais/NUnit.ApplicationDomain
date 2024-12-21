#pragma warning disable IDE0060 // Remove unused parameter.
#pragma warning disable CS9113 // Parameter is unread.

#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System.Security;

using NUnit.ApplicationDomain.System.Security.Permissions;

/// <summary>
/// A fake PermissionSet class.
/// </summary>
/// <param name="state">The permission state.</param>
internal class PermissionSet(PermissionState state)
{
}

#endif