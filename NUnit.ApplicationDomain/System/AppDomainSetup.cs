#if NET8_0_OR_GREATER

namespace NUnit.ApplicationDomain.System;

using global::System;

/// <summary>
/// A fake AppDomainSetup.
/// </summary>
internal class AppDomainSetup : MarshalByRefObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomainSetup"/> class.
    /// </summary>
    internal AppDomainSetup()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomainSetup"/> class.
    /// </summary>
    /// <param name="applicationBase">The application base.</param>
    /// <param name="configurationFile">The configuration file.</param>
    public AppDomainSetup(string? applicationBase, string? configurationFile)
    {
        ApplicationBase = applicationBase;
        ConfigurationFile = configurationFile;
    }

    /// <summary>
    /// Gets or sets the application base.
    /// </summary>
    public string? ApplicationBase { get; set; }

    /// <summary>
    /// Gets or sets the configuration file.
    /// </summary>
    public string? ConfigurationFile { get; set; }
}

#endif